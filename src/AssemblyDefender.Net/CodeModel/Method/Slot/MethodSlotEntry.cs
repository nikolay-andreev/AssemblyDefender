using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public class MethodSlotEntry
	{
		#region Fields

		private int _depth;
		private MethodSlot _slot;
		private IMethod _method;
		private MethodSlotEntry _baseMethod;
		private MethodSlotEntry _superMethod;
		private IReadOnlyList<IMethod> _interfaceMethods;

		#endregion

		#region Ctors

		internal MethodSlotEntry(IMethod method, int depth)
		{
			_method = method;
			_depth = depth;
		}

		#endregion

		#region Properties

		public int Depth
		{
			get { return _depth; }
		}

		public MethodSlot Slot
		{
			get { return _slot; }
			internal set { _slot = value; }
		}

		public IMethod Method
		{
			get { return _method; }
		}

		public MethodSlotEntry BaseMethod
		{
			get { return _baseMethod; }
			internal set { _baseMethod = value; }
		}

		public MethodSlotEntry SuperMethod
		{
			get { return _superMethod; }
			internal set { _superMethod = value; }
		}

		public MethodSlotEntry TopMethod
		{
			get
			{
				var method = this;
				while (true)
				{
					if (method.SuperMethod == null)
						break;

					method = method.SuperMethod;
				}

				return method;
			}
		}

		public MethodSlotEntry BottomMethod
		{
			get
			{
				var method = this;
				while (true)
				{
					if (method.BaseMethod == null)
						break;

					method = method.BaseMethod;
				}

				return method;
			}
		}

		public IReadOnlyList<IMethod> InterfaceMethods
		{
			get
			{
				if (_interfaceMethods == null)
				{
					_interfaceMethods = ReadOnlyList<IMethod>.Empty;
				}

				return _interfaceMethods;
			}
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return _method.ToString();
		}

		/// <summary>
		/// Each slot method usually maps to one interface method. In rare occasions map to more.
		/// </summary>
		internal void AddInterfaceMethod(IMethod method)
		{
			IMethod[] methods;
			if (_interfaceMethods == null)
			{
				methods = new IMethod[] { method };
			}
			else
			{
				int count = _interfaceMethods.Count;
				methods = new IMethod[count + 1];
				for (int i = 0; i < count; i++)
				{
					methods[i] = _interfaceMethods[i];
				}

				methods[count] = method;
			}

			_interfaceMethods = new ReadOnlyList<IMethod>(methods);
		}

		#endregion
	}
}
