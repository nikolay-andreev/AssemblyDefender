using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Reflection style printer.
	/// </summary>
	internal class ReflectionSignaturePrinter
	{
		private SignaturePrintingFlags _flags;
		private StringBuilder _builder = new StringBuilder(0x60);

		internal ReflectionSignaturePrinter(SignaturePrintingFlags flags)
		{
			_flags = flags;
		}

		public override string ToString()
		{
			return _builder.ToString();
		}

		internal void PrintAssembly(IAssemblySignature assemblySig)
		{
			SignaturePrinter.PrintAssembly(_builder, assemblySig, _flags);
		}

		internal void PrintType(ITypeSignature typeSig, IModule module)
		{
			PrintType(
				typeSig,
				module,
				(_flags & SignaturePrintingFlags.IgnoreTypeOwner) == SignaturePrintingFlags.IgnoreTypeOwner);
		}

		private void PrintType(ITypeSignature typeSig, IModule module, bool ignoreOwner)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					{
						PrintType(typeSig.ElementType, module, true);

						_builder.Append("[");

						int count = typeSig.ArrayDimensions.Count - 1;
						for (int i = 0; i < count; i++)
						{
							_builder.Append(",");
						}

						_builder.Append("]");
					}
					break;

				case TypeElementCode.ByRef:
					{
						PrintType(typeSig.ElementType, module, true);
						_builder.Append("&");
					}
					break;

				case TypeElementCode.CustomModifier:
					{
						PrintType(typeSig.ElementType, module, true);
					}
					break;

				case TypeElementCode.FunctionPointer:
					break;

				case TypeElementCode.GenericParameter:
					{
						bool isMethod;
						int position;
						typeSig.GetGenericParameter(out isMethod, out position);

						if (isMethod)
							_builder.Append("!!");
						else
							_builder.Append("!");

						_builder.Append(position);
					}
					break;

				case TypeElementCode.GenericType:
					{
						PrintDeclaringType(typeSig.DeclaringType, module);
						PrintGenericArguments(typeSig.GenericArguments, module);
					}
					break;

				case TypeElementCode.Pinned:
					{
						PrintType(typeSig.ElementType, module, true);
					}
					break;

				case TypeElementCode.Pointer:
					{
						PrintType(typeSig.ElementType, module, true);
						_builder.Append("*");
					}
					break;

				case TypeElementCode.DeclaringType:
					{
						PrintDeclaringType(typeSig, module);
					}
					break;

				default:
					throw new InvalidOperationException();
			}

			if (!ignoreOwner)
			{
				typeSig = typeSig.GetDeclaringType();
				if (typeSig != null)
				{
					var owner = typeSig.ResolutionScope;
					if (owner != null)
					{
						_builder.Append(", ");
						PrintAssembly((IAssemblySignature)owner);
					}
					else if (module != null && module.IsPrimeModule)
					{
						_builder.Append(", ");
						PrintAssembly(module.Assembly);
					}
				}
			}
		}

		private void PrintDeclaringType(ITypeSignature typeSig, IModule module)
		{
			var owner = typeSig.Owner;
			if (owner != null)
			{
				if (owner.SignatureType == SignatureType.Type)
				{
					PrintDeclaringType((ITypeSignature)owner, module);
					_builder.Append("+");
				}
			}

			if (typeSig.Namespace != null)
			{
				_builder.Append(typeSig.Namespace);
				_builder.Append(".");
			}

			_builder.Append(typeSig.Name);
		}

		private void PrintGenericArguments(IReadOnlyList<ITypeSignature> genericArguments, IModule module)
		{
			if (genericArguments.Count == 0)
				return;

			_builder.Append("[");

			for (int i = 0; i < genericArguments.Count; i++)
			{
				if (i > 0)
					_builder.Append(",");

				var genericArgument = genericArguments[i];

				bool isLocalType = IsLocalType(genericArgument);
				bool ignoreOwner = isLocalType && (_flags & SignaturePrintingFlags.IgnoreTypeOwner) == SignaturePrintingFlags.IgnoreTypeOwner;
				if (!ignoreOwner && isLocalType && module == null)
					ignoreOwner = true;

				if (ignoreOwner)
				{
					ignoreOwner = IsLocalType(genericArgument);
				}

				if (!ignoreOwner)
					_builder.Append("[");

				PrintType(genericArgument, module, ignoreOwner);

				if (!ignoreOwner)
					_builder.Append("]");
			}

			_builder.Append("]");
		}

		private bool IsLocalType(ITypeSignature typeSig)
		{
			typeSig = typeSig.GetDeclaringType();
			if (typeSig == null)
				return true;

			return null == typeSig.ResolutionScope;
		}
	}
}
