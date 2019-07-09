using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ReferencedFunctionPointer : ReferencedType
	{
		private ICallSite _callSite;

		internal ReferencedFunctionPointer(AssemblyManager assemblyManager, ICallSite callSite)
			: base(assemblyManager)
		{
			_callSite = callSite;
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.FunctionPointer; }
		}

		public override ICallSite GetFunctionPointer()
		{
			return _callSite;
		}
	}
}
