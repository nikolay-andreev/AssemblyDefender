using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ILCryptoMethod : StateObject
	{
		#region Fields

		private int _methodID;
		private int _invokeMethodRID;
		private TypeSignature[] _delegateGenericArguments;
		private TypeSignature[] _invokeGenericArguments;

		#endregion

		#region Properties

		public int MethodID
		{
			get { return _methodID; }
			set
			{
				_methodID = value;
				OnChanged();
			}
		}

		public ILCryptoInvokeMethod InvokeMethod
		{
			get { return _state.GetObject<ILCryptoInvokeMethod>(_invokeMethodRID); }
			set
			{
				_invokeMethodRID = _state.AddObject(value);
				OnChanged();
			}
		}

		public TypeSignature[] DelegateGenericArguments
		{
			get { return _delegateGenericArguments; }
			set
			{
				_delegateGenericArguments = value;
				OnChanged();
			}
		}

		public TypeSignature[] InvokeGenericArguments
		{
			get { return _invokeGenericArguments; }
			set
			{
				_invokeGenericArguments = value;
				OnChanged();
			}
		}

		#endregion

		#region Methods

		protected internal override void Read(IBinaryAccessor accessor)
		{
			_methodID = accessor.Read7BitEncodedInt();
			_invokeMethodRID = accessor.Read7BitEncodedInt();
			_delegateGenericArguments = ReadSignatures<TypeSignature>(accessor);
			_invokeGenericArguments = ReadSignatures<TypeSignature>(accessor);

			base.Read(accessor);
		}

		protected internal override void Write(IBinaryAccessor accessor)
		{
			accessor.Write7BitEncodedInt(_methodID);
			accessor.Write7BitEncodedInt(_invokeMethodRID);
			WriteSignatures(accessor, _delegateGenericArguments);
			WriteSignatures(accessor, _invokeGenericArguments);

			base.Write(accessor);
		}

		#endregion
	}
}
