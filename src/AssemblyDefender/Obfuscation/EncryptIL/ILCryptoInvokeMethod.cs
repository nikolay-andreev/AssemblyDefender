using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ILCryptoInvokeMethod : StateObject
	{
		#region Fields

		private int _genericParameterCount;
		private int[] _parameterFlags;
		private CallSite _callSite;
		private int _delegateTypeRID;
		private int _ownerTypeRID;

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

		public int[] ParameterFlags
		{
			get { return _parameterFlags; }
			set
			{
				_parameterFlags = value;
				OnChanged();
			}
		}

		public CallSite CallSite
		{
			get { return _callSite; }
			set
			{
				_callSite = value;
				OnChanged();
			}
		}

		public DelegateType DelegateType
		{
			get { return _state.GetObject<DelegateType>(_delegateTypeRID); }
			set
			{
				_delegateTypeRID = _state.AddObject(value);
				OnChanged();
			}
		}

		public ILCryptoInvokeType OwnerType
		{
			get { return _state.GetObject<ILCryptoInvokeType>(_ownerTypeRID); }
			set
			{
				_ownerTypeRID = _state.AddObject(value);
				OnChanged();
			}
		}

		#endregion

		#region Methods

		protected internal override void Read(IBinaryAccessor accessor)
		{
			_genericParameterCount = accessor.Read7BitEncodedInt();
			_parameterFlags = ReadIntArray(accessor);
			_callSite = (CallSite)ReadSignature(accessor);
			_delegateTypeRID = accessor.Read7BitEncodedInt();
			_ownerTypeRID = accessor.Read7BitEncodedInt();

			base.Read(accessor);
		}

		protected internal override void Write(IBinaryAccessor accessor)
		{
			accessor.Write7BitEncodedInt(_genericParameterCount);
			WriteIntArray(accessor, _parameterFlags);
			WriteSignature(accessor, _callSite);
			accessor.Write7BitEncodedInt(_delegateTypeRID);
			accessor.Write7BitEncodedInt(_ownerTypeRID);

			base.Write(accessor);
		}

		#endregion
	}
}
