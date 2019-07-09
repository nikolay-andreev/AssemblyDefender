using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Specifies the kinds of addresses used by the interfaces
	/// </summary>
	public enum CorSymAddrKind : int
	{
		/// <summary>
		/// IL_OFFSET: addr1 = IL local var or param index.
		/// </summary>
		IL_OFFSET = 1,

		/// <summary>
		/// NATIVE_RVA: addr1 = RVA into module.
		/// </summary>
		NATIVE_RVA = 2,

		/// <summary>
		/// NATIVE_REGISTER: addr1 = register the var is stored in.
		/// </summary>
		NATIVE_REGISTER = 3,

		/// <summary>
		/// NATIVE_REGREL: addr1 = register, addr2 = offset.
		/// </summary>
		NATIVE_REGREL = 4,

		/// <summary>
		/// NATIVE_OFFSET: addr1 = offset from start of parent.
		/// </summary>
		NATIVE_OFFSET = 5,

		/// <summary>
		/// NATIVE_REGREG: addr1 = reg low, addr2 = reg high.
		/// </summary>
		NATIVE_REGREG = 6,

		/// <summary>
		/// NATIVE_REGSTK: addr1 = reg low, addr2 = reg stk, addr3 = offset.
		/// </summary>
		NATIVE_REGSTK = 7,

		/// <summary>
		/// NATIVE_STKREG: addr1 = reg stk, addr2 = offset, addr3 = reg high.
		/// </summary>
		NATIVE_STKREG = 8,

		/// <summary>
		/// BITFIELD: addr1 = field start, addr = field length.
		/// </summary>
		BITFIELD = 9,

		/// <summary>
		/// NATIVE_SECTOFF: addr1 = section, addr = offset.
		/// </summary>
		NATIVE_ISECTOFFSET = 10
	}
}
