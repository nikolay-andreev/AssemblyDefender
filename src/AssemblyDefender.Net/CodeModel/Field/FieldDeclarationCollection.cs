using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class FieldDeclarationCollection : MemberNodeCollection<FieldDeclaration>
	{
		#region Fields

		private TypeDeclaration _type;

		#endregion

		#region Ctors

		internal FieldDeclarationCollection(TypeDeclaration type)
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

		public FieldDeclaration Find(string name, bool throwIfMissing = false)
		{
			foreach (var field in this)
			{
				if (field.Name == name)
					return field;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.FieldNotFound, _type.ToString(), name));
			}

			return null;
		}

		public void CopyTo(FieldDeclarationCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override FieldDeclaration GetItem(int rid)
		{
			return _module.FieldTable.Get(rid, _type.RID);
		}

		protected override FieldDeclaration CreateItem()
		{
			return _module.CreateField(0, _type.RID);
		}

		private void Load()
		{
			var image = _module.Image;

			int[] rids;
			image.GetFieldsByType(_type.RID, out rids);

			_list.Capacity = rids.Length;
			_list.AddRange(rids);
		}

		#endregion
	}
}
