using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class PropertyReference : Signature, IPropertySignature
	{
		#region Fields

		private string _name;
		private TypeSignature _owner;
		private TypeSignature _returnType;
		private IReadOnlyList<TypeSignature> _arguments;

		#endregion

		#region Ctors

		public PropertyReference(
			string name,
			TypeSignature owner,
			TypeSignature returnType,
			TypeSignature[] arguments)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			if (returnType == null)
				throw new ArgumentNullException("returnType");

			_name = name.NullIfEmpty();
			_owner = owner;
			_returnType = returnType;
			_arguments = ReadOnlyList<TypeSignature>.Create(arguments);
		}

		public PropertyReference(
			string name,
			TypeSignature owner,
			TypeSignature returnType,
			IReadOnlyList<TypeSignature> arguments)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			if (returnType == null)
				throw new ArgumentNullException("returnType");

			_name = name.NullIfEmpty();
			_owner = owner;
			_returnType = returnType;
			_arguments = arguments ?? ReadOnlyList<TypeSignature>.Empty;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public TypeSignature Owner
		{
			get { return _owner; }
		}

		public TypeSignature ReturnType
		{
			get { return _returnType; }
		}

		public IReadOnlyList<TypeSignature> Arguments
		{
			get { return _arguments; }
		}

		public override SignatureType SignatureType
		{
			get { return SignatureType.Property; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return this.ToString(null, SignaturePrintingFlags.None);
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _returnType);
			module.AddSignature(ref _owner);

			int count = _arguments.Count;
			if (count > 0)
			{
				var arguments = new TypeSignature[count];
				for (int i = 0; i < count; i++)
				{
					var argument = _arguments[i];
					module.AddSignature(ref argument);
					arguments[i] = argument;
				}

				_arguments = ReadOnlyList<TypeSignature>.Create(arguments);
			}
		}

		#endregion

		#region IPropertySignature

		ITypeSignature IPropertySignature.ReturnType
		{
			get { return _returnType; }
		}

		ITypeSignature IPropertySignature.Owner
		{
			get { return _owner; }
		}

		IReadOnlyList<ITypeSignature> IPropertySignature.Arguments
		{
			get { return _arguments; }
		}

		#endregion
	}
}
