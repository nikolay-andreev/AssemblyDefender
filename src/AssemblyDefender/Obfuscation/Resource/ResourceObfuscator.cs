using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	public class ResourceObfuscator
	{
		#region Fields

		public const int ResourceSignature = 0x53524441;
		private BuildAssembly _assembly;
		private Random _random;
		private Dictionary<Assembly, Assembly> _assemblyToResourceAssembly;

		#endregion

		#region Ctors

		public ResourceObfuscator(BuildAssembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			_assembly = assembly;
			_random = assembly.RandomGenerator;
		}

		#endregion

		#region Methods

		public void Obfuscate()
		{
			Assembly resourceAssembly = null;

			for (int i = _assembly.Resources.Count - 1; i >= 0; i--)
			{
				var resource = (BuildResource)_assembly.Resources[i];
				if (!resource.Obfuscate)
					continue;

				foreach (var satelliteResource in resource.SatelliteResources)
				{
					ObfuscateSatelliteResource(satelliteResource);
				}

				if (resourceAssembly == null)
				{
					resourceAssembly = CreateResourceAssembly();
				}

				resource.CopyTo(resourceAssembly.Resources.Add());
				_assembly.Resources.RemoveAt(i);
			}

			if (resourceAssembly == null)
				return;

			AddResourceAssembly(_assembly, resourceAssembly);

			if (_assemblyToResourceAssembly != null)
			{
				foreach (var kvp in _assemblyToResourceAssembly)
				{
					AddResourceAssembly(kvp.Key, kvp.Value);
				}
			}
		}

		private void ObfuscateSatelliteResource(Resource resource)
		{
			if (_assemblyToResourceAssembly == null)
			{
				_assemblyToResourceAssembly = new Dictionary<Assembly, Assembly>();
			}

			var assembly = resource.Assembly;

			Assembly resourceAssembly;
			if (!_assemblyToResourceAssembly.TryGetValue(assembly, out resourceAssembly))
			{
				resourceAssembly = CreateResourceAssembly();
				_assemblyToResourceAssembly.Add(assembly, resourceAssembly);
			}

			int index = assembly.Resources.IndexOf(resource);
			resource.CopyTo(resourceAssembly.Resources.Add());
			assembly.Resources.RemoveAt(index);
		}

		private Assembly CreateResourceAssembly()
		{
			string assemblyName = _random.NextString(12);

			var assembly = new Assembly(_assembly.AssemblyManager);
			assembly.Name = assemblyName;

			var module = assembly.Module;
			module.Name = assemblyName + ".dll";

			// Add global type.
			var globalType = module.Types.Add();
			globalType.Name = CodeModelUtils.GlobalTypeName;

			return assembly;
		}

		private void AddResourceAssembly(Assembly assembly, Assembly resourceAssembly)
		{
			var assembler = new Assembler(resourceAssembly.Module);
			assembler.InitDefaultDLL();
			assembler.Build();

			byte[] resourceAssemblyBytes = assembler.Save();

			var resource = assembly.Resources.Add();
			resource.Name = _random.NextString(12);
			resource.Visibility = ResourceVisibilityFlags.Public;
			resource.SetData(GetResourceBytes(resourceAssemblyBytes));
		}

		private byte[] GetResourceBytes(byte[] buffer)
		{
			byte flags = 0;
			int encryptKey = _random.Next(100, int.MaxValue);

			// Compress
			if (buffer.Length > 30)
			{
				buffer = CompressionUtils.GZipCompress(buffer);
				flags |= 1;
			}

			// Encrypt
			StrongCryptoUtils.Encrypt(buffer, encryptKey);

			int pos = 0;
			var blob = new Blob(buffer.Length + 14);
			blob.Write(ref pos, (int)ResourceSignature); // Signature
			blob.Write(ref pos, (int)StrongCryptoUtils.ComputeHash(buffer)); // Hash
			blob.Write(ref pos, (int)encryptKey); // Encrypt Key
			blob.Write(ref pos, (byte)flags); // Encrypt Key
			blob.Write(ref pos, (byte)0); // Unused
			blob.Write(ref pos, (byte[])buffer); // Data

			return blob.ToArray();
		}

		#endregion

		#region Static

		public static void Obfuscate(BuildAssembly assembly)
		{
			var obfuscator = new ResourceObfuscator(assembly);
			obfuscator.Obfuscate();
		}

		#endregion
	}
}
