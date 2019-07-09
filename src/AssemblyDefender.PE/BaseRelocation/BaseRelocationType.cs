using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public enum BaseRelocationType
	{
		/// <summary>
		/// The base relocation is skipped. This type can be used to pad a block.
		/// </summary>
		ABSOLUTE = 0,

		/// <summary>
		/// The base relocation adds the high 16 bits of the difference to the 16-bit field at offset.
		/// The 16-bit field represents the high value of a 32-bit word.
		/// </summary>
		HIGH = 1,

		/// <summary>
		/// The base relocation adds the low 16 bits of the difference to the 16-bit field at offset.
		/// The 16-bit field represents the low half of a 32-bit word.
		/// </summary>
		LOW = 2,

		/// <summary>
		/// The base relocation applies all 32 bits of the difference to the 32-bit field at offset.
		/// </summary>
		HIGHLOW = 3,

		/// <summary>
		/// The base relocation adds the high 16 bits of the difference to the 16-bit field at offset.
		/// The 16-bit field represents the high value of a 32-bit word. The low 16 bits of the 32-bit
		/// value are stored in the 16-bit word that follows this base relocation. This means that this
		/// base relocation occupies two slots.
		/// </summary>
		HIGHADJ = 4,

		/// <summary>
		/// The base relocation applies to a MIPS jump instruction.
		/// </summary>
		MIPS_JMPADDR = 5,

		/// <summary>
		/// The base relocation applies to a MIPS16 jump instruction.
		/// </summary>
		MIPS_JMPADDR16 = 9,

		/// <summary>
		/// The base relocation applies the difference to the 64-bit field at offset.
		/// </summary>
		DIR64 = 10,
	}
}
