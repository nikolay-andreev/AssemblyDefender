using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public enum ILBlockType
	{
		Block,
		Body,
		Try,
		ExceptionHandle,
		ExceptionFilter,
	}
}
