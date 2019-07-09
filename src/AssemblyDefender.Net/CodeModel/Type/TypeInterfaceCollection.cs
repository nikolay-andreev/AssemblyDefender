using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	public class TypeInterfaceCollection : SignatureCollection<TypeSignature>
	{
		#region Fields

		private TypeDeclaration _type;

		#endregion

		#region Ctors

		internal TypeInterfaceCollection(TypeDeclaration type)
			: base(type)
		{
			_type = type;

			if (type.IsNew)
			{
				IsNew = true;
			}
			else
			{
				Load();
			}
		}

		#endregion

		#region Properties

		public TypeDeclaration Type
		{
			get { return _type; }
		}

		#endregion

		#region Methods

		public void CopyTo(TypeInterfaceCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		private void Load()
		{
			var image = _module.Image;

			int[] interfaceTokens;
			image.GetInterfaceImplsByClass(_type.RID, out interfaceTokens);

			int count = interfaceTokens.Length;
			_list.Capacity = count;

			for (int i = 0; i < count; i++)
			{
				_list.Add(TypeSignature.Load(_module, MetadataToken.DecompressTypeDefOrRef(interfaceTokens[i])));
			}
		}

		#endregion
	}
}
