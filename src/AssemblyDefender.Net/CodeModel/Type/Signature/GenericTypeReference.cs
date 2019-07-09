using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class GenericTypeReference : TypeSignature
	{
		#region Fields

		private TypeReference _declaringType;
		private IReadOnlyList<TypeSignature> _genericArguments;

		#endregion

		#region Ctors

		private GenericTypeReference()
		{
		}

		public GenericTypeReference(TypeReference declaringType, TypeSignature[] genericArguments)
		{
			if (declaringType == null)
				throw new ArgumentNullException("declaringType");

			_declaringType = declaringType;
			_genericArguments = ReadOnlyList<TypeSignature>.Create(genericArguments);
		}

		public GenericTypeReference(TypeReference declaringType, IReadOnlyList<TypeSignature> genericArguments)
		{
			if (declaringType == null)
				throw new ArgumentNullException("declaringType");

			_declaringType = declaringType;
			_genericArguments = genericArguments ?? ReadOnlyList<TypeSignature>.Empty;
		}

		#endregion

		#region Properties

		public override string Name
		{
			get
			{
				string name = _declaringType.Name;
				if (string.IsNullOrEmpty(name))
					return null;

				var builder = new StringBuilder();
				builder.Append(name);

				builder.Append("<");

				for (int i = 0; i < _genericArguments.Count; i++)
				{
					builder.Append(_genericArguments[i].ToString());
				}

				builder.Append(">");

				return builder.ToString();
			}
		}

		public override string Namespace
		{
			get { return _declaringType.Namespace; }
		}

		public override TypeReference EnclosingType
		{
			get { return _declaringType.EnclosingType; }
		}

		public override TypeReference DeclaringType
		{
			get { return _declaringType; }
		}

		public override Signature ResolutionScope
		{
			get { return _declaringType.ResolutionScope; }
		}

		public override Signature Owner
		{
			get { return _declaringType.Owner; }
		}

		public override IReadOnlyList<TypeSignature> GenericArguments
		{
			get { return _genericArguments; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.GenericType; }
		}

		#endregion

		#region Methods

		public override bool GetSize(Module module, out int size)
		{
			return _declaringType.GetSize(module, out size);
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _declaringType);

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

		internal static GenericTypeReference LoadGeneric(IBinaryAccessor accessor, Module module)
		{
			var typeRef = Load(accessor, module) as TypeReference;
			if (typeRef == null)
			{
				throw new CodeModelException(string.Format(SR.AssemblyLoadError, module.Location));
			}

			var genericArguments = LoadGenericArguments(accessor, module);

			return new GenericTypeReference(typeRef, genericArguments);
		}

		#endregion
	}
}
