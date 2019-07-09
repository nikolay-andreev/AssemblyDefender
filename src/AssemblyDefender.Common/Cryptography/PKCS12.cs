using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AssemblyDefender.Common.Cryptography
{
	/// <summary>
	/// PKCS 12 - Personal Information Exchange Syntax.
	/// </summary>
	public class PKCS12
	{
		#region Fields

		private const string pbeWithSHAAnd128BitRC4 = "1.2.840.113549.1.12.1.1";
		private const string pbeWithSHAAnd40BitRC4 = "1.2.840.113549.1.12.1.2";
		private const string pbeWithSHAAnd3KeyTripleDESCBC = "1.2.840.113549.1.12.1.3";
		private const string pbeWithSHAAnd2KeyTripleDESCBC = "1.2.840.113549.1.12.1.4";
		private const string pbeWithSHAAnd128BitRC2CBC = "1.2.840.113549.1.12.1.5";
		private const string pbeWithSHAAnd40BitRC2CBC = "1.2.840.113549.1.12.1.6";

		// bags
		private const string keyBag = "1.2.840.113549.1.12.10.1.1";
		private const string pkcs8ShroudedKeyBag = "1.2.840.113549.1.12.10.1.2";
		private const string certBag = "1.2.840.113549.1.12.10.1.3";
		private const string crlBag = "1.2.840.113549.1.12.10.1.4";
		private const string secretBag = "1.2.840.113549.1.12.10.1.5";
		private const string safeContentsBag = "1.2.840.113549.1.12.10.1.6";

		// types
		private const string x509Certificate = "1.2.840.113549.1.9.22.1";
		private const string sdsiCertificate = "1.2.840.113549.1.9.22.2";
		private const string x509Crl = "1.2.840.113549.1.9.23.1";

		private int _iterations = 2000;
		private byte[] _password;
		private List<RSA> _keyBags = new List<RSA>();
		private List<byte[]> _secretBags = new List<byte[]>();
		private ArrayList _safeBags = new ArrayList();
		private List<X509Certificate> _certs = new List<X509Certificate>();

		#endregion

		#region Ctors

		public PKCS12(byte[] data)
		{
			Load(data);
		}

		public PKCS12(byte[] data, string password)
		{
			SetPassword(password);
			Load(data);
		}

		public PKCS12(byte[] data, byte[] password)
		{
			_password = password;
			Load(data);
		}

		#endregion

		#region Properties

		public List<RSA> Keys
		{
			get { return _keyBags; }
		}

		public List<byte[]> Secrets
		{
			get { return _secretBags; }
		}

		public List<X509Certificate> Certificates
		{
			get { return _certs; }
		}

		#endregion

		#region Methods

		private void SetPassword(string value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			int size = value.Length;
			int nul = 0;

			// if not present, add space for a NULL (0x00) character
			if (value[size - 1] != 0)
				nul = 1;

			_password = new byte[(size + nul) << 1]; // double for unicode
			Encoding.BigEndianUnicode.GetBytes(value, 0, size, _password, 0);
		}

		private void Load(byte[] data)
		{
			ASN1 pfx = new ASN1(data);
			if (pfx.Tag != 0x30)
				throw new CryptographicException(SR.PKCS12NotValid); // data

			ASN1 version = pfx[0];
			if (version.Tag != 0x02)
				throw new CryptographicException(SR.PKCS12NotValid); // PFX version

			var authSafe = new PKCS7.ContentInfo(pfx[1]);
			if (authSafe.ContentType != PKCS7.Oid.data)
				throw new CryptographicException(SR.PKCS12NotValid); // authenticated safe

			// Now that we know it's a PKCS#12 file, check the (optional) MAC
			// before decoding anything else in the file
			if (pfx.Count > 2)
			{
				var macData = pfx[2];
				if (macData.Tag != 0x30)
					throw new CryptographicException(SR.PKCS12NotValid); // MAC;

				var mac = macData[0];

				if (mac.Tag != 0x30)
					throw new CryptographicException(SR.PKCS12NotValid); // MAC;

				var macAlgorithm = mac[0];

				string macOid = macAlgorithm[0].ToOid();
				if (macOid != "1.3.14.3.2.26")
					throw new CryptographicException(SR.PKCS12NotValid); // unsupported HMAC

				byte[] macValue = mac[1].Value;

				var macSalt = macData[1];

				if (macSalt.Tag != 0x04)
					throw new CryptographicException(SR.PKCS12NotValid); // missing MAC salt

				_iterations = 1; // default value

				if (macData.Count > 2)
				{
					var iters = macData[2];

					if (iters.Tag != 0x02)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid MAC iteration

					_iterations = iters.ToInt32();
				}

				byte[] authSafeData = authSafe.Content[0].Value;

				byte[] calculatedMac = MAC(_password, macSalt.Value, _iterations, authSafeData);

				if (!CompareUtils.Equals(macValue, calculatedMac))
					throw new CryptographicException(SR.PKCS12NotValid); // Invalid MAC - file may have been tampered!
			}

			// we now returns to our original presentation - PFX
			var authenticatedSafe = new ASN1(authSafe.Content[0].Value);
			for (int i = 0; i < authenticatedSafe.Count; i++)
			{
				var ci = new PKCS7.ContentInfo(authenticatedSafe[i]);
				switch (ci.ContentType)
				{
					case PKCS7.Oid.data:
						{
							// Unencrypted (by PKCS#12).
							var safeContents = new ASN1(ci.Content[0].Value);
							for (int j = 0; j < safeContents.Count; j++)
							{
								var safeBag = safeContents[j];
								ReadSafeBag(safeBag);
							}
						}
						break;

					case PKCS7.Oid.encryptedData:
						{
							// Password encrypted.
							var ed = new PKCS7.EncryptedData(ci.Content[0]);
							var decrypted = new ASN1(Decrypt(ed));
							for (int j = 0; j < decrypted.Count; j++)
							{
								var safeBag = decrypted[j];
								ReadSafeBag(safeBag);
							}
						}
						break;

					case PKCS7.Oid.envelopedData:
					// Public key encrypted.
					default:
						throw new CryptographicException(SR.PKCS12NotValid);
				}
			}
		}

		private void ReadSafeBag(ASN1 safeBag)
		{
			if (safeBag.Tag != 0x30)
				throw new CryptographicException(SR.PKCS12NotValid); // invalid safeBag

			ASN1 bagId = safeBag[0];
			if (bagId.Tag != 0x06)
				throw new CryptographicException(SR.PKCS12NotValid); // invalid safeBag id

			ASN1 bagValue = safeBag[1];
			string oid = bagId.ToOid();
			switch (oid)
			{
				case keyBag:
					// NEED UNIT TEST
					AddPrivateKey(new PKCS8.PrivateKeyInfo(bagValue.Value));
					break;
				case pkcs8ShroudedKeyBag:
					var epki = new PKCS8.EncryptedPrivateKeyInfo(bagValue.Value);
					byte[] decrypted = Decrypt(epki.Algorithm, epki.Salt, epki.IterationCount, epki.EncryptedData);
					AddPrivateKey(new PKCS8.PrivateKeyInfo(decrypted));
					Array.Clear(decrypted, 0, decrypted.Length);
					break;
				case certBag:
					var cert = new PKCS7.ContentInfo(bagValue.Value);
					if (cert.ContentType != x509Certificate)
						throw new CryptographicException(SR.PKCS12NotValid); // unsupport certificate type
					X509Certificate x509 = new X509Certificate(cert.Content[0].Value);
					_certs.Add(x509);
					break;
				case crlBag:
					// PKCS12
					break;
				case secretBag:
					byte[] secret = bagValue.Value;
					_secretBags.Add(secret);
					break;
				case safeContentsBag:
					// PKCS12 - ? recurse ?
					break;
				default:
					throw new CryptographicException(SR.PKCS12NotValid); // unknown safeBag oid
			}

			if (safeBag.Count > 2)
			{
				ASN1 bagAttributes = safeBag[2];
				if (bagAttributes.Tag != 0x31)
					throw new CryptographicException(SR.PKCS12NotValid); // invalid safeBag attributes id

				for (int i = 0; i < bagAttributes.Count; i++)
				{
					ASN1 pkcs12Attribute = bagAttributes[i];

					if (pkcs12Attribute.Tag != 0x30)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid PKCS12 attributes id

					ASN1 attrId = pkcs12Attribute[0];
					if (attrId.Tag != 0x06)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid attribute id

					string attrOid = attrId.ToOid();

					ASN1 attrValues = pkcs12Attribute[1];
					for (int j = 0; j < attrValues.Count; j++)
					{
						ASN1 attrValue = attrValues[j];

						switch (attrOid)
						{
							case PKCS9.friendlyName:
								if (attrValue.Tag != 0x1e)
									throw new CryptographicException(SR.PKCS12NotValid); // invalid attribute value id
								break;
							case PKCS9.localKeyId:
								if (attrValue.Tag != 0x04)
									throw new CryptographicException(SR.PKCS12NotValid); // invalid attribute value id
								break;
							default:
								// Unknown OID -- don't check Tag
								break;
						}
					}
				}
			}

			_safeBags.Add(new SafeBag(oid, safeBag));
		}

		private void AddPrivateKey(PKCS8.PrivateKeyInfo pki)
		{
			byte[] privateKey = pki.PrivateKey;
			switch (privateKey[0])
			{
				case 0x02:
					//bool found;
					//DSAParameters p = GetExistingParameters(out found);
					//if (found)
					//{
					//    _keyBags.Add(PKCS8.PrivateKeyInfo.DecodeDSA(privateKey, p));
					//}
					break;
				case 0x30:
					_keyBags.Add(PKCS8.PrivateKeyInfo.DecodeRSA(privateKey));
					break;
				default:
					Array.Clear(privateKey, 0, privateKey.Length);
					throw new CryptographicException(SR.PKCS12NotValid); // Unknown private key format
			}
			Array.Clear(privateKey, 0, privateKey.Length);
		}

		private byte[] MAC(byte[] password, byte[] salt, int iterations, byte[] data)
		{
			PKCS12.DeriveBytes pd = new PKCS12.DeriveBytes();
			pd.HashName = "SHA1";
			pd.Password = password;
			pd.Salt = salt;
			pd.IterationCount = iterations;

			HMACSHA1 hmac = (HMACSHA1)HMACSHA1.Create();
			hmac.Key = pd.DeriveMAC(20);
			return hmac.ComputeHash(data, 0, data.Length);
		}

		private byte[] Decrypt(string algorithmOid, byte[] salt, int iterationCount, byte[] encryptedData)
		{
			SymmetricAlgorithm sa = null;
			byte[] result = null;
			try
			{
				sa = GetSymmetricAlgorithm(algorithmOid, salt, iterationCount);
				ICryptoTransform ct = sa.CreateDecryptor();
				result = ct.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
			}
			finally
			{
				if (sa != null)
					sa.Clear();
			}
			return result;
		}

		private byte[] Decrypt(PKCS7.EncryptedData ed)
		{
			return Decrypt(ed.EncryptionAlgorithm.ContentType,
				ed.EncryptionAlgorithm.Content[0].Value,
				ed.EncryptionAlgorithm.Content[1].ToInt32(),
				ed.EncryptedContent);
		}

		private SymmetricAlgorithm GetSymmetricAlgorithm(string algorithmOid, byte[] salt, int iterationCount)
		{
			string algorithm = null;
			int keyLength = 8;	// 64 bits (default)
			int ivLength = 8;	// 64 bits (default)

			var pd = new DeriveBytes();
			pd.Password = _password;
			pd.Salt = salt;
			pd.IterationCount = iterationCount;

			switch (algorithmOid)
			{
				case PKCS5.pbeWithMD2AndDESCBC:			// no unit test available
					pd.HashName = "MD2";
					algorithm = "DES";
					break;
				case PKCS5.pbeWithMD5AndDESCBC:			// no unit test available
					pd.HashName = "MD5";
					algorithm = "DES";
					break;
				case PKCS5.pbeWithMD2AndRC2CBC:			// no unit test available
					// PKCS12 - RC2-CBC-Parameter (PKCS5)
					// if missing default to 32 bits !!!
					pd.HashName = "MD2";
					algorithm = "RC2";
					keyLength = 4;		// default
					break;
				case PKCS5.pbeWithMD5AndRC2CBC:			// no unit test available
					// PKCS12 - RC2-CBC-Parameter (PKCS5)
					// if missing default to 32 bits !!!
					pd.HashName = "MD5";
					algorithm = "RC2";
					keyLength = 4;		// default
					break;
				case PKCS5.pbeWithSHA1AndDESCBC: 		// no unit test available
					pd.HashName = "SHA1";
					algorithm = "DES";
					break;
				case PKCS5.pbeWithSHA1AndRC2CBC:		// no unit test available
					// PKCS12 - RC2-CBC-Parameter (PKCS5)
					// if missing default to 32 bits !!!
					pd.HashName = "SHA1";
					algorithm = "RC2";
					keyLength = 4;		// default
					break;
				case PKCS12.pbeWithSHAAnd128BitRC4: 		// no unit test available
					pd.HashName = "SHA1";
					algorithm = "RC4";
					keyLength = 16;
					ivLength = 0;		// N/A
					break;
				case PKCS12.pbeWithSHAAnd40BitRC4: 		// no unit test available
					pd.HashName = "SHA1";
					algorithm = "RC4";
					keyLength = 5;
					ivLength = 0;		// N/A
					break;
				case PKCS12.pbeWithSHAAnd3KeyTripleDESCBC:
					pd.HashName = "SHA1";
					algorithm = "TripleDES";
					keyLength = 24;
					break;
				case PKCS12.pbeWithSHAAnd2KeyTripleDESCBC:	// no unit test available
					pd.HashName = "SHA1";
					algorithm = "TripleDES";
					keyLength = 16;
					break;
				case PKCS12.pbeWithSHAAnd128BitRC2CBC: 		// no unit test available
					pd.HashName = "SHA1";
					algorithm = "RC2";
					keyLength = 16;
					break;
				case PKCS12.pbeWithSHAAnd40BitRC2CBC:
					pd.HashName = "SHA1";
					algorithm = "RC2";
					keyLength = 5;
					break;
				default:
					throw new CryptographicException(SR.PKCS12NotValid); // unknown oid : algorithm
			}

			SymmetricAlgorithm sa = SymmetricAlgorithm.Create(algorithm);
			sa.Key = pd.DeriveKey(keyLength);
			// IV required only for block ciphers (not stream ciphers)
			if (ivLength > 0)
			{
				sa.IV = pd.DeriveIV(ivLength);
				sa.Mode = CipherMode.CBC;
			}
			return sa;
		}

		#endregion

		#region Static

		public static bool IsValid(byte[] data)
		{
			try
			{
				ASN1 pfx = new ASN1(data);
				if (pfx.Tag != 0x30)
					return false; // data

				ASN1 version = pfx[0];
				if (version.Tag != 0x02)
					return false; // PFX version

				var authSafe = new PKCS7.ContentInfo(pfx[1]);
				if (authSafe.ContentType != PKCS7.Oid.data)
					return false; // authenticated safe
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		#endregion

		#region SafeBag

		private class SafeBag
		{
			private string _bagOID;
			private ASN1 _asn1;

			public SafeBag(string bagOID, ASN1 asn1)
			{
				_bagOID = bagOID;
				_asn1 = asn1;
			}

			public string BagOID
			{
				get { return _bagOID; }
			}

			public ASN1 ASN1
			{
				get { return _asn1; }
			}
		}

		#endregion

		#region DeriveBytes

		private class DeriveBytes
		{
			public enum Purpose
			{
				Key,
				IV,
				MAC
			}

			static private byte[] keyDiversifier = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
			static private byte[] ivDiversifier = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
			static private byte[] macDiversifier = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };

			private string _hashName;
			private int _iterations;
			private byte[] _password;
			private byte[] _salt;

			public DeriveBytes() { }

			public string HashName
			{
				get { return _hashName; }
				set { _hashName = value; }
			}

			public int IterationCount
			{
				get { return _iterations; }
				set { _iterations = value; }
			}

			public byte[] Password
			{
				get { return (byte[])_password.Clone(); }
				set
				{
					if (value == null)
						_password = BufferUtils.EmptyArray;
					else
						_password = (byte[])value.Clone();
				}
			}

			public byte[] Salt
			{
				get { return (byte[])_salt.Clone(); }
				set
				{
					if (value != null)
						_salt = (byte[])value.Clone();
					else
						_salt = null;
				}
			}

			private void Adjust(byte[] a, int aOff, byte[] b)
			{
				int x = (b[b.Length - 1] & 0xff) + (a[aOff + b.Length - 1] & 0xff) + 1;

				a[aOff + b.Length - 1] = (byte)x;
				x >>= 8;

				for (int i = b.Length - 2; i >= 0; i--)
				{
					x += (b[i] & 0xff) + (a[aOff + i] & 0xff);
					a[aOff + i] = (byte)x;
					x >>= 8;
				}
			}

			private byte[] Derive(byte[] diversifier, int n)
			{
				HashAlgorithm digest = HashAlgorithm.Create(_hashName);
				int u = (digest.HashSize >> 3); // div 8
				int v = 64;
				byte[] dKey = new byte[n];

				byte[] S;
				if ((_salt != null) && (_salt.Length != 0))
				{
					S = new byte[v * ((_salt.Length + v - 1) / v)];

					for (int i = 0; i != S.Length; i++)
					{
						S[i] = _salt[i % _salt.Length];
					}
				}
				else
				{
					S = BufferUtils.EmptyArray;
				}

				byte[] P;
				if ((_password != null) && (_password.Length != 0))
				{
					P = new byte[v * ((_password.Length + v - 1) / v)];

					for (int i = 0; i != P.Length; i++)
					{
						P[i] = _password[i % _password.Length];
					}
				}
				else
				{
					P = BufferUtils.EmptyArray;
				}

				byte[] I = new byte[S.Length + P.Length];

				Buffer.BlockCopy(S, 0, I, 0, S.Length);
				Buffer.BlockCopy(P, 0, I, S.Length, P.Length);

				byte[] B = new byte[v];
				int c = (n + u - 1) / u;

				for (int i = 1; i <= c; i++)
				{
					digest.TransformBlock(diversifier, 0, diversifier.Length, diversifier, 0);
					digest.TransformFinalBlock(I, 0, I.Length);
					byte[] A = digest.Hash;
					digest.Initialize();
					for (int j = 1; j != _iterations; j++)
					{
						A = digest.ComputeHash(A, 0, A.Length);
					}

					for (int j = 0; j != B.Length; j++)
					{
						B[j] = A[j % A.Length];
					}

					for (int j = 0; j != I.Length / v; j++)
					{
						Adjust(I, j * v, B);
					}

					if (i == c)
					{
						Buffer.BlockCopy(A, 0, dKey, (i - 1) * u, dKey.Length - ((i - 1) * u));
					}
					else
					{
						Buffer.BlockCopy(A, 0, dKey, (i - 1) * u, A.Length);
					}
				}

				return dKey;
			}

			public byte[] DeriveKey(int size)
			{
				return Derive(keyDiversifier, size);
			}

			public byte[] DeriveIV(int size)
			{
				return Derive(ivDiversifier, size);
			}

			public byte[] DeriveMAC(int size)
			{
				return Derive(macDiversifier, size);
			}
		}

		#endregion

		#region PKCS5

		private class PKCS5
		{
			public const string pbeWithMD2AndDESCBC = "1.2.840.113549.1.5.1";
			public const string pbeWithMD5AndDESCBC = "1.2.840.113549.1.5.3";
			public const string pbeWithMD2AndRC2CBC = "1.2.840.113549.1.5.4";
			public const string pbeWithMD5AndRC2CBC = "1.2.840.113549.1.5.6";
			public const string pbeWithSHA1AndDESCBC = "1.2.840.113549.1.5.10";
			public const string pbeWithSHA1AndRC2CBC = "1.2.840.113549.1.5.11";
		}

		#endregion

		#region PKCS7

		private class PKCS7
		{
			public static class Oid
			{
				// pkcs 1
				public const string rsaEncryption = "1.2.840.113549.1.1.1";

				// pkcs 7
				public const string data = "1.2.840.113549.1.7.1";
				public const string signedData = "1.2.840.113549.1.7.2";
				public const string envelopedData = "1.2.840.113549.1.7.3";
				public const string signedAndEnvelopedData = "1.2.840.113549.1.7.4";
				public const string digestedData = "1.2.840.113549.1.7.5";
				public const string encryptedData = "1.2.840.113549.1.7.6";

				// pkcs 9
				public const string contentType = "1.2.840.113549.1.9.3";
				public const string messageDigest = "1.2.840.113549.1.9.4";
				public const string signingTime = "1.2.840.113549.1.9.5";
				public const string countersignature = "1.2.840.113549.1.9.6";
			}

			public class ContentInfo
			{
				private string contentType;
				private ASN1 content;

				public ContentInfo()
				{
					content = new ASN1(0xA0);
				}

				public ContentInfo(string oid)
					: this()
				{
					contentType = oid;
				}

				public ContentInfo(byte[] data)
					: this(new ASN1(data))
				{
				}

				public ContentInfo(ASN1 asn1)
				{
					// SEQUENCE with 1 or 2 elements.
					if ((asn1.Tag != 0x30) || ((asn1.Count < 1) && (asn1.Count > 2)))
						throw new CryptographicException(SR.PKCS12NotValid);  // Invalid ASN1
					if (asn1[0].Tag != 0x06)
						throw new CryptographicException(SR.PKCS12NotValid); // Invalid contentType

					contentType = asn1[0].ToOid();

					if (asn1.Count > 1)
					{
						if (asn1[1].Tag != 0xA0)
							throw new CryptographicException(SR.PKCS12NotValid); // Invalid content
						content = asn1[1];
					}
				}

				public ASN1 ASN1
				{
					get { return GetASN1(); }
				}

				public ASN1 Content
				{
					get { return content; }
					set { content = value; }
				}

				public string ContentType
				{
					get { return contentType; }
					set { contentType = value; }
				}

				internal ASN1 GetASN1()
				{
					// ContentInfo ::= SEQUENCE {
					ASN1 contentInfo = new ASN1(0x30);
					// contentType ContentType, -> ContentType ::= OBJECT IDENTIFIER
					contentInfo.Add(ASN1.FromOid(contentType));
					// content [0] EXPLICIT ANY DEFINED BY contentType OPTIONAL
					if ((content != null) && (content.Count > 0))
						contentInfo.Add(content);
					return contentInfo;
				}

				public byte[] GetBytes()
				{
					return GetASN1().GetBytes();
				}
			}

			public class EncryptedData
			{
				private byte _version;
				private ContentInfo _content;
				private ContentInfo _encryptionAlgorithm;
				private byte[] _encrypted;

				public EncryptedData()
				{
					_version = 0;
				}

				public EncryptedData(byte[] data)
					: this(new ASN1(data))
				{
				}

				public EncryptedData(ASN1 asn1)
					: this()
				{
					if ((asn1.Tag != 0x30) || (asn1.Count < 2))
						throw new CryptographicException(SR.PKCS12NotValid); // Invalid EncryptedData

					if (asn1[0].Tag != 0x02)
						throw new CryptographicException(SR.PKCS12NotValid); // Invalid version
					_version = asn1[0].Value[0];

					ASN1 encryptedContentInfo = asn1[1];
					if (encryptedContentInfo.Tag != 0x30)
						throw new CryptographicException(SR.PKCS12NotValid); // missing EncryptedContentInfo

					ASN1 contentType = encryptedContentInfo[0];
					if (contentType.Tag != 0x06)
						throw new CryptographicException(SR.PKCS12NotValid); // missing EncryptedContentInfo.ContentType
					_content = new ContentInfo(contentType.ToOid());

					ASN1 contentEncryptionAlgorithm = encryptedContentInfo[1];
					if (contentEncryptionAlgorithm.Tag != 0x30)
						throw new CryptographicException(SR.PKCS12NotValid); // missing EncryptedContentInfo.ContentEncryptionAlgorithmIdentifier
					_encryptionAlgorithm = new ContentInfo(contentEncryptionAlgorithm[0].ToOid());
					_encryptionAlgorithm.Content = contentEncryptionAlgorithm[1];

					ASN1 encryptedContent = encryptedContentInfo[2];
					if (encryptedContent.Tag != 0x80)
						throw new CryptographicException(SR.PKCS12NotValid);  // missing EncryptedContentInfo.EncryptedContent
					_encrypted = encryptedContent.Value;
				}

				public ASN1 ASN1
				{
					get { return GetASN1(); }
				}

				public ContentInfo ContentInfo
				{
					get { return _content; }
				}

				public ContentInfo EncryptionAlgorithm
				{
					get { return _encryptionAlgorithm; }
				}

				public byte[] EncryptedContent
				{
					get
					{
						if (_encrypted == null)
							return null;
						return (byte[])_encrypted.Clone();
					}
				}

				public byte Version
				{
					get { return _version; }
					set { _version = value; }
				}

				// methods

				internal ASN1 GetASN1()
				{
					return null;
				}

				public byte[] GetBytes()
				{
					return GetASN1().GetBytes();
				}
			}
		}

		#endregion

		#region PKCS8

		private class PKCS8
		{
			public enum KeyInfo
			{
				PrivateKey,
				EncryptedPrivateKey,
				Unknown
			}

			static public KeyInfo GetType(byte[] data)
			{
				if (data == null)
					throw new CryptographicException(SR.PKCS12NotValid); // data

				KeyInfo ki = KeyInfo.Unknown;
				try
				{
					ASN1 top = new ASN1(data);
					if ((top.Tag == 0x30) && (top.Count > 0))
					{
						ASN1 firstLevel = top[0];
						switch (firstLevel.Tag)
						{
							case 0x02:
								ki = KeyInfo.PrivateKey;
								break;
							case 0x30:
								ki = KeyInfo.EncryptedPrivateKey;
								break;
						}
					}
				}
				catch
				{
					throw new CryptographicException(SR.PKCS12NotValid); // invalid ASN.1 data
				}

				return ki;
			}

			/*
			 * PrivateKeyInfo ::= SEQUENCE {
			 *	version Version,
			 *	privateKeyAlgorithm PrivateKeyAlgorithmIdentifier,
			 *	privateKey PrivateKey,
			 *	attributes [0] IMPLICIT Attributes OPTIONAL
			 * }
			 *
			 * Version ::= INTEGER
			 *
			 * PrivateKeyAlgorithmIdentifier ::= AlgorithmIdentifier
			 *
			 * PrivateKey ::= OCTET STRING
			 *
			 * Attributes ::= SET OF Attribute
			 */

			public class PrivateKeyInfo
			{
				private int _version;
				private string _algorithm;
				private byte[] _key;
				private ArrayList _list;

				public PrivateKeyInfo()
				{
					_version = 0;
					_list = new ArrayList();
				}

				public PrivateKeyInfo(byte[] data)
					: this()
				{
					Decode(data);
				}

				// properties

				public string Algorithm
				{
					get { return _algorithm; }
					set { _algorithm = value; }
				}

				public ArrayList Attributes
				{
					get { return _list; }
				}

				public byte[] PrivateKey
				{
					get
					{
						if (_key == null)
							return null;
						return (byte[])_key.Clone();
					}
					set
					{
						if (value == null)
							throw new CryptographicException(SR.PKCS12NotValid); // PrivateKey
						_key = (byte[])value.Clone();
					}
				}

				public int Version
				{
					get { return _version; }
					set
					{
						if (value < 0)
							throw new CryptographicException(SR.PKCS12NotValid); // negative version
						_version = value;
					}
				}

				// methods

				private void Decode(byte[] data)
				{
					ASN1 privateKeyInfo = new ASN1(data);
					if (privateKeyInfo.Tag != 0x30)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid PrivateKeyInfo

					ASN1 version = privateKeyInfo[0];
					if (version.Tag != 0x02)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid version
					_version = version.Value[0];

					ASN1 privateKeyAlgorithm = privateKeyInfo[1];
					if (privateKeyAlgorithm.Tag != 0x30)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid algorithm

					ASN1 algorithm = privateKeyAlgorithm[0];
					if (algorithm.Tag != 0x06)
						throw new CryptographicException(SR.PKCS12NotValid); // missing algorithm OID
					_algorithm = algorithm.ToOid();

					ASN1 privateKey = privateKeyInfo[2];
					_key = privateKey.Value;

					// attributes [0] IMPLICIT Attributes OPTIONAL
					if (privateKeyInfo.Count > 3)
					{
						ASN1 attributes = privateKeyInfo[3];
						for (int i = 0; i < attributes.Count; i++)
						{
							_list.Add(attributes[i]);
						}
					}
				}

				public byte[] GetBytes()
				{
					ASN1 privateKeyAlgorithm = new ASN1(0x30);
					privateKeyAlgorithm.Add(ASN1.FromOid(_algorithm));
					privateKeyAlgorithm.Add(new ASN1(0x05)); // ASN.1 NULL

					ASN1 pki = new ASN1(0x30);
					pki.Add(new ASN1(0x02, new byte[1] { (byte)_version }));
					pki.Add(privateKeyAlgorithm);
					pki.Add(new ASN1(0x04, _key));

					if (_list.Count > 0)
					{
						ASN1 attributes = new ASN1(0xA0);
						foreach (ASN1 attribute in _list)
						{
							attributes.Add(attribute);
						}
						pki.Add(attributes);
					}

					return pki.GetBytes();
				}

				// static methods

				static private byte[] RemoveLeadingZero(byte[] bigInt)
				{
					int start = 0;
					int length = bigInt.Length;
					if (bigInt[0] == 0x00)
					{
						start = 1;
						length--;
					}
					byte[] bi = new byte[length];
					Buffer.BlockCopy(bigInt, start, bi, 0, length);
					return bi;
				}

				static private byte[] Normalize(byte[] bigInt, int length)
				{
					if (bigInt.Length == length)
						return bigInt;
					else if (bigInt.Length > length)
						return RemoveLeadingZero(bigInt);
					else
					{
						// pad with 0
						byte[] bi = new byte[length];
						Buffer.BlockCopy(bigInt, 0, bi, (length - bigInt.Length), bigInt.Length);
						return bi;
					}
				}

				/*
				 * RSAPrivateKey ::= SEQUENCE {
				 *	version           Version,
				 *	modulus           INTEGER,  -- n
				 *	publicExponent    INTEGER,  -- e
				 *	privateExponent   INTEGER,  -- d
				 *	prime1            INTEGER,  -- p
				 *	prime2            INTEGER,  -- q
				 *	exponent1         INTEGER,  -- d mod (p-1)
				 *	exponent2         INTEGER,  -- d mod (q-1)
				 *	coefficient       INTEGER,  -- (inverse of q) mod p
				 *	otherPrimeInfos   OtherPrimeInfos OPTIONAL
				 * }
				 */

				static public RSA DecodeRSA(byte[] keypair)
				{
					ASN1 privateKey = new ASN1(keypair);
					if (privateKey.Tag != 0x30)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid private key format

					ASN1 version = privateKey[0];
					if (version.Tag != 0x02)
						throw new CryptographicException(SR.PKCS12NotValid); // missing version

					if (privateKey.Count < 9)
						throw new CryptographicException(SR.PKCS12NotValid); // not enough key parameters

					RSAParameters param = new RSAParameters();
					// note: MUST remove leading 0 - else MS wont import the key
					param.Modulus = RemoveLeadingZero(privateKey[1].Value);
					int keysize = param.Modulus.Length;
					int keysize2 = (keysize >> 1); // half-size
					// size must be normalized - else MS wont import the key
					param.D = Normalize(privateKey[3].Value, keysize);
					param.DP = Normalize(privateKey[6].Value, keysize2);
					param.DQ = Normalize(privateKey[7].Value, keysize2);
					param.Exponent = RemoveLeadingZero(privateKey[2].Value);
					param.InverseQ = Normalize(privateKey[8].Value, keysize2);
					param.P = Normalize(privateKey[4].Value, keysize2);
					param.Q = Normalize(privateKey[5].Value, keysize2);

					RSA rsa = null;
					try
					{
						rsa = RSA.Create();
						rsa.ImportParameters(param);
					}
					catch (CryptographicException)
					{
						// this may cause problem when this code is run under
						// the SYSTEM identity on Windows (e.g. ASP.NET). See
						// http://bugzilla.ximian.com/show_bug.cgi?id=77559
						CspParameters csp = new CspParameters();
						csp.Flags = CspProviderFlags.UseMachineKeyStore;
						rsa = new RSACryptoServiceProvider(csp);
						rsa.ImportParameters(param);
					}
					return rsa;
				}

				/*
				 * RSAPrivateKey ::= SEQUENCE {
				 *	version           Version,
				 *	modulus           INTEGER,  -- n
				 *	publicExponent    INTEGER,  -- e
				 *	privateExponent   INTEGER,  -- d
				 *	prime1            INTEGER,  -- p
				 *	prime2            INTEGER,  -- q
				 *	exponent1         INTEGER,  -- d mod (p-1)
				 *	exponent2         INTEGER,  -- d mod (q-1)
				 *	coefficient       INTEGER,  -- (inverse of q) mod p
				 *	otherPrimeInfos   OtherPrimeInfos OPTIONAL
				 * }
				 */

				static public byte[] Encode(RSA rsa)
				{
					RSAParameters param = rsa.ExportParameters(true);

					ASN1 rsaPrivateKey = new ASN1(0x30);
					rsaPrivateKey.Add(new ASN1(0x02, new byte[1] { 0x00 }));
					rsaPrivateKey.Add(ASN1.FromUnsignedBigInteger(param.Modulus));
					rsaPrivateKey.Add(ASN1.FromUnsignedBigInteger(param.Exponent));
					rsaPrivateKey.Add(ASN1.FromUnsignedBigInteger(param.D));
					rsaPrivateKey.Add(ASN1.FromUnsignedBigInteger(param.P));
					rsaPrivateKey.Add(ASN1.FromUnsignedBigInteger(param.Q));
					rsaPrivateKey.Add(ASN1.FromUnsignedBigInteger(param.DP));
					rsaPrivateKey.Add(ASN1.FromUnsignedBigInteger(param.DQ));
					rsaPrivateKey.Add(ASN1.FromUnsignedBigInteger(param.InverseQ));

					return rsaPrivateKey.GetBytes();
				}

				// DSA only encode it's X private key inside an ASN.1 INTEGER (Hint: Tag == 0x02)
				// which isn't enough for rebuilding the keypair. The other parameters
				// can be found (98% of the time) in the X.509 certificate associated
				// with the private key or (2% of the time) the parameters are in it's
				// issuer X.509 certificate (not supported in the .NET framework).
				static public DSA DecodeDSA(byte[] privateKey, DSAParameters dsaParameters)
				{
					ASN1 pvk = new ASN1(privateKey);
					if (pvk.Tag != 0x02)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid private key format

					// X is ALWAYS 20 bytes (no matter if the key length is 512 or 1024 bits)
					dsaParameters.X = Normalize(pvk.Value, 20);
					DSA dsa = DSA.Create();
					dsa.ImportParameters(dsaParameters);
					return dsa;
				}

				static public byte[] Encode(DSA dsa)
				{
					DSAParameters param = dsa.ExportParameters(true);
					return ASN1.FromUnsignedBigInteger(param.X).GetBytes();
				}

				static public byte[] Encode(AsymmetricAlgorithm aa)
				{
					if (aa is RSA)
						return Encode((RSA)aa);
					else if (aa is DSA)
						return Encode((DSA)aa);
					else
						throw new CryptographicException(SR.PKCS12NotValid); // Unknown asymmetric algorithm {0}", aa.ToString());
				}
			}

			/*
			 * EncryptedPrivateKeyInfo ::= SEQUENCE {
			 *	encryptionAlgorithm EncryptionAlgorithmIdentifier,
			 *	encryptedData EncryptedData
			 * }
			 *
			 * EncryptionAlgorithmIdentifier ::= AlgorithmIdentifier
			 *
			 * EncryptedData ::= OCTET STRING
			 *
			 * --
			 *  AlgorithmIdentifier  ::= SEQUENCE {
			 *	algorithm  OBJECT IDENTIFIER,
			 *	parameters ANY DEFINED BY algorithm OPTIONAL
			 * }
			 *
			 * -- from PKCS#5
			 * PBEParameter ::= SEQUENCE {
			 *	salt OCTET STRING SIZE(8),
			 *	iterationCount INTEGER
			 * }
			 */

			public class EncryptedPrivateKeyInfo
			{
				private string _algorithm;
				private byte[] _salt;
				private int _iterations;
				private byte[] _data;

				public EncryptedPrivateKeyInfo() { }

				public EncryptedPrivateKeyInfo(byte[] data)
					: this()
				{
					Decode(data);
				}

				// properties

				public string Algorithm
				{
					get { return _algorithm; }
					set { _algorithm = value; }
				}

				public byte[] EncryptedData
				{
					get { return (_data == null) ? null : (byte[])_data.Clone(); }
					set { _data = (value == null) ? null : (byte[])value.Clone(); }
				}

				public byte[] Salt
				{
					get
					{
						if (_salt == null)
						{
							RandomNumberGenerator rng = RandomNumberGenerator.Create();
							_salt = new byte[8];
							rng.GetBytes(_salt);
						}
						return (byte[])_salt.Clone();
					}
					set { _salt = (byte[])value.Clone(); }
				}

				public int IterationCount
				{
					get { return _iterations; }
					set
					{
						if (value < 0)
							throw new CryptographicException(SR.PKCS12NotValid);
						_iterations = value;
					}
				}

				// methods

				private void Decode(byte[] data)
				{
					ASN1 encryptedPrivateKeyInfo = new ASN1(data);
					if (encryptedPrivateKeyInfo.Tag != 0x30)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid EncryptedPrivateKeyInfo

					ASN1 encryptionAlgorithm = encryptedPrivateKeyInfo[0];
					if (encryptionAlgorithm.Tag != 0x30)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid encryptionAlgorithm
					ASN1 algorithm = encryptionAlgorithm[0];
					if (algorithm.Tag != 0x06)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid algorithm
					_algorithm = algorithm.ToOid();
					// parameters ANY DEFINED BY algorithm OPTIONAL
					if (encryptionAlgorithm.Count > 1)
					{
						ASN1 parameters = encryptionAlgorithm[1];
						if (parameters.Tag != 0x30)
							throw new CryptographicException(SR.PKCS12NotValid); // invalid parameters

						ASN1 salt = parameters[0];
						if (salt.Tag != 0x04)
							throw new CryptographicException(SR.PKCS12NotValid); // invalid salt
						_salt = salt.Value;

						ASN1 iterationCount = parameters[1];
						if (iterationCount.Tag != 0x02)
							throw new CryptographicException(SR.PKCS12NotValid); // invalid iterationCount
						_iterations = iterationCount.ToInt32();
					}

					ASN1 encryptedData = encryptedPrivateKeyInfo[1];
					if (encryptedData.Tag != 0x04)
						throw new CryptographicException(SR.PKCS12NotValid); // invalid EncryptedData
					_data = encryptedData.Value;
				}

				// Note: PKCS#8 doesn't define how to generate the key required for encryption
				// so you're on your own. Just don't try to copy the big guys too much ;)
				// Netscape:	http://www.cs.auckland.ac.nz/~pgut001/pubs/netscape.txt
				// Microsoft:	http://www.cs.auckland.ac.nz/~pgut001/pubs/breakms.txt
				public byte[] GetBytes()
				{
					if (_algorithm == null)
						throw new CryptographicException(SR.PKCS12NotValid); // No algorithm OID specified

					ASN1 encryptionAlgorithm = new ASN1(0x30);
					encryptionAlgorithm.Add(ASN1.FromOid(_algorithm));

					// parameters ANY DEFINED BY algorithm OPTIONAL
					if ((_iterations > 0) || (_salt != null))
					{
						ASN1 salt = new ASN1(0x04, _salt);
						ASN1 iterations = ASN1.FromInt32(_iterations);

						ASN1 parameters = new ASN1(0x30);
						parameters.Add(salt);
						parameters.Add(iterations);
						encryptionAlgorithm.Add(parameters);
					}

					// encapsulates EncryptedData into an OCTET STRING
					ASN1 encryptedData = new ASN1(0x04, _data);

					ASN1 encryptedPrivateKeyInfo = new ASN1(0x30);
					encryptedPrivateKeyInfo.Add(encryptionAlgorithm);
					encryptedPrivateKeyInfo.Add(encryptedData);

					return encryptedPrivateKeyInfo.GetBytes();
				}
			}
		}

		#endregion

		#region PKCS9

		private class PKCS9
		{
			public const string friendlyName = "1.2.840.113549.1.9.20";
			public const string localKeyId = "1.2.840.113549.1.9.21";
		}

		#endregion

		#region ASN1

		/// <summary>
		/// Abstract Syntax Notation 1 - micro-parser and generator.
		/// </summary>
		private class ASN1
		{
			private byte m_nTag;
			private byte[] m_aValue;
			private ArrayList elist;

			public ASN1() : this(0x00, null) { }

			public ASN1(byte tag) : this(tag, null) { }

			public ASN1(byte tag, byte[] data)
			{
				m_nTag = tag;
				m_aValue = data;
			}

			public ASN1(byte[] data)
			{
				m_nTag = data[0];

				int nLenLength = 0;
				int nLength = data[1];

				if (nLength > 0x80)
				{
					// composed length
					nLenLength = nLength - 0x80;
					nLength = 0;
					for (int i = 0; i < nLenLength; i++)
					{
						nLength *= 256;
						nLength += data[i + 2];
					}
				}
				else if (nLength == 0x80)
				{
					// undefined length encoding
					throw new CryptographicException(SR.PKCS12NotValid); // Undefined length encoding.
				}

				m_aValue = new byte[nLength];
				Buffer.BlockCopy(data, (2 + nLenLength), m_aValue, 0, nLength);

				if ((m_nTag & 0x20) == 0x20)
				{
					int nStart = (2 + nLenLength);
					Decode(data, ref nStart, data.Length);
				}
			}

			public int Count
			{
				get
				{
					if (elist == null)
						return 0;
					return elist.Count;
				}
			}

			public byte Tag
			{
				get { return m_nTag; }
			}

			public int Length
			{
				get
				{
					if (m_aValue != null)
						return m_aValue.Length;
					else
						return 0;
				}
			}

			public byte[] Value
			{
				get
				{
					if (m_aValue == null)
						GetBytes();
					return (byte[])m_aValue.Clone();
				}
				set
				{
					if (value != null)
						m_aValue = (byte[])value.Clone();
				}
			}

			private bool CompareArray(byte[] array1, byte[] array2)
			{
				bool bResult = (array1.Length == array2.Length);
				if (bResult)
				{
					for (int i = 0; i < array1.Length; i++)
					{
						if (array1[i] != array2[i])
							return false;
					}
				}
				return bResult;
			}

			public bool Equals(byte[] asn1)
			{
				return CompareArray(this.GetBytes(), asn1);
			}

			public bool CompareValue(byte[] value)
			{
				return CompareArray(m_aValue, value);
			}

			public ASN1 Add(ASN1 asn1)
			{
				if (asn1 != null)
				{
					if (elist == null)
						elist = new ArrayList();
					elist.Add(asn1);
				}
				return asn1;
			}

			public virtual byte[] GetBytes()
			{
				byte[] val = null;

				if (Count > 0)
				{
					int esize = 0;
					ArrayList al = new ArrayList();
					foreach (ASN1 a in elist)
					{
						byte[] item = a.GetBytes();
						al.Add(item);
						esize += item.Length;
					}
					val = new byte[esize];
					int pos = 0;
					for (int i = 0; i < elist.Count; i++)
					{
						byte[] item = (byte[])al[i];
						Buffer.BlockCopy(item, 0, val, pos, item.Length);
						pos += item.Length;
					}
				}
				else if (m_aValue != null)
				{
					val = m_aValue;
				}

				byte[] der;
				int nLengthLen = 0;

				if (val != null)
				{
					int nLength = val.Length;
					// special for length > 127
					if (nLength > 127)
					{
						if (nLength <= Byte.MaxValue)
						{
							der = new byte[3 + nLength];
							Buffer.BlockCopy(val, 0, der, 3, nLength);
							nLengthLen = 0x81;
							der[2] = (byte)(nLength);
						}
						else if (nLength <= UInt16.MaxValue)
						{
							der = new byte[4 + nLength];
							Buffer.BlockCopy(val, 0, der, 4, nLength);
							nLengthLen = 0x82;
							der[2] = (byte)(nLength >> 8);
							der[3] = (byte)(nLength);
						}
						else if (nLength <= 0xFFFFFF)
						{
							// 24 bits
							der = new byte[5 + nLength];
							Buffer.BlockCopy(val, 0, der, 5, nLength);
							nLengthLen = 0x83;
							der[2] = (byte)(nLength >> 16);
							der[3] = (byte)(nLength >> 8);
							der[4] = (byte)(nLength);
						}
						else
						{
							// max (Length is an integer) 32 bits
							der = new byte[6 + nLength];
							Buffer.BlockCopy(val, 0, der, 6, nLength);
							nLengthLen = 0x84;
							der[2] = (byte)(nLength >> 24);
							der[3] = (byte)(nLength >> 16);
							der[4] = (byte)(nLength >> 8);
							der[5] = (byte)(nLength);
						}
					}
					else
					{
						// basic case (no encoding)
						der = new byte[2 + nLength];
						Buffer.BlockCopy(val, 0, der, 2, nLength);
						nLengthLen = nLength;
					}
					if (m_aValue == null)
						m_aValue = val;
				}
				else
					der = new byte[2];

				der[0] = m_nTag;
				der[1] = (byte)nLengthLen;

				return der;
			}

			// Note: Recursive
			protected void Decode(byte[] asn1, ref int anPos, int anLength)
			{
				byte nTag;
				int nLength;
				byte[] aValue;

				// minimum is 2 bytes (tag + length of 0)
				while (anPos < anLength - 1)
				{
					DecodeTLV(asn1, ref anPos, out nTag, out nLength, out aValue);
					// sometimes we get trailing 0
					if (nTag == 0)
						continue;

					ASN1 elm = Add(new ASN1(nTag, aValue));

					if ((nTag & 0x20) == 0x20)
					{
						int nConstructedPos = anPos;
						elm.Decode(asn1, ref nConstructedPos, nConstructedPos + nLength);
					}
					anPos += nLength; // value length
				}
			}

			// TLV : Tag - Length - Value
			protected void DecodeTLV(byte[] asn1, ref int pos, out byte tag, out int length, out byte[] content)
			{
				tag = asn1[pos++];
				length = asn1[pos++];

				// special case where L contains the Length of the Length + 0x80
				if ((length & 0x80) == 0x80)
				{
					int nLengthLen = length & 0x7F;
					length = 0;
					for (int i = 0; i < nLengthLen; i++)
						length = length * 256 + asn1[pos++];
				}

				content = new byte[length];
				Buffer.BlockCopy(asn1, pos, content, 0, length);
			}

			public ASN1 this[int index]
			{
				get
				{
					try
					{
						if ((elist == null) || (index >= elist.Count))
							return null;
						return (ASN1)elist[index];
					}
					catch (ArgumentOutOfRangeException)
					{
						return null;
					}
				}
			}

			public ASN1 Element(int index, byte anTag)
			{
				try
				{
					if ((elist == null) || (index >= elist.Count))
						return null;

					ASN1 elm = (ASN1)elist[index];
					if (elm.Tag == anTag)
						return elm;
					else
						return null;
				}
				catch (ArgumentOutOfRangeException)
				{
					return null;
				}
			}

			public override string ToString()
			{
				StringBuilder hexLine = new StringBuilder();

				// Add tag
				hexLine.AppendFormat("Tag: {0} {1}", m_nTag.ToString("X2"), Environment.NewLine);

				// Add length
				hexLine.AppendFormat("Length: {0} {1}", Value.Length, Environment.NewLine);

				// Add value
				hexLine.Append("Value: ");
				hexLine.Append(Environment.NewLine);
				for (int i = 0; i < Value.Length; i++)
				{
					hexLine.AppendFormat("{0} ", Value[i].ToString("X2"));
					if ((i + 1) % 16 == 0)
						hexLine.AppendFormat(Environment.NewLine);
				}
				return hexLine.ToString();
			}

			public string ToOid()
			{
				byte[] aOID = Value;
				StringBuilder sb = new StringBuilder();
				// Pick apart the OID
				byte x = (byte)(aOID[0] / 40);
				byte y = (byte)(aOID[0] % 40);
				if (x > 2)
				{
					// Handle special case for large y if x = 2
					y += (byte)((x - 2) * 40);
					x = 2;
				}
				sb.Append(x.ToString(CultureInfo.InvariantCulture));
				sb.Append(".");
				sb.Append(y.ToString(CultureInfo.InvariantCulture));
				ulong val = 0;
				for (x = 1; x < aOID.Length; x++)
				{
					val = ((val << 7) | ((byte)(aOID[x] & 0x7F)));
					if (!((aOID[x] & 0x80) == 0x80))
					{
						sb.Append(".");
						sb.Append(val.ToString(CultureInfo.InvariantCulture));
						val = 0;
					}
				}
				return sb.ToString();
			}

			public int ToInt32()
			{
				if (Tag != 0x02)
					throw new CryptographicException(SR.PKCS12NotValid); // Only integer can be converted

				int x = 0;
				for (int i = 0; i < Value.Length; i++)
					x = (x << 8) + Value[i];
				return x;
			}

			public static ASN1 FromOid(string oid)
			{
				if (oid == null)
					throw new CryptographicException(SR.PKCS12NotValid); // oid

				return new ASN1(CryptoConfig.EncodeOID(oid));
			}

			public static ASN1 FromInt32(int value)
			{
				byte[] integer = BitConverter.GetBytes(value);
				Array.Reverse(integer);
				int x = 0;
				while ((x < integer.Length) && (integer[x] == 0x00))
					x++;
				ASN1 asn1 = new ASN1(0x02);
				switch (x)
				{
					case 0:
						asn1.Value = integer;
						break;
					case 4:
						asn1.Value = new byte[1];
						break;
					default:
						byte[] smallerInt = new byte[4 - x];
						Buffer.BlockCopy(integer, x, smallerInt, 0, smallerInt.Length);
						asn1.Value = smallerInt;
						break;
				}
				return asn1;
			}

			public static ASN1 FromUnsignedBigInteger(byte[] big)
			{
				if (big == null)
					throw new CryptographicException(SR.PKCS12NotValid); // big

				// check for numbers that could be interpreted as negative (first bit)
				if (big[0] >= 0x80)
				{
					// in thie cas we add a new, empty, byte (position 0) so we're
					// sure this will always be interpreted an unsigned integer.
					// However we can't feed it into RSAParameters or DSAParameters
					int length = big.Length + 1;
					byte[] uinteger = new byte[length];
					Buffer.BlockCopy(big, 0, uinteger, 1, length - 1);
					big = uinteger;
				}
				return new ASN1(0x02, big);
			}
		}

		#endregion
	}
}
