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
	public class PropertyDeclarationCollection : MemberNodeCollection<PropertyDeclaration>
	{
		#region Fields

		private TypeDeclaration _type;

		#endregion

		#region Ctors

		internal PropertyDeclarationCollection(TypeDeclaration type)
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

		public PropertyDeclaration Find(string name, bool throwIfMissing = false)
		{
			foreach (var property in this)
			{
				if (property.Name == name)
					return property;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.PropertyNotFound, _type.ToString(), name));
			}

			return null;
		}

		public void CopyTo(PropertyDeclarationCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override PropertyDeclaration GetItem(int rid)
		{
			return _module.PropertyTable.Get(rid, _type.RID);
		}

		protected override PropertyDeclaration CreateItem()
		{
			return _module.CreateProperty(0, _type.RID);
		}

		private void Load()
		{
			var image = _module.Image;

			int[] rids;
			image.GetPropertiesByType(_type.RID, out rids);

			_list.Capacity = rids.Length;
			_list.AddRange(rids);
		}

		#endregion
	}
}
