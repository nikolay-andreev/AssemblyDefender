using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class FunctionPointer : TypeSignature
	{
		#region Fields

		private CallSite _callSite;

		#endregion

		#region Ctors

		private FunctionPointer()
		{
		}

		public FunctionPointer(CallSite callSite)
		{
			if (callSite == null)
				throw new ArgumentNullException("callSite");

			_callSite = callSite;
		}

		#endregion

		#region Properties

		public CallSite CallSite
		{
			get { return _callSite; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.FunctionPointer; }
		}

		#endregion

		#region Methods

		public override CallSite GetFunctionPointer()
		{
			return _callSite;
		}

		public override bool GetSize(Module module, out int size)
		{
			size = 4;
			return true;
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _callSite);
		}

		#endregion

		#region Static

		internal static FunctionPointer LoadFnPtr(IBinaryAccessor accessor, Module module)
		{
			var callSite = CallSite.LoadCallSite(accessor, module);

			return new FunctionPointer(callSite);
		}

		#endregion
	}
}
