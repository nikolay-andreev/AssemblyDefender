using System;
using System.Runtime.InteropServices;
using System.Text;
using InteropUnmanagedType = System.Runtime.InteropServices.UnmanagedType;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// P/Invoke declarations for strong name APIs
	/// </summary>
	internal static class StrongNameNative
	{
		/// <summary>
		/// Return the last error
		/// </summary>
		/// <returns>error information for the last strong name call</returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameErrorInfo", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public extern static int StrongNameErrorInfo();

		/// <summary>
		/// Free the buffer allocated by strong name functions
		/// </summary>
		/// <param name="pbMemory">address of memory to free</param>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameFreeBuffer", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public extern static void StrongNameFreeBuffer(
			[In] IntPtr pbMemory);

		/// <summary>
		/// Generate a new key pair with the specified key size for strong name use.
		/// </summary>
		/// <param name="wszKeyContainer">desired key container name</param>
		/// <param name="dwFlags">flags</param>
		/// <param name="dwKeySize">desired key size</param>
		/// <param name="ppbKeyBlob">[out] generated public / private key blob</param>
		/// <param name="pcbKeyBlob">[out] size of the generated blob</param>
		/// <returns>true if the key was generated, false if there was an error</returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameKeyGenEx", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public extern static bool StrongNameKeyGenEx(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string wszKeyContainer,
			[In] StrongNameKeyGenFlags dwFlags,
			[In] int dwKeySize,
			[Out] out IntPtr ppbKeyBlob,
			[Out] out long pcbKeyBlob);

		/// <summary>
		/// Import a key pair into a key container
		/// </summary>
		/// <param name="wszKeyContainer">desired key container name</param>
		/// <param name="pbKeyBlob">public/private key blob</param>
		/// <param name="cbKeyBlob">number of bytes in <paramref name="pbKeyBlob"/></param>
		/// <returns>true on success, false on error</returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameKeyInstall", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public extern static bool StrongNameKeyInstall(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string wszKeyContainer,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbKeyBlob,
			[In] int cbKeyBlob);

		/// <summary>
		/// Delete a key pair from a key container
		/// </summary>
		/// <param name="wszKeyContainer">key container name</param>
		/// <returns>true on success, false on failure</returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameKeyDelete", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public extern static bool StrongNameKeyDelete(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string wszKeyContainer);

		/// <summary>
		/// Retrieve the public portion of a key pair.
		/// </summary>
		/// <param name="wszKeyContainer">key container to extract from, null to create a temporary container</param>
		/// <param name="pbKeyBlob">key blob to extract from, null to extract from a container</param>
		/// <param name="cbKeyBlob">size in bytes of <paramref name="pbKeyBlob"/></param>
		/// <param name="ppbPublicKeyBlob">[out]public key blob</param>
		/// <param name="pcbPublicKeyBlob">[out]size of <paramref name="pcbPublicKeyBlob"/></param>
		/// <returns>true on success, false on error</returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameGetPublicKey", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public extern static bool StrongNameGetPublicKey(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string wszKeyContainer,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbKeyBlob,
			[In, MarshalAs(InteropUnmanagedType.U4)] int cbKeyBlob,
			[Out] out IntPtr ppbPublicKeyBlob,
			[Out, MarshalAs(InteropUnmanagedType.U4)] out int pcbPublicKeyBlob);

		/// <summary>
		/// Generates a strong name signature for the specified assembly, according to the specified flags.
		/// </summary>
		/// <param name="wszFilePath">The path to the file that contains the manifest of the assembly for which the
		/// strong name signature will be generated.</param>
		/// <param name="wszKeyContainer"> The name of the key container that contains the public/private key pair.
		/// If pbKeyBlob is null, wszKeyContainer must specify a valid container within the cryptographic
		/// service provider (CSP). In this case, the key pair stored in the container is used to sign the file.
		/// If pbKeyBlob is not null, the key pair is assumed to be contained in the key binary large object (BLOB).</param>
		/// <param name="pbKeyBlob">A pointer to the public/private key pair. This pair is in the format created by
		/// the Win32 CryptExportKey function. If pbKeyBlob is null, the key container specified by wszKeyContainer
		/// is assumed to contain the key pair.</param>
		/// <param name="cbKeyBlob">The size, in bytes, of pbKeyBlob.</param>
		/// <param name="ppbSignatureBlob">A pointer to the location to which the common language runtime returns the
		/// signature. If ppbSignatureBlob is null, the runtime stores the signature in the file specified by wszFilePath.
		/// If ppbSignatureBlob is not null, the common language runtime allocates space in which to return the signature.
		/// The caller must free this space using the StrongNameFreeBuffer function.</param>
		/// <param name="pcbSignatureBlob">The size, in bytes, of the returned signature.</param>
		/// <param name="dwFlags">One or more of the following values:
		/// SN_SIGN_ALL_FILES (0x00000001) - Recompute all hashes for linked modules.
		/// SN_TEST_SIGN (0x00000002) - Test-sign the assembly.</param>
		/// <returns></returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameSignatureGenerationEx", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public static extern bool StrongNameSignatureGenerationEx(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string wszFilePath,
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string wszKeyContainer,
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbKeyBlob,
			[In] int cbKeyBlob,
			[Out] out IntPtr ppbSignatureBlob,
			[Out] out int pcbSignatureBlob,
			[In] StrongNameGenerationFlags dwFlags);

		/// <summary>
		/// Create a strong name token from an assembly file, and addtionally return the full public key blob.
		/// </summary>
		/// <param name="wszFilePath">path to the PE file for the assembly</param>
		/// <param name="ppbStrongNameToken">[out]strong name token</param>
		/// <param name="pcbStrongNameToken">[out]length of <paramref name="ppbStrongNameToken"/></param>
		/// <param name="ppbPublicKeyBlob">[out]public key blob</param>
		/// <param name="pcbPublicKeyBlob">[out]length of <paramref name="ppbPublicKeyBlob"/></param>
		/// <returns>true on success, false on error</returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameTokenFromAssemblyEx", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public extern static bool StrongNameTokenFromAssemblyEx(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string wszFilePath,
			[Out] out IntPtr ppbStrongNameToken,
			[Out, MarshalAs(InteropUnmanagedType.U4)] out int pcbStrongNameToken,
			[Out] out IntPtr ppbPublicKeyBlob,
			[Out, MarshalAs(InteropUnmanagedType.U4)] out int pcbPublicKeyBlob);

		/// <summary>
		/// Create a strong name token from a public key blob.
		/// </summary>
		/// <param name="pbPublicKeyBlob">key blob to generate the token for</param>
		/// <param name="cbPublicKeyBlob">number of bytes in <paramref name="pbPublicKeyBlob"/></param>
		/// <param name="ppbStrongNameToken">[out]public key token</param>
		/// <param name="pcbStrongNameToken">[out]number of bytes in the token</param>
		/// <returns>true on success, false on error</returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameTokenFromPublicKey", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public extern static bool StrongNameTokenFromPublicKey(
			[In, MarshalAs(InteropUnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbPublicKeyBlob,
			[In] int cbPublicKeyBlob,
			[Out] out IntPtr ppbStrongNameToken,
			[Out, MarshalAs(InteropUnmanagedType.U4)] out int pcbStrongNameToken);

		/// <summary>
		/// Verify a strong name/manifest against a public key blob.
		/// </summary>
		/// <param name="wszFilePath">valid path to the PE file for the assembly</param>
		/// <param name="fForceVerification">verify even if the settings in the registry disable it</param>
		/// <param name="pfWasVerified">[out] set to false if verify succeeded due to registry settings</param>
		/// <returns>true if the assembly verified, false otherwise</returns>
		[DllImport("mscoree.dll", EntryPoint = "StrongNameSignatureVerificationEx", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
		public static extern bool StrongNameSignatureVerificationEx(
			[In, MarshalAs(InteropUnmanagedType.LPWStr)] string wszFilePath,
			[In] bool fForceVerification,
			[In, Out] ref bool pfWasVerified);
	}
}
