using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class FieldReference : Signature, IFieldSignature
	{
		#region Fields

		private string _name;
		private TypeSignature _fieldType;
		private TypeSignature _owner;

		#endregion

		#region Ctors

		private FieldReference()
		{
		}

		public FieldReference(string name, TypeSignature fieldType, TypeSignature owner)
		{
			if (fieldType == null)
				throw new ArgumentNullException("fieldType");

			if (owner == null)
				throw new ArgumentNullException("owner");

			_name = name.NullIfEmpty();
			_fieldType = fieldType;
			_owner = owner;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public TypeSignature FieldType
		{
			get { return _fieldType; }
		}

		public TypeSignature Owner
		{
			get { return _owner; }
		}

		public override SignatureType SignatureType
		{
			get { return SignatureType.Field; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return this.ToString(null, SignaturePrintingFlags.None);
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _fieldType);
			module.AddSignature(ref _owner);
		}

		#endregion

		#region IFieldSignature

		ITypeSignature IFieldSignature.FieldType
		{
			get { return _fieldType; }
		}

		ITypeSignature IFieldSignature.Owner
		{
			get { return _owner; }
		}

		#endregion

		#region Static

		internal static FieldReference Load(Module module, int token)
		{
			switch (MetadataToken.GetType(token))
			{
				case MetadataTokenType.Field:
					return LoadFieldDef(module, MetadataToken.GetRID(token));

				case MetadataTokenType.MemberRef:
					return (FieldReference)MethodReference.LoadMemberRef(module, MetadataToken.GetRID(token));

				default:
					throw new Exception(string.Format("Invalid token {0}. Expected FieldRef.", MetadataToken.GetType(token)));
			}
		}

		internal static FieldReference LoadFieldDef(Module module, int rid)
		{
			var image = module.Image;

			var fieldRef = image.FieldSignatures[rid - 1] as FieldReference;
			if (fieldRef != null)
				return fieldRef;

			FieldRow row;
			image.GetField(rid, out row);

			fieldRef = new FieldReference();

			fieldRef._name = image.GetString(row.Name);

			int typeRID = image.GetTypeByField(rid);

			fieldRef._owner = TypeReference.LoadTypeDef(module, typeRID);

			using (var accessor = image.OpenBlob(row.Signature))
			{
				byte sigType = accessor.ReadByte();
				if (sigType != Metadata.SignatureType.Field)
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, module.Location));
				}

				fieldRef._fieldType = TypeSignature.Load(accessor, module);
			}

			module.AddSignature(ref fieldRef);
			image.FieldSignatures[rid - 1] = fieldRef;

			return fieldRef;
		}

		#endregion
	}
}
