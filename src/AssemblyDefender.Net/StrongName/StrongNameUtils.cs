using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public static class StrongNameUtils
	{
		public static bool IsPKCS12(byte[] data)
		{
			return PKCS12.IsValid(data);
		}

		public static bool IsPKCS12File(string filePath)
		{
			return IsPKCS12(File.ReadAllBytes(filePath));
		}

		public static bool IsPasswordValid(string filePath, string password)
		{
			return IsPasswordValid(File.ReadAllBytes(filePath), password);
		}

		public static bool IsPasswordValid(byte[] data, string password)
		{
			try
			{
				var pkcs12 = new PKCS12(data, password);
				return pkcs12.Keys.Count > 0;
			}
			catch (CryptographicException)
			{
				return false;
			}
		}

		/// <summary>
		/// Create a public key token from a signed assembly
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="filePath"/> is null
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If <paramref name="filePath"/> is empty
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If the token could not be generated
		/// </exception>
		/// <param name="filePath">assembly to generate the token of</param>
		/// <returns>public key token of <paramref name="filePath"/></returns>
		public static byte[] CreateTokenFromAssembly(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullOrEmptyException("filePath");

			// get the key and token
			IntPtr blobBuffer = IntPtr.Zero;
			IntPtr tokenBuffer = IntPtr.Zero;

			try
			{
				int blobSize = 0;
				int tokenSize = 0;

				// extract the public key token
				if (!StrongNameNative.StrongNameTokenFromAssemblyEx(
						filePath, out tokenBuffer, out tokenSize, out blobBuffer, out blobSize))
				{
					Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
				}

				// copy the token out of unmanaged memory, and return it
				byte[] token = new byte[tokenSize];
				Marshal.Copy(tokenBuffer, token, 0, tokenSize);
				return token;
			}
			finally
			{
				if (blobBuffer != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(blobBuffer);
				if (tokenBuffer != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(tokenBuffer);
			}
		}

		/// <summary>
		/// Create a public key token from a public key
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="publicKey"/> is null
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If the token could not be generated
		/// </exception>
		/// <param name="publicKey">public key to generate the token for</param>
		/// <returns>public key token of <paramref name="publicKey"/></returns>
		public static byte[] CreateTokenFromPublicKey(byte[] publicKey)
		{
			if (publicKey == null || publicKey.Length == 0)
				throw new ArgumentNullOrEmptyException("publicKey");

			IntPtr tokenPointer = IntPtr.Zero;
			int tokenSize = 0;

			try
			{
				// generate the token
				if (!StrongNameNative.StrongNameTokenFromPublicKey(
						publicKey, publicKey.Length, out tokenPointer, out tokenSize))
				{
					Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
				}

				// make sure the key size makes sense
				if (tokenSize <= 0 || tokenSize > int.MaxValue)
				{
					throw new InvalidOperationException();
				}

				// get the key into managed memory
				byte[] token = new byte[tokenSize];
				Marshal.Copy(tokenPointer, token, 0, tokenSize);
				return token;
			}
			finally
			{
				// release the token memory
				if (tokenPointer != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(tokenPointer);
			}
		}

		/// <summary>
		/// Get the public key from an assembly
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="filePath"/> is null
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If <paramref name="filePath"/> is empty
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If the public key blob could not be extracted
		/// </exception>
		/// <param name="filePath">assembly to extract from</param>
		/// <returns>public key blob the assembly was signed with</returns>
		public static byte[] ExtractPublicKeyFromAssembly(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullOrEmptyException("filePath");

			// get the key and token
			IntPtr blobBuffer = IntPtr.Zero;
			IntPtr tokenBuffer = IntPtr.Zero;

			try
			{
				int blobSize = 0;
				int tokenSize = 0;

				// extract the public key blob
				if (!StrongNameNative.StrongNameTokenFromAssemblyEx(
						filePath, out tokenBuffer, out tokenSize, out blobBuffer, out blobSize))
				{
					Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
				}

				// copy the key out of unmanaged memory, and return it
				byte[] keyBlob = new byte[blobSize];
				Marshal.Copy(blobBuffer, keyBlob, 0, blobSize);
				return keyBlob;
			}
			finally
			{
				if (blobBuffer != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(blobBuffer);
				if (tokenBuffer != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(tokenBuffer);
			}
		}

		/// <summary>
		///     Get the public key from a key container
		/// </summary>
		/// <exception cref="ArgumentNullException">
		///     If <paramref name="keyContainer"/> is null
		/// </exception>
		/// <exception cref="ArgumentException">
		///     If <paramref name="keyContainer"/> is empty
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///     If the key could not be extracted
		/// </exception>
		/// <param name="keyContainer">key container to get the public key from</param>
		/// <returns>public key blob</returns>
		public static byte[] ExtractPublicKeyFromKeyContainer(string keyContainerName)
		{
			if (string.IsNullOrEmpty(keyContainerName))
				throw new ArgumentNullOrEmptyException("keyContainerName");

			// extract the public key portion of the blob
			IntPtr publicKeyBuffer = IntPtr.Zero;
			try
			{
				int publicKeyBlobSize = 0;

				if (!StrongNameNative.StrongNameGetPublicKey(
					keyContainerName, null, 0, out publicKeyBuffer, out publicKeyBlobSize))
				{
					Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
				}

				// copy the key out of unmanaged memory, and return it
				byte[] publicKeyBlob = new byte[publicKeyBlobSize];
				Marshal.Copy(publicKeyBuffer, publicKeyBlob, 0, publicKeyBlobSize);
				return publicKeyBlob;
			}
			finally
			{
				if (publicKeyBuffer != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(publicKeyBuffer);
			}
		}

		/// <summary>
		/// Get the public key from a key pair blob
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="keyPair"/> is null
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If the key could not be extracted
		/// </exception>
		/// <param name="keyPair">key blob to get the public key from</param>
		/// <returns>public key blob</returns>
		public static byte[] ExtractPublicKeyFromKeyPair(byte[] keyPair)
		{
			if (keyPair == null)
				throw new ArgumentNullException("keyPair");

			// extract the public key portion of the blob
			IntPtr publicKeyBuffer = IntPtr.Zero;
			try
			{
				int publicKeyBlobSize = 0;

				if (!StrongNameNative.StrongNameGetPublicKey(
					null, keyPair, keyPair.Length, out publicKeyBuffer, out publicKeyBlobSize))
				{
					Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
				}

				// copy the key out of unmanaged memory, and return it
				byte[] publicKeyBlob = new byte[publicKeyBlobSize];
				Marshal.Copy(publicKeyBuffer, publicKeyBlob, 0, publicKeyBlobSize);
				return publicKeyBlob;
			}
			finally
			{
				if (publicKeyBuffer != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(publicKeyBuffer);
			}
		}

		/// <summary>
		/// Extract key pair from PKCS12 certificate.
		/// </summary>
		public static byte[] ExtractKeyPairFromPKCS12(byte[] data, string password)
		{
			try
			{
				var pkcs12 = new PKCS12(data, password);
				var rsa = pkcs12.Keys[0];
				return rsa.ToCapiPrivateKeyBlob();
			}
			catch (Exception)
			{
				throw new CryptographicException(SR.InvalidCertificate);
			}
		}

		/// <summary>
		/// Extract key pair from PKCS12 certificate.
		/// </summary>
		public static byte[] ExtractKeyPairFromPKCS12File(string filePath, string password)
		{
			return ExtractKeyPairFromPKCS12(File.ReadAllBytes(filePath), password);
		}

		/// <summary>
		/// Generate a key pair blob
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If <paramref name="keySize"/> is not positive
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If the key could not be generated
		/// </exception>
		/// <returns>generated key pair blob</returns>
		public static byte[] GenerateKeyPair()
		{
			return GenerateKeyPair(0x400);
		}

		/// <summary>
		/// Generate a key pair blob
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If <paramref name="keySize"/> is not positive
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If the key could not be generated
		/// </exception>
		/// <param name="keySize">size, in bits, of the key to generate</param>
		/// <returns>generated key pair blob</returns>
		public static byte[] GenerateKeyPair(int keySize)
		{
			if (keySize <= 0)
				throw new ArgumentOutOfRangeException("keySize");

			// variables that hold the unmanaged key
			IntPtr keyBlob = IntPtr.Zero;
			long generatedSize = 0;

			try
			{
				// create the key
				if (!StrongNameNative.StrongNameKeyGenEx(
						null, StrongNameKeyGenFlags.None, (int)keySize,
						out keyBlob, out generatedSize))
				{
					Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
				}

				// make sure the key size makes sense
				if (generatedSize <= 0 || generatedSize > int.MaxValue)
				{
					throw new InvalidOperationException();
				}

				// get the key into managed memory
				byte[] key = new byte[generatedSize];
				Marshal.Copy(keyBlob, key, 0, (int)generatedSize);
				return key;
			}
			finally
			{
				// release the unmanaged memory the key resides in
				if (keyBlob != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(keyBlob);
			}
		}

		/// <summary>
		/// Install a key into a key container
		/// </summary>
		/// <param name="keyBlob">Key pair blob</param>
		/// <param name="keyContainerName">Name of the key container to install the keys into</param>
		public static void InstallKey(byte[] keyBlob, string keyContainerName)
		{
			if (keyBlob == null)
				throw new ArgumentNullException("keyBlob");

			if (string.IsNullOrEmpty(keyContainerName))
				throw new ArgumentNullOrEmptyException("keyContainerName");

			if (!StrongNameNative.StrongNameKeyInstall(keyContainerName, keyBlob, keyBlob.Length))
			{
				Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
			}
		}

		/// <summary>
		/// Delete a key from a key container
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="keyContainerName"/> is null
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If the key container could not be deleted
		/// </exception>
		/// <param name="keyContainerName">name of the key container to delete</param>
		public static void DeleteKey(string keyContainerName)
		{
			if (string.IsNullOrEmpty(keyContainerName))
				throw new ArgumentNullOrEmptyException("keyContainerName");

			if (!StrongNameNative.StrongNameKeyDelete(keyContainerName))
			{
				Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
			}
		}

		public static void SignAssemblyFromKeyPair(string filePath, byte[] keyPair, StrongNameGenerationFlags flags)
		{
			byte[] signatureBlob = GenerateSignatureFromKeyPair(filePath, keyPair, flags);
			SignAssembly(filePath, signatureBlob);
		}

		public static void SignAssemblyFromKeyContainer(string filePath, string keyContainerName, StrongNameGenerationFlags flags)
		{
			byte[] signatureBlob = GenerateSignatureFromKeyContainer(filePath, keyContainerName, flags);
			SignAssembly(filePath, signatureBlob);
		}

		public static unsafe void SignAssembly(string filePath, byte[] signatureBlob)
		{
			long offset;
			using (var pe = PEImage.LoadFile(filePath))
			{
				var corHeader = CorHeader.Load(pe);

				if (!pe.ResolvePositionToSectionData(corHeader.StrongNameSignature.RVA, out offset))
				{
					throw new BadImageFormatException(string.Format(SR.MetadataImageNotValid, filePath));
				}
			}

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
			{
				stream.Position = offset;
				stream.Write(signatureBlob, 0, signatureBlob.Length);
			}
		}

		/// <summary>
		/// Generates a strong name signature for the specified assembly, according to the specified flags.
		/// </summary>
		/// <param name="filePath">The path to the file that contains the manifest of the assembly for which the
		/// strong name signature will be generated.</param>
		/// <param name="keyPair">A pointer to the public/private key pair.</param>
		/// <param name="flags">Flags</param>
		public static byte[] GenerateSignatureFromKeyPair(string filePath, byte[] keyPair, StrongNameGenerationFlags flags)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullOrEmptyException("filePath");

			if (keyPair == null)
				throw new ArgumentNullException("keyPair");

			IntPtr signatureBlobPtr = IntPtr.Zero;
			try
			{
				int signatureBlobSize = 0;

				if (!StrongNameNative.StrongNameSignatureGenerationEx(
					filePath, null, keyPair, keyPair.Length, out signatureBlobPtr, out signatureBlobSize, flags))
				{
					Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
				}

				byte[] signatureBlob = new byte[signatureBlobSize];
				Marshal.Copy(signatureBlobPtr, signatureBlob, 0, signatureBlobSize);
				return signatureBlob;
			}
			finally
			{
				if (signatureBlobPtr != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(signatureBlobPtr);
			}
		}

		/// <summary>
		/// Generates a strong name signature for the specified assembly, according to the specified flags.
		/// </summary>
		/// <param name="filePath">The path to the file that contains the manifest of the assembly for which the
		/// strong name signature will be generated.</param>
		/// <param name="keyPair">A pointer to the public/private key pair.</param>
		/// <param name="flags">Flags</param>
		public static byte[] GenerateSignatureFromKeyContainer(string filePath, string keyContainerName, StrongNameGenerationFlags flags)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullOrEmptyException("filePath");

			if (string.IsNullOrEmpty(keyContainerName))
				throw new ArgumentNullOrEmptyException("keyContainerName");

			IntPtr signatureBlobPtr = IntPtr.Zero;
			try
			{
				int signatureBlobSize = 0;

				if (!StrongNameNative.StrongNameSignatureGenerationEx(
					filePath, keyContainerName, null, 0, out signatureBlobPtr, out signatureBlobSize, flags))
				{
					Marshal.ThrowExceptionForHR(StrongNameNative.StrongNameErrorInfo());
				}

				byte[] signatureBlob = new byte[signatureBlobSize];
				Marshal.Copy(signatureBlobPtr, signatureBlob, 0, signatureBlobSize);
				return signatureBlob;
			}
			finally
			{
				if (signatureBlobPtr != IntPtr.Zero)
					StrongNameNative.StrongNameFreeBuffer(signatureBlobPtr);
			}
		}

		/// <summary>
		/// Verify an assembly's strong name
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// If <paramref name="filePath"/> is null
		/// </exception>
		/// <exception cref="ArgumentException">
		/// If <paramref name="filePath"/> is empty
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// If verification could not complete
		/// </exception>
		/// <param name="filePath">assembly to verify</param>
		/// <param name="forceVerification">true to ignore the skip verify registry </param>
		/// <returns>result of the strong name verification</returns>
		public static bool VerifyAssembly(string filePath, bool forceVerification)
		{
			try
			{
				// do the verification
				bool wasVerified = false;
				if (!StrongNameNative.StrongNameSignatureVerificationEx(
						filePath, forceVerification, ref wasVerified))
				{
					return false;
				}

				return wasVerified;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
