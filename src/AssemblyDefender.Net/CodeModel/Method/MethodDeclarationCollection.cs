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
	public class MethodDeclarationCollection : MemberNodeCollection<MethodDeclaration>
	{
		#region Fields

		private TypeDeclaration _type;

		#endregion

		#region Ctors

		internal MethodDeclarationCollection(TypeDeclaration type)
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

		public MethodDeclaration Find(string name, bool throwIfMissing = false)
		{
			foreach (var method in this)
			{
				if (method.Name == name)
					return method;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.MethodNotFound, _type.ToString(), name));
			}

			return null;
		}

		public MethodDeclaration Find(IMethodSignature methodSig, SignatureComparer comparer, bool throwIfMissing = false)
		{
			foreach (var method in this)
			{
				if (comparer.Equals(method, methodSig))
					return method;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.MethodNotFound2, methodSig.ToString()));
			}

			return null;
		}

		public MethodDeclaration FindStaticConstructor(bool throwIfMissing = false)
		{
			foreach (var method in this)
			{
				if (method.Name == CodeModelUtils.MethodStaticConstructorName)
					return method;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.MethodNotFound, _type.ToString(), CodeModelUtils.MethodStaticConstructorName));
			}

			return null;
		}

		public MethodDeclaration GetOrCreateStaticConstructor()
		{
			var method = FindStaticConstructor();
			if (method == null)
			{
				method = Add();
				method.Name = CodeModelUtils.MethodStaticConstructorName;
				method.Visibility = MethodVisibilityFlags.Private;
				method.IsStatic = true;
				method.IsHideBySig = true;
				method.IsSpecialName = true;
				method.IsRuntimeSpecialName = true;

				var methodBody = new MethodBody();
				methodBody.Instructions.Add(new Instruction(OpCodes.Ret));
				methodBody.Build(method);
			}

			return method;
		}

		public void CopyTo(MethodDeclarationCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override MethodDeclaration GetItem(int rid)
		{
			return _module.MethodTable.Get(rid, _type.RID);
		}

		protected override MethodDeclaration CreateItem()
		{
			return _module.CreateMethod(0, _type.RID);
		}

		private void Load()
		{
			var image = _module.Image;

			int[] rids;
			image.GetMethodsByType(_type.RID, out rids);

			_list.Capacity = rids.Length;
			_list.AddRange(rids);
		}

		#endregion
	}
}
