using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class GenericParameterCollection : CodeNodeCollection<GenericParameter>
	{
		#region Fields

		private MemberNode _owner;

		#endregion

		#region Ctors

		protected internal GenericParameterCollection(MemberNode parent, int parentToken)
			: base(parent)
		{
			_owner = parent;

			if (parent.IsNew)
			{
				IsNew = true;
			}
			else
			{
				Load(parentToken);
			}
		}

		#endregion

		#region Methods

		public void CopyTo(GenericParameterCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override GenericParameter CreateItem()
		{
			return new GenericParameter(this, _owner);
		}

		private void Load(int parentToken)
		{
			var image = _module.Image;

			int[] rids;
			image.GetGenericParamsByOwner(MetadataToken.CompressTypeOrMethodDef(parentToken), out rids);

			int count = rids.Length;
			_list.Capacity = count;

			for (int i = 0; i < count; i++)
			{
				_list.Add(new GenericParameter(_parent, _owner, rids[i]));
			}
		}

		#endregion
	}
}
