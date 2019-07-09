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
	public class SecurityAttributeCollection : MemberNodeCollection<SecurityAttribute>
	{
		#region Ctors

		internal SecurityAttributeCollection(CodeNode parent, int parentToken)
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

		public void CopyTo(SecurityAttributeCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override SecurityAttribute GetItem(int rid)
		{
			return _module.SecurityAttributeTable.Get(rid, 0);
		}

		protected override SecurityAttribute CreateItem()
		{
			return _module.CreateSecurityAttribute(0, 0);
		}

		private void Load(int parentToken)
		{
			var image = _module.Image;

			int[] rids;
			image.GetSecurityAttributesByParent(MetadataToken.CompressHasDeclSecurity(parentToken), out rids);

			_list.Capacity = rids.Length;
			_list.AddRange(rids);
		}

		#endregion
	}
}
