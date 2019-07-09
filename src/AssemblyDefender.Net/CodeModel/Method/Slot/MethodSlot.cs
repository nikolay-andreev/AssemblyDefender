using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public class MethodSlot : IEnumerable<MethodSlotEntry>
	{
		#region Fields

		private MethodSlotList _owner;
		private MethodSlotEntry _mainMethod;
		private List<MethodSlotEntry> _methods = new List<MethodSlotEntry>();

		#endregion

		#region Ctors

		internal MethodSlot(MethodSlotList owner)
		{
			_owner = owner;
		}

		#endregion

		#region Properties

		public MethodSlotEntry this[int index]
		{
			get { return _methods[index]; }
		}

		public int Count
		{
			get { return _methods.Count; }
		}

		/// <summary>
		/// Main slot method.
		/// </summary>
		public MethodSlotEntry MainMethod
		{
			get { return _mainMethod; }
		}

		public MethodSlotList Owner
		{
			get { return _owner; }
		}

		#endregion

		#region Methods

		public IEnumerator<MethodSlotEntry> GetEnumerator()
		{
			return _methods.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override string ToString()
		{
			return string.Format("Main: {0}; Count: {1}",
				_mainMethod != null ? _mainMethod.ToString() : "<Null>",
				_methods.Count);
		}

		internal void Add(MethodSlotEntry method)
		{
			InitSlot(method);

			if (_methods.Count == 0)
			{
				_mainMethod = method;
			}

			_methods.Add(method);
		}

		internal void Add(MethodSlot slot)
		{
			foreach (var method in slot._methods)
			{
				InitSlot(method);
				_methods.Add(method);
			}

			slot._mainMethod = null;
			slot._methods.Clear();
		}

		private void InitSlot(MethodSlotEntry method)
		{
			if (method.SuperMethod != null)
			{
				throw new InvalidOperationException();
			}

			while (method != null)
			{
				method.Slot = this;
				method = method.BaseMethod;
			}
		}

		#endregion
	}
}
