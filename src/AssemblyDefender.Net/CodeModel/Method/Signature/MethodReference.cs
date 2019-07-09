using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class MethodReference : MethodSignature
	{
		#region Fields

		private string _name;
		private TypeSignature _owner;
		private CallSite _callSite;

		#endregion

		#region Ctors

		private MethodReference()
		{
		}

		public MethodReference(string name, TypeSignature owner, CallSite callSite)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			if (callSite == null)
				throw new ArgumentNullException("callSite");

			_name = name.NullIfEmpty();
			_owner = owner;
			_callSite = callSite;
		}

		#endregion

		#region Properties

		public override string Name
		{
			get { return _name; }
		}

		public override bool HasThis
		{
			get { return _callSite.HasThis; }
		}

		public override bool ExplicitThis
		{
			get { return _callSite.ExplicitThis; }
		}

		public override int VarArgIndex
		{
			get { return _callSite.VarArgIndex; }
		}

		public override int GenericParameterCount
		{
			get { return _callSite.GenericParameterCount; }
		}

		public override MethodCallingConvention CallConv
		{
			get { return _callSite.CallConv; }
		}

		public override TypeSignature Owner
		{
			get { return _owner; }
		}

		public override TypeSignature ReturnType
		{
			get { return _callSite.ReturnType; }
		}

		public override IReadOnlyList<TypeSignature> Arguments
		{
			get { return _callSite.Arguments; }
		}

		public override MethodReference DeclaringMethod
		{
			get { return this; }
		}

		public override CallSite CallSite
		{
			get { return _callSite; }
		}

		public override MethodSignatureType Type
		{
			get { return MethodSignatureType.DeclaringMethod; }
		}

		#endregion

		#region Methods

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _owner);
			module.AddSignature(ref _callSite);
		}

		#endregion

		#region Static

		internal static MethodReference LoadMethodDefOrRef(Module module, int token)
		{
			switch (MetadataToken.GetType(token))
			{
				case MetadataTokenType.Method:
					return LoadMethodDef(module, MetadataToken.GetRID(token));

				case MetadataTokenType.MemberRef:
					return (MethodReference)LoadMemberRef(module, MetadataToken.GetRID(token));

				default:
					throw new Exception(string.Format("Invalid method reference token {0}", token.ToString()));
			}
		}

		internal static MethodReference LoadMethodDef(Module module, int rid)
		{
			var image = module.Image;

			var methodRef = image.MethodSignatures[rid - 1] as MethodReference;
			if (methodRef != null)
				return methodRef;

			MethodRow row;
			image.GetMethod(rid, out row);

			methodRef = new MethodReference();

			methodRef._name = image.GetString(row.Name);

			int typeRID = image.GetTypeByMethod(rid);

			methodRef._owner = TypeReference.LoadTypeDef(module, typeRID);

			using (var accessor = image.OpenBlob(row.Signature))
			{
				methodRef._callSite = CallSite.LoadCallSite(accessor, module);
			}

			module.AddSignature(ref methodRef);
			image.MethodSignatures[rid - 1] = methodRef;

			return methodRef;
		}

		internal static Signature LoadMemberRef(Module module, int rid)
		{
			var image = module.Image;

			var memberRef = image.MemberRefSignatures[rid - 1] as Signature;
			if (memberRef != null)
				return memberRef;

			MemberRefRow row;
			image.GetMemberRef(rid, out row);

			string name = image.GetString(row.Name);

			// Owner
			TypeSignature owner;
			int classToken = MetadataToken.DecompressMemberRefParent(row.Class);
			switch (MetadataToken.GetType(classToken))
			{
				case MetadataTokenType.ModuleRef:
					{
						// A ModuleRef token, if the member is defined, in another module of the same image,
						// as a global function or variable.
						var moduleRef = ModuleReference.LoadRef(module, MetadataToken.GetRID(classToken));
						var typeRef = new TypeReference(CodeModelUtils.GlobalTypeName, null, moduleRef);
						module.AddSignature(ref typeRef);
						owner = typeRef;
					}
					break;

				case MetadataTokenType.Method:
					{
						// A MethodDef token, when used to supply a call-site signature for a vararg method that is
						// defined in this module. The Name shall match the Name in the corresponding MethodDef row.
						// The Signature shall match the Signature in the target method definition.
						int typeRID = image.GetTypeByMethod(MetadataToken.GetRID(classToken));
						owner = TypeReference.LoadTypeDef(module, typeRID);
					}
					break;

				default:
					{
						owner = TypeReference.Load(module, classToken);
					}
					break;
			}

			// Signature
			using (var accessor = image.OpenBlob(row.Signature))
			{
				byte sigType = accessor.ReadByte();
				if (sigType == Metadata.SignatureType.Field)
				{
					var fieldType = TypeSignature.Load(accessor, module);
					memberRef = new FieldReference(name, fieldType, owner);
				}
				else
				{
					var callSite = CallSite.LoadCallSite(accessor, module, sigType);
					memberRef = new MethodReference(name, owner, callSite);
				}
			}

			module.AddSignature(ref memberRef);
			image.MemberRefSignatures[rid - 1] = memberRef;

			return memberRef;
		}

		#endregion
	}
}
