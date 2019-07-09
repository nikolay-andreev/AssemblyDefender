using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class GenericMethodReference : MethodSignature
	{
		#region Fields

		private MethodReference _declaringMethod;
		private IReadOnlyList<TypeSignature> _genericArguments;

		#endregion

		#region Ctors

		private GenericMethodReference()
		{
		}

		public GenericMethodReference(MethodReference declaringMethod, TypeSignature[] genericArguments)
		{
			if (declaringMethod == null)
				throw new ArgumentNullException("declaringMethod");

			_declaringMethod = declaringMethod;
			_genericArguments = ReadOnlyList<TypeSignature>.Create(genericArguments);
		}

		public GenericMethodReference(MethodReference declaringMethod, IReadOnlyList<TypeSignature> genericArguments)
		{
			if (declaringMethod == null)
				throw new ArgumentNullException("declaringMethod");

			_declaringMethod = declaringMethod;
			_genericArguments = genericArguments ?? ReadOnlyList<TypeSignature>.Empty;
		}

		#endregion

		#region Properties

		public override string Name
		{
			get { return _declaringMethod.Name; }
		}

		public override bool HasThis
		{
			get { return _declaringMethod.HasThis; }
		}

		public override bool ExplicitThis
		{
			get { return _declaringMethod.ExplicitThis; }
		}

		public override int VarArgIndex
		{
			get { return _declaringMethod.VarArgIndex; }
		}

		public override int GenericParameterCount
		{
			get { return _declaringMethod.GenericParameterCount; }
		}

		public override MethodCallingConvention CallConv
		{
			get { return _declaringMethod.CallConv; }
		}

		public override TypeSignature Owner
		{
			get { return _declaringMethod.Owner; }
		}

		public override TypeSignature ReturnType
		{
			get { return _declaringMethod.ReturnType; }
		}

		public override IReadOnlyList<TypeSignature> Arguments
		{
			get { return _declaringMethod.Arguments; }
		}

		public override IReadOnlyList<TypeSignature> GenericArguments
		{
			get { return _genericArguments; }
		}

		public override MethodReference DeclaringMethod
		{
			get { return _declaringMethod; }
		}

		public override CallSite CallSite
		{
			get { return _declaringMethod.CallSite; }
		}

		public override MethodSignatureType Type
		{
			get { return MethodSignatureType.GenericMethod; }
		}

		#endregion

		#region Methods

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _declaringMethod);

			int count = _genericArguments.Count;
			if (count > 0)
			{
				var genericArguments = new TypeSignature[count];
				for (int i = 0; i < count; i++)
				{
					var argument = _genericArguments[i];
					module.AddSignature(ref argument);
					genericArguments[i] = argument;
				}

				_genericArguments = ReadOnlyList<TypeSignature>.Create(genericArguments);
			}
		}

		#endregion

		#region Static

		internal static GenericMethodReference LoadMethodSpec(Module module, int rid)
		{
			var image = module.Image;

			var genericMethodRef = image.MethodSpecSignatures[rid - 1] as GenericMethodReference;
			if (genericMethodRef != null)
				return genericMethodRef;

			MethodSpecRow row;
			image.GetMethodSpec(rid, out row);

			genericMethodRef = new GenericMethodReference();

			genericMethodRef._declaringMethod = MethodReference.LoadMethodDefOrRef(module, MetadataToken.DecompressMethodDefOrRef(row.Method));

			using (var accessor = image.OpenBlob(row.Instantiation))
			{
				byte sigType = accessor.ReadByte(); // Should be equal to SignatureType.GenericInst
				if (sigType != Metadata.SignatureType.GenericInst)
				{
					throw new CodeModelException(string.Format(SR.AssemblyLoadError, module.Location));
				}

				var genericArguments = TypeSignature.LoadGenericArguments(accessor, module);
				genericMethodRef._genericArguments = ReadOnlyList<TypeSignature>.Create(genericArguments);
			}

			module.AddSignature(ref genericMethodRef);
			image.MethodSpecSignatures[rid - 1] = genericMethodRef;

			return genericMethodRef;
		}

		#endregion
	}
}
