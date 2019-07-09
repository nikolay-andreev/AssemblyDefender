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
	public class CustomAttributeCollection : MemberNodeCollection<CustomAttribute>
	{
		#region Ctors

		internal CustomAttributeCollection(CodeNode parent, int parentToken)
			: base(parent)
		{
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

		public void CopyTo(CustomAttributeCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override CustomAttribute GetItem(int rid)
		{
			return _module.CustomAttributeTable.Get(rid, 0);
		}

		protected override CustomAttribute CreateItem()
		{
			return _module.CreateCustomAttribute(0, 0);
		}

		private void Load(int parentToken)
		{
			var image = _module.Image;

			int[] rids;
			image.GetCustomAttributesByParent(MetadataToken.CompressHasCustomAttribute(parentToken), out rids);

			_list.Capacity = rids.Length;
			_list.AddRange(rids);
		}

		#endregion
	}
}
