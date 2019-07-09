using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class GenericParameterType : TypeSignature
	{
		#region Fields

		private bool _isMethod;
		private int _position;

		#endregion

		#region Ctors

		public GenericParameterType(bool isMethod, int position)
		{
			_isMethod = isMethod;
			_position = position;
		}

		public GenericParameterType(System.Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			Type[] genericArguments;
			if (type.DeclaringMethod != null)
			{
				_isMethod = true;
				genericArguments = type.DeclaringMethod.GetGenericArguments();
			}
			else
			{
				_isMethod = false;
				genericArguments = type.DeclaringType.GetGenericArguments();
			}

			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (genericArguments[i] == type)
				{
					_position = i;
					break;
				}
			}
		}

		#endregion

		#region Properties

		public override string Name
		{
			get { return (_isMethod ? "!!" : "!") + _position.ToString(); }
		}

		public bool IsMethod
		{
			get { return _isMethod; }
		}

		public int Position
		{
			get { return _position; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.GenericParameter; }
		}

		#endregion

		#region Methods

		public override void GetGenericParameter(out bool isMethod, out int position)
		{
			isMethod = _isMethod;
			position = _position;
		}

		public override bool GetSize(Module module, out int size)
		{
			size = 0;
			return false;
		}

		public override string ToString()
		{
			return Name;
		}

		#endregion

		#region Static

		internal static GenericParameterType LoadVar(IBinaryAccessor accessor, Module module)
		{
			return new GenericParameterType(false, accessor.ReadCompressedInteger());
		}

		internal static GenericParameterType LoadMVar(IBinaryAccessor accessor, Module module)
		{
			return new GenericParameterType(true, accessor.ReadCompressedInteger());
		}

		#endregion
	}
}
