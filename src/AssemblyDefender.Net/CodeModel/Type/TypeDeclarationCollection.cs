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
	public class TypeDeclarationCollection : MemberNodeCollection<TypeDeclaration>
	{
		#region Fields

		private int _enclosingTypeRID;
		private TypeDeclaration _enclosingType;

		#endregion

		#region Ctors

		internal TypeDeclarationCollection(Module module)
			: base(module)
		{
			if (module.IsNew)
			{
				IsNew = true;
			}
			else
			{
				Load();
			}
		}

		internal TypeDeclarationCollection(TypeDeclaration enclosingType)
			: base(enclosingType)
		{
			_enclosingType = enclosingType;
			_enclosingTypeRID = _enclosingType.RID;

			if (_enclosingType.IsNew)
			{
				IsNew = true;
			}
			else
			{
				LoadNested();
			}
		}

		#endregion

		#region Properties

		public TypeDeclaration EnclosingType
		{
			get { return _enclosingType; }
		}

		#endregion

		#region Methods

		public TypeDeclaration Find(string fullName, bool throwIfMissing = false)
		{
			foreach (var type in this)
			{
				if (type.FullName == fullName)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, fullName));
			}

			return null;
		}

		public TypeDeclaration Find(string name, string ns, bool throwIfMissing = false)
		{
			foreach (var type in this)
			{
				if (type.Name == name && type.Namespace == ns)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, CodeModelUtils.GetTypeName(name, ns)));
			}

			return null;
		}

		public TypeDeclaration FindBackward(string fullName, bool throwIfMissing = false)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				var type = this[i];
				if (type.FullName == fullName)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, fullName));
			}

			return null;
		}

		public TypeDeclaration FindBackward(string name, string ns, bool throwIfMissing = false)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				var type = this[i];
				if (type.Name == name && type.Namespace == ns)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, CodeModelUtils.GetTypeName(name, ns)));
			}

			return null;
		}

		public TypeDeclaration FindGlobal(bool throwIfMissing = false)
		{
			if (_enclosingType != null)
			{
				throw new InvalidOperationException();
			}

			foreach (var type in this)
			{
				if (type.Name == CodeModelUtils.GlobalTypeName && type.Namespace == null)
					return type;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.TypeNotFound, CodeModelUtils.GlobalTypeName));
			}

			return null;
		}

		public TypeDeclaration GetOrCreateGlobal()
		{
			var type = FindGlobal();
			if (type == null)
			{
				type = Insert(0);
				type.Name = CodeModelUtils.GlobalTypeName;
			}

			return type;
		}

		public void CopyTo(TypeDeclarationCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override TypeDeclaration GetItem(int rid)
		{
			return _module.TypeTable.Get(rid, _enclosingTypeRID);
		}

		protected override TypeDeclaration CreateItem()
		{
			return _module.CreateType(0, _enclosingTypeRID);
		}

		private void Load()
		{
			var image = _module.Image;

			int count = image.GetTypeDefCount();
			for (int typeRID = 1; typeRID <= count; typeRID++)
			{
				if (image.IsTypeNested(typeRID))
					continue;

				_list.Add(typeRID);
			}

			_list.Capacity = _list.Count;
		}

		private void LoadNested()
		{
			var image = _module.Image;

			int[] rids;
			image.GetNestedTypesByEnclosing(_enclosingType.RID, out rids);

			_list.Capacity = rids.Length;
			_list.AddRange(rids);
		}

		#endregion
	}
}
