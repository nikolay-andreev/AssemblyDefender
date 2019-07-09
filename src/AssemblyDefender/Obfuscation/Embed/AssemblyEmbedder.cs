using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class AssemblyEmbedder
	{
		#region Fields

		private int _dataID;
		private Module _module;
		private ResourceStorage _storage;
		private List<EmbeddedAssembly> _assemblies = new List<EmbeddedAssembly>();

		#endregion

		#region Ctors

		public AssemblyEmbedder(BuildModule module)
		{
			if (module == null)
				throw new ArgumentNullException("module");

			_module = module;
			_storage = module.ResourceStorage;
		}

		#endregion

		#region Properties

		public int DataID
		{
			get { return _dataID; }
		}

		#endregion

		#region Methods

		public void AddAssembly(byte[] rawAssembly, bool encrypt = true, bool compress = true)
		{
			var embeddedAssembly = new EmbeddedAssembly();

			var assemblyRef = AssemblyReference.GetAssemblyName(PEImage.Load(rawAssembly, null));
			embeddedAssembly.Name = assemblyRef.Name;
			embeddedAssembly.DataID = _storage.Add(rawAssembly, encrypt, compress);
			_assemblies.Add(embeddedAssembly);
		}

		public void Embed()
		{
			if (_assemblies.Count == 0)
				return;

			int pos = 0;
			var blob = new Blob();
			blob.Write7BitEncodedInt(ref pos, _assemblies.Count);

			foreach (var embeddedAssembly in _assemblies)
			{
				blob.Write(ref pos, (int)embeddedAssembly.DataID);
				blob.WriteLengthPrefixedString(ref pos, (string)embeddedAssembly.Name.ToLower(), Encoding.UTF8);
			}

			_dataID = _storage.Add(blob.GetBuffer(), 0, blob.Length, true, true);
		}

		#endregion

		#region Nested types

		private class EmbeddedAssembly
		{
			internal string Name;
			internal int DataID;
		}

		#endregion
	}
}
