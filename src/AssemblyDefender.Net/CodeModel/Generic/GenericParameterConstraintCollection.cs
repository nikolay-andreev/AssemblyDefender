using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class GenericParameterConstraintCollection : SignatureCollection<TypeSignature>
	{
		#region Fields

		private GenericParameter _genericParameter;

		#endregion

		#region Ctors

		internal GenericParameterConstraintCollection(GenericParameter genericParameter)
			: base(genericParameter)
		{
			_genericParameter = genericParameter;

			if (genericParameter.IsNew)
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

		public GenericParameter GenericParameter
		{
			get { return _genericParameter; }
		}

		#endregion

		#region Methods

		public void CopyTo(GenericParameterConstraintCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		private void Load()
		{
			var image = _module.Image;

			int[] constraintTokens;
			image.GetGenericParamConstraintsByOwner(_genericParameter.RID, out constraintTokens);

			int count = constraintTokens.Length;
			_list.Capacity = count;

			for (int i = 0; i < count; i++)
			{
				_list.Add(TypeSignature.Load(_module, MetadataToken.DecompressTypeDefOrRef(constraintTokens[i])));
			}
		}

		#endregion
	}
}
