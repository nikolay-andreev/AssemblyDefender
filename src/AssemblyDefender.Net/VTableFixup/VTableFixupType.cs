using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Type of the fixup
	/// </summary>
	public enum VTableFixupType
	{
		/// <summary>
		/// Vtable slots are 32 bits.
		/// </summary>
		SlotSize32Bit = 0x01,

		/// <summary>
		/// Vtable slots are 64 bits.
		/// </summary>
		SlotSize64Bit = 0x02,

		/// <summary>
		/// Transition from unmanaged to managed code. If specified, the contents of the v-table slot (method token)
		/// are replaced at run time with the address of the marshaling thunk. If this flag is not specified, the thunk
		/// is not created, and the method token is replaced with this method’s address.
		/// </summary>
		FromUnmanaged = 0x04,

		RetainAppDomain = 0x08,

		/// <summary>
		/// Call most derived method described by the token (only valid for virtual methods).
		/// This flag is not currently used.
		/// </summary>
		CallMostDerived = 0x10,
	}
}
