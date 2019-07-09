using System;
using System.Runtime.InteropServices;
using System.Text;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.SymbolStore.UnmanagedApi
{
	/// <summary>
	/// Provides information to the symbol handler about methods that were moved as a result of edits.
	/// Line deltas allow a compiler to omit functions that have not been modified from
	/// the pdb stream provided the line information meets the following condition.
	/// The correct line information can be determined with the old pdb line info and
	/// one delta for all lines in the function.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct SYMLINEDELTA
	{
		/// <summary>
		/// The method's metadata token.
		/// </summary>
		public uint Method;

		/// <summary>
		/// The number of lines the method was moved.
		/// </summary>
		public int Delta;
	}
}
