using System;
using System.Runtime.InteropServices;
using System.Text;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	/// <summary>
	/// Represents a reference that an application makes to an assembly that the application has installed
	/// in the global assembly cache.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct FUSION_INSTALL_REFERENCE
	{
		/// <summary>
		/// The size of the structure in bytes.
		/// </summary>
		public uint cbSize;

		/// <summary>
		/// Reserved for future extensibility. This value must be 0 (zero).
		/// </summary>
		public uint dwFlags;

		/// <summary>
		/// The entity that adds the reference. This field can have one of the FusionNative.FUSION_REFCOUNT_* values.
		/// </summary>
		public Guid guidScheme;

		/// <summary>
		/// A unique string that identifies the application that installed the assembly in the
		/// global assembly cache. Its value depends upon the value of the guidScheme field.
		/// </summary>
		[MarshalAs(InteropUnmanagedType.LPWStr)]
		public string szIdentifier;

		/// <summary>
		/// A string that is understood only by the entity that adds the reference. The global assembly cache
		/// stores this string, but does not use it.
		/// </summary>
		[MarshalAs(InteropUnmanagedType.LPWStr)]
		public string szNonCanonicalData;
	}
}
