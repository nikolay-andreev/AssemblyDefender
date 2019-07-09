using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public class MethodSlotList : IEnumerable<MethodSlot>
	{
		#region Fields

		private int _maxDepth;
		private IType _type;
		private List<MethodSlot> _slots = new List<MethodSlot>();
		private Dictionary<IMethod, MethodSlotEntry> _methods = new Dictionary<IMethod, MethodSlotEntry>();

		#endregion

		#region Ctors

		public MethodSlotList(IType type, bool skipInterfaces = false)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (type.IsInterface)
				throw new InvalidOperationException();

			_type = type;

			CreateSlots();
			ApplyOverrides();

			if (!skipInterfaces)
			{
				LoadInterfaces();
			}
		}

		#endregion

		#region Properties

		public MethodSlot this[int index]
		{
			get { return _slots[index]; }
		}

		public int Count
		{
			get { return _slots.Count; }
		}

		public int MaxDepth
		{
			get { return _maxDepth; }
		}

		public IType Type
		{
			get { return _type; }
		}

		#endregion

		#region Methods

		public MethodSlotEntry GetMethod(IMethod method)
		{
			MethodSlotEntry slotMethod;
			_methods.TryGetValue(method, out slotMethod);

			return slotMethod;
		}

		public MethodSlotEntry GetMethod(IMethod method, bool throwOnFailure)
		{
			var slotMethod = GetMethod(method);
			if (slotMethod == null)
			{
				if (throwOnFailure)
				{
					throw new CodeModelException(string.Format(SR.MethodNotFound2, method.ToString()));
				}

				return null;
			}

			return slotMethod;
		}

		public MethodSlotEntry GetTargetMethod(IMethod callMethod)
		{
			var slotMethod = GetMethod(callMethod);
			if (slotMethod == null)
				return null;

			return slotMethod.Slot.MainMethod;
		}

		public MethodSlotEntry GetTargetMethod(IMethod callMethod, bool throwOnFailure)
		{
			var slotMethod = GetTargetMethod(callMethod);
			if (slotMethod == null)
			{
				if (throwOnFailure)
				{
					throw new CodeModelException(string.Format(SR.MethodNotFound2, callMethod.ToString()));
				}

				return null;
			}

			return slotMethod;
		}

		public IEnumerator<MethodSlot> GetEnumerator()
		{
			return _slots.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void CreateSlots()
		{
			int depth = 0;
			var type = _type;
			while (type != null)
			{
				foreach (var method in type.Methods)
				{
					// Method is already part of a slot.
					if (_methods.ContainsKey(method))
						continue;

					var slot = CreateSlot(method, depth);
					_slots.Add(slot);
				}

				type = type.BaseType;
				depth++;
			}
		}

		private MethodSlot CreateSlot(IMethod method, int depth)
		{
			var slot = new MethodSlot(this);

			var mainSlotMethod = new MethodSlotEntry(method, depth);
			_methods.Add(method, mainSlotMethod);

			if (_maxDepth < depth)
				_maxDepth = depth;

			var slotMethod = mainSlotMethod;

			method = method.GetBaseMethod(ref depth);
			while (method != null)
			{
				var baseSlotMethod = new MethodSlotEntry(method, depth);
				_methods.Add(method, baseSlotMethod);

				if (_maxDepth < depth)
					_maxDepth = depth;

				slotMethod.BaseMethod = baseSlotMethod;
				baseSlotMethod.SuperMethod = slotMethod;

				slotMethod = baseSlotMethod;

				method = method.GetBaseMethod(ref depth);
			}

			slot.Add(mainSlotMethod);

			return slot;
		}

		private void ApplyOverrides()
		{
			var queue = new PriorityQueue<int, MethodSlotEntry>();

			// Load overrides.
			{
				var type = _type;
				while (type != null)
				{
					foreach (var method in type.Methods)
					{
						if (method.Overrides.Count == 0)
							continue;

						MethodSlotEntry slotMethod;
						if (!_methods.TryGetValue(method, out slotMethod))
							continue;

						queue.Enqueue(slotMethod.Depth, slotMethod);
					}

					type = type.BaseType;
				}
			}

			// Process queue.
			while (queue.Count > 0)
			{
				var slotMethod = queue.Dequeue();
				var method = slotMethod.Method;

				foreach (var overridenIMethod in method.Overrides)
				{
					if (overridenIMethod.Owner.IsInterface)
						continue;

					MethodSlotEntry overridenSlotMethod;
					if (!_methods.TryGetValue(overridenIMethod, out overridenSlotMethod))
						continue;

					var overridenSlot = overridenSlotMethod.Slot;
					var overridenSlotTopMethod = overridenSlotMethod.TopMethod;

					// Slot is already processed.
					if (overridenSlot == slotMethod.Slot)
						continue;

					// Slot is relocated.
					if (overridenSlotTopMethod != overridenSlot.MainMethod)
						continue;

					// Overridden slot method is in higher type.
					if (overridenSlotTopMethod.Depth < slotMethod.Depth)
						continue;

					if (slotMethod.SuperMethod != null)
					{
						// Base method of root slot. Do not process root overrides.
						if (overridenSlotTopMethod.Depth == 0)
							continue;
					}

					slotMethod.Slot.Add(overridenSlot);
				}
			}

			// Remove relocated slots.
			for (int i = _slots.Count - 1; i >= 0; i--)
			{
				var slot = _slots[i];
				if (slot.MainMethod == null)
				{
					_slots.RemoveAt(i);
				}
			}
		}

		private void LoadInterfaces()
		{
			LoadInterfaces(_type);
		}

		private void LoadInterfaces(IType type)
		{
			// Apply interface overrides.
			foreach (var method in type.Methods)
			{
				foreach (var overridenMethod in method.Overrides)
				{
					if (!overridenMethod.Owner.IsInterface)
						continue;

					if (_methods.ContainsKey(overridenMethod))
						continue;

					MethodSlotEntry slotMethod;
					if (!_methods.TryGetValue(method, out slotMethod))
					{
						throw new CodeModelException(string.Format(SR.MethodNotFound2, method.ToString()));
					}

					slotMethod.AddInterfaceMethod(overridenMethod);
					_methods.Add(overridenMethod, slotMethod);
				}
			}

			// Map root interfaces to current type.
			foreach (var interfaceType in type.Interfaces)
			{
				foreach (var interfaceMethod in interfaceType.Methods)
				{
					// Interface method is overriden.
					if (_methods.ContainsKey(interfaceMethod))
						continue;

					var targetMethod = type.Methods.FindPolymorphic(interfaceMethod);
					if (targetMethod == null)
						continue;

					MethodSlotEntry slotMethod;
					if (!_methods.TryGetValue(targetMethod, out slotMethod))
					{
						throw new CodeModelException(string.Format(SR.MethodNotFound2, targetMethod.ToString()));
					}

					slotMethod.AddInterfaceMethod(interfaceMethod);
					_methods.Add(interfaceMethod, slotMethod);
				}
			}

			// Process base types
			if (type.BaseType != null)
			{
				LoadInterfaces(type.BaseType);
			}

			// Map sub interfaces.
			foreach (var interfaceType in type.Interfaces)
			{
				MapInterfaces(type, interfaceType);
			}
		}

		private void MapInterfaces(IType type, IType interfaceType)
		{
			foreach (var interfaceMethod in interfaceType.Methods)
			{
				// Interface method is overridden.
				if (_methods.ContainsKey(interfaceMethod))
					continue;

				var targetMethod = FindInterfaceMethod(type, interfaceMethod);
				if (targetMethod == null)
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, ((Module)type.Module).Location));
				}

				MethodSlotEntry targetSlotMethod;
				if (!_methods.TryGetValue(targetMethod, out targetSlotMethod))
				{
					throw new CodeModelException(string.Format(SR.MethodNotFound2, targetMethod.ToString()));
				}

				targetSlotMethod.AddInterfaceMethod(interfaceMethod);
				_methods.Add(interfaceMethod, targetSlotMethod);
			}

			// Map sub interfaces.
			foreach (var subInterfaceType in interfaceType.Interfaces)
			{
				MapInterfaces(type, subInterfaceType);
			}
		}

		private IMethod FindInterfaceMethod(IType type, IMethod interfaceMethod)
		{
			while (type != null)
			{
				var method = type.Methods.FindPolymorphic(interfaceMethod);
				if (method != null)
					return method;

				type = type.BaseType;
			}

			return null;
		}

		#endregion
	}
}
