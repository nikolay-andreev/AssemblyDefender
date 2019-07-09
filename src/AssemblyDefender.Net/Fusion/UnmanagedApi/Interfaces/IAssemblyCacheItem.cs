using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net.Fusion.UnmanagedApi
{
	/// <summary>
	///
	/// </summary>
	[Guid("9E3AAEB4-D1CD-11D2-BAB9-00C04F8ECEAE")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible(true)]
	public interface IAssemblyCacheItem
	{
		/// <summary>
		/// Creates a stream with the specified name and format.
		/// </summary>
		/// <param name="dwFlags">Flags value.</param>
		/// <param name="pszStreamName">The name of the stream to be created.</param>
		/// <param name="dwFormat">The format of the file to be streamed.</param>
		/// <param name="dwFormatFlags">Format-specific flags defined in STREAM_FORMAT.</param>
		/// <param name="ppIStream"> A pointer to the address of the returned IStream instance.</param>
		/// <param name="puliMaxSize">The maximum size of the stream referenced by ppIStream.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int CreateStream(
			[In] uint dwFlags,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string pszStreamName,
			[In] uint dwFormat,
			[In] uint dwFormatFlags,
			[Out] out IStream ppIStream,
			[In, Out] ref long puliMaxSize);

		/// <summary>
		/// Commits the cached assembly reference to memory.
		/// </summary>
		/// <param name="dwFlags">Flags defined in Fusion.idl.</param>
		/// <param name="pulDisposition">A value that indicates the result of the operation.</param>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int Commit(
			[In] uint dwFlags,
			[Out] out long pulDisposition);

		/// <summary>
		/// Allows the assembly in the global assembly cache to perform cleanup operations before it is released.
		/// </summary>
		/// <returns>HRESULT</returns>
		[PreserveSig]
		int AbortItem();
	}
}
