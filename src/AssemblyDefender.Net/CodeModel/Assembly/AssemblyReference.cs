using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class AssemblyReference : Signature, IAssemblySignature
	{
		#region Fields

		private string _name;
		private string _culture;
		private Version _version;
		private byte[] _publicKeyToken;
		private ProcessorArchitecture _processorArchitecture;

		#endregion

		#region Ctors

		private AssemblyReference()
		{
		}

		public AssemblyReference(string name)
		{
			_name = name.NullIfEmpty();
		}

		public AssemblyReference(string name, string culture, Version version, byte[] publicKeyToken)
			: this(name)
		{
			_culture = culture.NullIfEmpty();
			_version = version;
			_publicKeyToken = publicKeyToken.NullIfEmpty();
		}

		public AssemblyReference(
			string name, string culture, Version version, byte[] publicKeyToken,
			ProcessorArchitecture processorArchitecture)
			: this(name, culture, version, publicKeyToken)
		{
			_processorArchitecture = processorArchitecture;
		}

		#endregion

		#region Properties

		public bool IsStrongNameSigned
		{
			get { return _publicKeyToken != null; }
		}

		public string Name
		{
			get { return _name; }
		}

		public string Culture
		{
			get { return _culture; }
		}

		public Version Version
		{
			get { return _version; }
		}

		public byte[] PublicKeyToken
		{
			get { return _publicKeyToken; }
		}

		/// <summary>
		/// Gets or sets a value that identifies the processor and bits-per-word of the platform targeted by an executable.
		/// </summary>
		public ProcessorArchitecture ProcessorArchitecture
		{
			get { return _processorArchitecture; }
		}

		public override SignatureType SignatureType
		{
			get { return SignatureType.Assembly; }
		}

		#endregion

		#region Methods

		public IAssembly Resolve(IModule context, bool throwOnFailure = false)
		{
			return context.AssemblyManager.Resolve(this, context, throwOnFailure);
		}

		public override string ToString()
		{
			return this.ToString(SignaturePrintingFlags.None);
		}

		#endregion

		#region Static

		public static AssemblyReference Parse(string value, bool throwOnError = false)
		{
			var assemblyRef = (new ReflectionSignatureParser(value)).ParseAssembly();
			if (assemblyRef == null)
			{
				if (throwOnError)
				{
					throw new IdentityParseException(string.Format(SR.AssemblyIdentityParseError, value ?? ""));
				}

				return null;
			}

			return assemblyRef;
		}

		public static AssemblyReference GetAssemblyName(string filePath, bool throwOnFailure = false)
		{
			try
			{
				return GetAssemblyName(PEImage.LoadFile(filePath));
			}
			catch (Exception ex)
			{
				if (throwOnFailure)
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, filePath), ex);
				}

				return null;
			}
		}

		public static AssemblyReference GetAssemblyName(PEImage pe)
		{
			var metadata = MemoryMappedMetadata.Load(pe);

			AssemblyRow row;
			metadata.GetAssembly(1, out row);

			var assemblyRef = new AssemblyReference();
			assemblyRef._name = metadata.GetString(row.Name);
			assemblyRef._culture = metadata.GetString(row.Locale);
			assemblyRef._processorArchitecture = (ProcessorArchitecture)((int)(row.Flags & AssemblyFlags.PA_Mask) >> (int)AssemblyFlags.PA_Shift);
			assemblyRef._version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);

			var publicKey = metadata.GetBlob(row.PublicKey);
			if (publicKey != null && publicKey.Length > 0)
			{
				assemblyRef._publicKeyToken = StrongNameUtils.CreateTokenFromPublicKey(publicKey);
			}

			return assemblyRef;
		}

		public static AssemblyReference GetMscorlib(IAssembly assembly)
		{
			return assembly.Framework.MscorlibAssembly;
		}

		public static AssemblyReference GetSystem(IAssembly assembly)
		{
			return assembly.Framework.SystemAssembly;
		}

		internal static AssemblyReference LoadDef(Module module)
		{
			var image = module.Image;

			var assemblyRef = image.AssemblyDefSignature;
			if (assemblyRef == null)
				return assemblyRef;

			AssemblyRow row;
			image.GetAssembly(1, out row);

			assemblyRef = new AssemblyReference();
			assemblyRef._name = image.GetString(row.Name);
			assemblyRef._culture = image.GetString(row.Locale);
			assemblyRef._processorArchitecture = (ProcessorArchitecture)((int)(row.Flags & AssemblyFlags.PA_Mask) >> (int)AssemblyFlags.PA_Shift);
			assemblyRef._version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);

			var publicKey = image.GetBlob(row.PublicKey);
			if (publicKey != null && publicKey.Length > 0)
			{
				assemblyRef._publicKeyToken = StrongNameUtils.CreateTokenFromPublicKey(publicKey);
			}

			module.AddSignature(ref assemblyRef);
			image.AssemblyDefSignature = assemblyRef;

			return assemblyRef;
		}

		internal static AssemblyReference LoadRef(Module module, int rid)
		{
			var image = module.Image;

			var assemblyRef = image.AssemblyRefSignatures[rid - 1];
			if (assemblyRef != null)
				return assemblyRef;

			AssemblyRefRow row;
			image.GetAssemblyRef(rid, out row);

			assemblyRef = new AssemblyReference();
			assemblyRef._name = image.GetString(row.Name);
			assemblyRef._culture = image.GetString(row.Locale);
			assemblyRef._processorArchitecture = (ProcessorArchitecture)((int)(row.Flags & AssemblyFlags.PA_Mask) >> (int)AssemblyFlags.PA_Shift);
			assemblyRef._version = new Version(row.MajorVersion, row.MinorVersion, row.BuildNumber, row.RevisionNumber);

			byte[] publicKeyToken = image.GetBlob(row.PublicKeyOrToken).NullIfEmpty();
			if ((row.Flags & AssemblyFlags.PublicKey) == AssemblyFlags.PublicKey &&
				publicKeyToken != null && publicKeyToken.Length > 0)
			{
				try
				{
					publicKeyToken = StrongNameUtils.CreateTokenFromPublicKey(publicKeyToken);
				}
				catch (InvalidOperationException)
				{
					publicKeyToken = null;
				}
			}

			assemblyRef._publicKeyToken = publicKeyToken;

			module.AddSignature(ref assemblyRef);
			image.AssemblyRefSignatures[rid - 1] = assemblyRef;

			return assemblyRef;
		}

		#endregion
	}
}
