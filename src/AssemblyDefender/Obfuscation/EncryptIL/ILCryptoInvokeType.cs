using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ILCryptoInvokeType : StateObject
	{
		#region Fields

		private int _genericParameterCount;
		private int _methodCount;
		private string _typeName;
		private string _invokeMethodName;
		private int _invokeMethodListRID;

		#endregion

		#region Properties

		public int GenericParameterCount
		{
			get { return _genericParameterCount; }
			set
			{
				_genericParameterCount = value;
				OnChanged();
			}
		}

		public int MethodCount
		{
			get { return _methodCount; }
			set
			{
				_methodCount = value;
				OnChanged();
			}
		}

		public string TypeName
		{
			get { return _typeName; }
			set
			{
				_typeName = value;
				OnChanged();
			}
		}

		public string InvokeMethodName
		{
			get { return _invokeMethodName; }
			set
			{
				_invokeMethodName = value;
				OnChanged();
			}
		}

		public StateObjectList<ILCryptoInvokeMethod> InvokeMethods
		{
			get
			{
				return GetOrCreateObject<StateObjectList<ILCryptoInvokeMethod>>(ref _invokeMethodListRID);
			}
		}

		#endregion

		#region Methods

		protected internal override void Read(IBinaryAccessor accessor)
		{
			_genericParameterCount = accessor.Read7BitEncodedInt();
			_methodCount = accessor.Read7BitEncodedInt();
			_typeName = ReadString(accessor);
			_invokeMethodName = ReadString(accessor);
			_invokeMethodListRID = accessor.Read7BitEncodedInt();

			base.Read(accessor);
		}

		protected internal override void Write(IBinaryAccessor accessor)
		{
			accessor.Write7BitEncodedInt(_genericParameterCount);
			accessor.Write7BitEncodedInt(_methodCount);
			WriteString(accessor, _typeName);
			WriteString(accessor, _invokeMethodName);
			accessor.Write7BitEncodedInt(_invokeMethodListRID);

			base.Write(accessor);
		}

		#endregion
	}
}
