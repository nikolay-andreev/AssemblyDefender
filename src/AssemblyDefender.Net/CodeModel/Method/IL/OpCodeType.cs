using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Describes the types of the Microsoft intermediate language (MSIL) instructions.
	/// </summary>
	public enum OpCodeType : byte
	{
		Primitive,
		Prefix,
		Reserved,
		Object,
		TypedRef,
		Call,
		Branch,
		CondBranch,
		Math,
		Box,
		Convert,
		Compare,
		Ldarg,
		Starg,
		Ldloc,
		Stloc,
		Ldc,
		Ldind,
		Stind,
		Ldelem,
		Stelem,
		Ldfld,
		Stfld,
		Ldftn,
	}
}
