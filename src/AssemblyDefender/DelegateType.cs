using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class DelegateType : StateObject
	{
		#region Fields

		private int _genericParameterCount;
		private int[] _invokeParameterFlags;
		private CallSite _invokeCallSite;
		private TypeReference _declaringType;

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

		public int[] InvokeParameterFlags
		{
			get { return _invokeParameterFlags; }
			set
			{
				_invokeParameterFlags = value;
				OnChanged();
			}
		}

		public CallSite InvokeCallSite
		{
			get { return _invokeCallSite; }
			set
			{
				_invokeCallSite = value;
				OnChanged();
			}
		}

		public TypeReference DeclaringType
		{
			get { return _declaringType; }
			set
			{
				_declaringType = value;
				OnChanged();
			}
		}

		#endregion

		#region Methods

		protected internal override void Read(IBinaryAccessor accessor)
		{
			_genericParameterCount = accessor.Read7BitEncodedInt();
			_invokeParameterFlags = ReadIntArray(accessor);
			_invokeCallSite = (CallSite)ReadSignature(accessor);
			_declaringType = (TypeReference)ReadSignature(accessor);

			base.Read(accessor);
		}

		protected internal override void Write(IBinaryAccessor accessor)
		{
			accessor.Write7BitEncodedInt(_genericParameterCount);
			WriteIntArray(accessor, _invokeParameterFlags);
			WriteSignature(accessor, _invokeCallSite);
			WriteSignature(accessor, _declaringType);

			base.Write(accessor);
		}

		#endregion
	}
}
