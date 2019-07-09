using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	internal class ResolveSignatureComparer
	{
		#region Fields

		private IModule _xModule;
		private IModule _yModule;

		#endregion

		#region Ctors

		internal ResolveSignatureComparer(IModule xModule, IModule yModule)
		{
			_xModule = xModule;
			_yModule = yModule;
		}

		#endregion

		#region Methods

		internal bool Equals(IMethodSignature x, IMethodSignature y, ICodeNode xGenericContext = null)
		{
			if (x.Name != y.Name)
				return false;

			if (x.HasThis != y.HasThis)
				return false;

			if (x.CallConv != y.CallConv)
				return false;

			if (x.GenericParameterCount != y.GenericParameterCount)
				return false;

			int xArgumentCount = x.GetArgumentCountNoVarArgs();
			int yArgumentCount = y.GetArgumentCountNoVarArgs();
			if (xArgumentCount != yArgumentCount)
				return false;

			if (!EqualsTypes(x.Arguments, y.Arguments, xArgumentCount, xGenericContext))
				return false;

			if (!EqualsType(x.ReturnType, y.ReturnType, xGenericContext))
				return false;

			return true;
		}

		internal bool Equals(IFieldSignature x, IFieldSignature y)
		{
			if (x.Name != y.Name)
				return false;

			if (!EqualsType(x.FieldType, y.FieldType))
				return false;

			return true;
		}

		internal bool Equals(IPropertySignature x, IPropertySignature y)
		{
			if (x.Name != y.Name)
				return false;

			if (!EqualsTypes(x.Arguments, y.Arguments))
				return false;

			if (!EqualsType(x.ReturnType, y.ReturnType))
				return false;

			return true;
		}

		internal bool Equals(IEventSignature x, IEventSignature y)
		{
			if (x.Name != y.Name)
				return false;

			if (!EqualsType(x.EventType, y.EventType))
				return false;

			return true;
		}

		private bool EqualsAssembly(IAssemblySignature x, IAssemblySignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (!CompareUtils.Equals(x.PublicKeyToken, y.PublicKeyToken))
				return false;

			return true;
		}

		private bool EqualsType(ITypeSignature x, ITypeSignature y, ICodeNode xGenericContext = null)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			switch (x.ElementCode)
			{
				case TypeElementCode.Array:
					{
						if (y.ElementCode != TypeElementCode.Array)
							return false;

						if (!SignatureComparer.EqualsArrayDimensions(x.ArrayDimensions, y.ArrayDimensions))
							return false;

						if (!EqualsType(x.ElementType, y.ElementType, xGenericContext))
							return false;

						return true;
					}

				case TypeElementCode.ByRef:
					{
						if (y.ElementCode != TypeElementCode.ByRef)
							return false;

						if (!EqualsType(x.ElementType, y.ElementType, xGenericContext))
							return false;

						return true;
					}

				case TypeElementCode.CustomModifier:
					{
						if (y.ElementCode != TypeElementCode.CustomModifier)
							return false;

						CustomModifierType xModifierType;
						var xModifier = x.GetCustomModifier(out xModifierType);

						CustomModifierType yModifierType;
						var yModifier = y.GetCustomModifier(out yModifierType);

						if (xModifierType != yModifierType)
							return false;

						if (!EqualsType(x.ElementType, y.ElementType, xGenericContext))
							return false;

						if (!EqualsType(xModifier, yModifier))
							return false;

						return true;
					}

				case TypeElementCode.FunctionPointer:
					{
						if (y.ElementCode != TypeElementCode.FunctionPointer)
							return false;

						if (!EqualsMethod(x.GetFunctionPointer(), y.GetFunctionPointer()))
							return false;

						return true;
					}

				case TypeElementCode.GenericParameter:
					{
						bool xIsMethod;
						int xPosition;
						x.GetGenericParameter(out xIsMethod, out xPosition);

						if (xGenericContext != null)
						{
							var xGenericType = xGenericContext.GetGenericArgument(xIsMethod, xPosition);
							if (xGenericType != null)
							{
								return EqualsType(xGenericType, y);
							}
						}

						if (y.ElementCode != TypeElementCode.GenericParameter)
							return false;

						bool yIsMethod;
						int yPosition;
						y.GetGenericParameter(out yIsMethod, out yPosition);

						if (xIsMethod != yIsMethod)
							return false;

						if (xPosition != yPosition)
							return false;

						return true;
					}

				case TypeElementCode.GenericType:
					{
						if (y.ElementCode != TypeElementCode.GenericType)
							return false;

						if (!EqualsType(x.DeclaringType, y.DeclaringType))
							return false;

						if (!EqualsTypes(x.GenericArguments, y.GenericArguments, xGenericContext))
							return false;

						return true;
					}

				case TypeElementCode.Pinned:
					{
						if (y.ElementCode != TypeElementCode.Pinned)
							return false;

						if (!EqualsType(x.ElementType, y.ElementType, xGenericContext))
							return false;

						return true;
					}

				case TypeElementCode.Pointer:
					{
						if (y.ElementCode != TypeElementCode.Pointer)
							return false;

						if (!EqualsType(x.ElementType, y.ElementType, xGenericContext))
							return false;

						return true;
					}

				case TypeElementCode.DeclaringType:
					{
						if (x.Name != y.Name)
							return false;

						if (x.Namespace != y.Namespace)
							return false;

						if (!EqualsTypeOwner(x.Owner, y.Owner))
							return false;

						return true;
					}

				default:
					throw new NotImplementedException();
			}
		}

		private bool EqualsTypes(IReadOnlyList<ITypeSignature> x, IReadOnlyList<ITypeSignature> y, ICodeNode xGenericContext = null)
		{
			if (x.Count != y.Count)
				return false;

			for (int i = 0; i < x.Count; i++)
			{
				if (!EqualsType(x[i], y[i], xGenericContext))
					return false;
			}

			return true;
		}

		private bool EqualsTypes(IReadOnlyList<ITypeSignature> x, IReadOnlyList<ITypeSignature> y, int count, ICodeNode xGenericContext = null)
		{
			for (int i = 0; i < count; i++)
			{
				if (!EqualsType(x[i], y[i], xGenericContext))
					return false;
			}

			return true;
		}

		private bool EqualsTypeOwner(ISignature x, ISignature y)
		{
			if (x != null)
			{
				return EqualsTypeOwner(x, y, _xModule, _yModule);
			}
			else if (y != null)
			{
				return EqualsTypeOwner(y, x, _yModule, _xModule);
			}
			else
			{
				return _xModule.Name == _yModule.Name;
			}
		}

		private bool EqualsTypeOwner(ISignature x, ISignature y, IModule xModule, IModule yModule)
		{
			switch (x.SignatureType)
			{
				case SignatureType.Assembly:
					{
						if (y != null)
						{
							switch (y.SignatureType)
							{
								case SignatureType.Assembly:
									return EqualsAssembly((IAssemblySignature)x, (IAssemblySignature)y);

								case SignatureType.Module:
									return EqualsTypeOwner((IAssemblySignature)x, (IModuleSignature)y, yModule);

								case SignatureType.Type:
									return false;

								default:
									throw new NotImplementedException();
							}
						}
						else
						{
							if (!yModule.IsPrimeModule)
								return false;

							return EqualsAssembly((IAssemblySignature)x, (IAssemblySignature)yModule.Assembly);
						}
					}

				case SignatureType.Module:
					{
						if (y != null)
						{
							switch (y.SignatureType)
							{
								case SignatureType.Assembly:
									return EqualsTypeOwner((IAssemblySignature)y, (IModuleSignature)x, xModule);

								case SignatureType.Module:
									return ((IModuleSignature)x).Name == ((IModuleSignature)y).Name;

								case SignatureType.Type:
									return false;

								default:
									throw new NotImplementedException();
							}
						}
						else
						{
							return ((IModuleSignature)x).Name == yModule.Name;
						}
					}

				case SignatureType.Type:
					return EqualsType(x as ITypeSignature, y as ITypeSignature);

				default:
					throw new NotImplementedException();
			}
		}

		private bool EqualsTypeOwner(IAssemblySignature x, IModuleSignature y, IModule yModule)
		{
			if (!yModule.IsPrimeModule)
				return false;

			if (y.Name != yModule.Name)
				return false;

			if (!EqualsAssembly(x, (IAssemblySignature)yModule.Assembly))
				return false;

			return true;
		}

		private bool EqualsMethod(IMethodSignature x, IMethodSignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (x.HasThis != y.HasThis)
				return false;

			if (x.ExplicitThis != y.ExplicitThis)
				return false;

			if (x.CallConv != y.CallConv)
				return false;

			if (x.GenericParameterCount != y.GenericParameterCount)
				return false;

			if (!EqualsTypes(x.Arguments, y.Arguments))
				return false;

			if (!EqualsTypes(x.GenericArguments, y.GenericArguments))
				return false;

			if (!EqualsType(x.ReturnType, y.ReturnType))
				return false;

			if (!EqualsType(x.Owner, y.Owner))
				return false;

			return true;
		}

		private bool EqualsField(IFieldSignature x, IFieldSignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (!EqualsType(x.FieldType, y.FieldType))
				return false;

			if (!EqualsType(x.Owner, y.Owner))
				return false;

			return true;
		}

		private bool EqualsProperty(IPropertySignature x, IPropertySignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (x.Arguments.Count != y.Arguments.Count)
				return false;

			if (!EqualsType(x.ReturnType, y.ReturnType))
				return false;

			if (!EqualsTypes(x.Arguments, y.Arguments))
				return false;

			if (!EqualsType(x.Owner, y.Owner))
				return false;

			return true;
		}

		private bool EqualsEvent(IEventSignature x, IEventSignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (!EqualsType(x.EventType, y.EventType))
				return false;

			if (!EqualsType(x.Owner, y.Owner))
				return false;

			return true;
		}

		#endregion
	}
}
