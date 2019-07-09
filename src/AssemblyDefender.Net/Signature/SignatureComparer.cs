using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender.Net
{
	public class SignatureComparer : IEqualityComparer<ISignature>
	{
		#region Fields

		public static readonly SignatureComparer Default = new SignatureComparer(SignatureComparisonFlags.None);
		public static readonly SignatureComparer IgnoreAssemblyStrongName = new SignatureComparer(SignatureComparisonFlags.IgnoreAssemblyStrongName);
		public static readonly SignatureComparer IgnoreTypeOwner = new SignatureComparer(SignatureComparisonFlags.IgnoreTypeOwner);
		public static readonly SignatureComparer IgnoreTypeOwner_IgnoreAssemblyStrongName = new SignatureComparer(SignatureComparisonFlags.IgnoreTypeOwner | SignatureComparisonFlags.IgnoreAssemblyStrongName);
		public static readonly SignatureComparer IgnoreMemberOwner = new SignatureComparer(SignatureComparisonFlags.IgnoreMemberOwner);
		public static readonly SignatureComparer IgnoreMemberOwner_IgnoreAssemblyStrongName = new SignatureComparer(SignatureComparisonFlags.IgnoreMemberOwner | SignatureComparisonFlags.IgnoreAssemblyStrongName);
		protected SignatureComparisonFlags _flags;

		#endregion

		#region Ctors

		public SignatureComparer()
		{
		}

		public SignatureComparer(SignatureComparisonFlags flags)
		{
			_flags = flags;
		}

		#endregion

		#region Properties

		public SignatureComparisonFlags Flags
		{
			get { return _flags; }
		}

		#endregion

		#region Methods

		public bool Equals(ISignature x, ISignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			switch (x.SignatureType)
			{
				case SignatureType.Assembly:
					return Equals((IAssemblySignature)x, y as IAssemblySignature);

				case SignatureType.Module:
					return Equals((IModuleSignature)x, y as IModuleSignature);

				case SignatureType.File:
					return Equals((IFileSignature)x, y as IFileSignature);

				case SignatureType.Type:
					return Equals((ITypeSignature)x, y as ITypeSignature);

				case SignatureType.Method:
					return Equals((IMethodSignature)x, y as IMethodSignature);

				case SignatureType.Field:
					return Equals((IFieldSignature)x, y as IFieldSignature);

				case SignatureType.Property:
					return Equals((IPropertySignature)x, y as IPropertySignature);

				case SignatureType.Event:
					return Equals((IEventSignature)x, y as IEventSignature);

				default:
					throw new NotImplementedException();
			}
		}

		public int GetHashCode(ISignature signature)
		{
			switch (signature.SignatureType)
			{
				case SignatureType.Assembly:
					return GetHashCode((IAssemblySignature)signature);

				case SignatureType.Module:
					return GetHashCode((IModuleSignature)signature);

				case SignatureType.File:
					return GetHashCode((IFileSignature)signature);

				case SignatureType.Type:
					return GetHashCode((ITypeSignature)signature);

				case SignatureType.Method:
					return GetHashCode((IMethodSignature)signature);

				case SignatureType.Field:
					return GetHashCode((IFieldSignature)signature);

				case SignatureType.Property:
					return GetHashCode((IPropertySignature)signature);

				case SignatureType.Event:
					return GetHashCode((IEventSignature)signature);

				default:
					throw new NotImplementedException();
			}
		}

		#endregion

		#region Assembly

		public bool Equals(IAssemblySignature x, IAssemblySignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if ((_flags & SignatureComparisonFlags.IgnoreAssemblyStrongName) != SignatureComparisonFlags.IgnoreAssemblyStrongName)
			{
				if (x.Culture != y.Culture)
					return false;

				if (!CompareUtils.Equals(x.Version, y.Version, true))
					return false;

				if (!CompareUtils.Equals(x.PublicKeyToken, y.PublicKeyToken))
					return false;
			}

			return true;
		}

		public int GetHashCode(IAssemblySignature obj)
		{
			int hashCode = 0x4000;

			if (obj.Name != null)
				hashCode ^= obj.Name.GetHashCode();

			return hashCode;
		}

		#endregion

		#region Module

		public bool Equals(IModuleSignature x, IModuleSignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			return true;
		}

		public int GetHashCode(IModuleSignature obj)
		{
			int hashCode = 0x5000;

			if (obj.Name != null)
				hashCode ^= obj.Name.GetHashCode();

			return hashCode;
		}

		#endregion

		#region File

		public bool Equals(IFileSignature x, IFileSignature y)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			return true;
		}

		public int GetHashCode(IFileSignature obj)
		{
			int hashCode = 0x17000;

			if (obj.Name != null)
				hashCode ^= obj.Name.GetHashCode();

			return hashCode;
		}

		#endregion

		#region Type

		public bool Equals(ITypeSignature x, ITypeSignature y)
		{
			return Equals(x, y, true);
		}

		protected bool Equals(ITypeSignature x, ITypeSignature y, bool canIgnoreOwner)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.ElementCode != y.ElementCode)
				return false;

			switch (x.ElementCode)
			{
				case TypeElementCode.Array:
					{
						if (!EqualsArrayDimensions(x.ArrayDimensions, y.ArrayDimensions))
							return false;

						if (!Equals(x.ElementType, y.ElementType, canIgnoreOwner))
							return false;

						return true;
					}

				case TypeElementCode.ByRef:
					{
						if (!Equals(x.ElementType, y.ElementType, canIgnoreOwner))
							return false;

						return true;
					}

				case TypeElementCode.CustomModifier:
					{
						CustomModifierType xModifierType;
						var xModifier = x.GetCustomModifier(out xModifierType);

						CustomModifierType yModifierType;
						var yModifier = y.GetCustomModifier(out yModifierType);

						if (xModifierType != yModifierType)
							return false;

						if (!Equals(x.ElementType, y.ElementType, canIgnoreOwner))
							return false;

						if (!Equals(xModifier, yModifier, false))
							return false;

						return true;
					}

				case TypeElementCode.FunctionPointer:
					{
						if (!Equals(x.GetFunctionPointer(), y.GetFunctionPointer(), false))
							return false;

						return true;
					}

				case TypeElementCode.GenericParameter:
					{
						bool xIsMethod;
						int xPosition;
						x.GetGenericParameter(out xIsMethod, out xPosition);

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
						if (!Equals(x.DeclaringType, y.DeclaringType, canIgnoreOwner))
							return false;

						if (!Equals(x.GenericArguments, y.GenericArguments, false))
							return false;

						return true;
					}

				case TypeElementCode.Pinned:
					{
						if (!Equals(x.ElementType, y.ElementType, canIgnoreOwner))
							return false;

						return true;
					}

				case TypeElementCode.Pointer:
					{
						if (!Equals(x.ElementType, y.ElementType, canIgnoreOwner))
							return false;

						return true;
					}

				case TypeElementCode.DeclaringType:
					{
						if (x.Name != y.Name)
							return false;

						if (x.Namespace != y.Namespace)
							return false;

						if (!EqualsTypeOwner(x.Owner, y.Owner, canIgnoreOwner))
							return false;

						return true;
					}

				default:
					throw new NotImplementedException();
			}
		}

		protected bool EqualsTypeOwner(ISignature x, ISignature y, bool canIgnoreOwner)
		{
			if ((x != null && x.SignatureType == SignatureType.Type) || (y != null && y.SignatureType == SignatureType.Type))
			{
				if (!Equals(x as ITypeSignature, y as ITypeSignature, canIgnoreOwner))
					return false;
			}
			else if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreTypeOwner) != SignatureComparisonFlags.IgnoreTypeOwner)
			{
				if (!Equals(x, y))
					return false;
			}

			return true;
		}

		public bool Equals(IReadOnlyList<ITypeSignature> x, IReadOnlyList<ITypeSignature> y)
		{
			return Equals(x, y, true);
		}

		protected bool Equals(IReadOnlyList<ITypeSignature> x, IReadOnlyList<ITypeSignature> y, bool canIgnoreOwner)
		{
			if (x.Count != y.Count)
				return false;

			for (int i = 0; i < x.Count; i++)
			{
				if (!Equals(x[i], y[i], canIgnoreOwner))
					return false;
			}

			return true;
		}

		public int GetHashCode(ITypeSignature obj)
		{
			return GetHashCode(obj, true);
		}

		protected int GetHashCode(ITypeSignature obj, bool canIgnoreOwner)
		{
			int hashCode = 0x87478;

			while (obj != null)
			{
				hashCode += 0x7549 << ((int)obj.ElementCode + 1);

				switch (obj.ElementCode)
				{
					case TypeElementCode.FunctionPointer:
						{
							hashCode ^= GetHashCode(obj.GetFunctionPointer(), false);
							obj = null;
						}
						break;

					case TypeElementCode.GenericParameter:
						{
							bool isMethod;
							int position;
							obj.GetGenericParameter(out isMethod, out position);

							if (isMethod)
								hashCode++;

							hashCode += position;
							obj = null;
						}
						break;

					case TypeElementCode.GenericType:
						{
							hashCode ^= obj.GenericArguments.Count << 8;
							obj = obj.DeclaringType;
						}
						break;

					case TypeElementCode.DeclaringType:
						{
							if (obj.Name != null)
								hashCode ^= obj.Name.GetHashCode();

							if (obj.Namespace != null)
								hashCode ^= obj.Namespace.GetHashCode();

							obj = obj.EnclosingType;
						}
						break;

					default:
						{
							obj = obj.ElementType;
						}
						break;
				}
			}

			return hashCode;
		}

		public int GetHashCode(IReadOnlyList<ITypeSignature> obj)
		{
			return GetHashCode(obj, true);
		}

		protected int GetHashCode(IReadOnlyList<ITypeSignature> obj, bool canIgnoreOwner)
		{
			int hashCode = obj.Count << 8;

			for (int i = 0; i < obj.Count; i++)
			{
				hashCode ^= GetHashCode(obj[i], canIgnoreOwner);
			}

			return hashCode;
		}

		#endregion

		#region Method

		public bool Equals(IMethodSignature x, IMethodSignature y)
		{
			return Equals(x, y, true);
		}

		protected bool Equals(IMethodSignature x, IMethodSignature y, bool canIgnoreOwner)
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

			if (!Equals(x.Arguments, y.Arguments, false))
				return false;

			if (!Equals(x.GenericArguments, y.GenericArguments, false))
				return false;

			if (!Equals(x.ReturnType, y.ReturnType, false))
				return false;

			if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreMemberOwner) != SignatureComparisonFlags.IgnoreMemberOwner)
			{
				if (!Equals(x.Owner, y.Owner, canIgnoreOwner))
					return false;
			}

			return true;
		}

		public int GetHashCode(IMethodSignature obj)
		{
			return GetHashCode(obj, true);
		}

		protected int GetHashCode(IMethodSignature obj, bool canIgnoreOwner)
		{
			int hashCode = 0x7000;

			if (obj.Name != null)
				hashCode ^= obj.Name.GetHashCode();

			if (obj.HasThis)
				hashCode++;

			if (obj.ExplicitThis)
				hashCode++;

			hashCode ^= (int)obj.CallConv << 8;

			hashCode ^= GetHashCode(obj.ReturnType, false);

			hashCode ^= obj.GenericParameterCount << 8;

			hashCode ^= GetHashCode(obj.Arguments, false);

			if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreMemberOwner) != SignatureComparisonFlags.IgnoreMemberOwner)
			{
				var owner = obj.Owner;
				if (owner != null)
				{
					hashCode ^= GetHashCode(owner, canIgnoreOwner);
				}
			}

			return hashCode;
		}

		#endregion

		#region Field

		public bool Equals(IFieldSignature x, IFieldSignature y)
		{
			return Equals(x, y, true);
		}

		protected bool Equals(IFieldSignature x, IFieldSignature y, bool canIgnoreOwner)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (!Equals(x.FieldType, y.FieldType, false))
				return false;

			if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreMemberOwner) != SignatureComparisonFlags.IgnoreMemberOwner)
			{
				if (!Equals(x.Owner, y.Owner, canIgnoreOwner))
					return false;
			}

			return true;
		}

		public int GetHashCode(IFieldSignature obj)
		{
			return GetHashCode(obj, true);
		}

		protected int GetHashCode(IFieldSignature obj, bool canIgnoreOwner)
		{
			int hashCode = 0x8000;

			if (obj.Name != null)
				hashCode ^= obj.Name.GetHashCode();

			if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreMemberOwner) != SignatureComparisonFlags.IgnoreMemberOwner)
			{
				var owner = obj.Owner;
				if (owner != null)
				{
					hashCode ^= GetHashCode(owner, canIgnoreOwner);
				}
			}

			return hashCode;
		}

		#endregion

		#region Property

		public bool Equals(IPropertySignature x, IPropertySignature y)
		{
			return Equals(x, y, true);
		}

		protected bool Equals(IPropertySignature x, IPropertySignature y, bool canIgnoreOwner)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (!Equals(x.Arguments, y.Arguments, false))
				return false;

			if (!Equals(x.ReturnType, y.ReturnType, false))
				return false;

			if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreMemberOwner) != SignatureComparisonFlags.IgnoreMemberOwner)
			{
				if (!Equals(x.Owner, y.Owner, canIgnoreOwner))
					return false;
			}

			return true;
		}

		public int GetHashCode(IPropertySignature obj)
		{
			return GetHashCode(obj, true);
		}

		protected int GetHashCode(IPropertySignature obj, bool canIgnoreOwner)
		{
			int hashCode = 0x9000;

			if (obj.Name != null)
				hashCode ^= obj.Name.GetHashCode();

			hashCode += obj.Arguments.Count;

			if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreMemberOwner) != SignatureComparisonFlags.IgnoreMemberOwner)
			{
				var owner = obj.Owner;
				if (owner != null)
				{
					hashCode ^= GetHashCode(owner, canIgnoreOwner);
				}
			}

			return hashCode;
		}

		#endregion

		#region Event

		public bool Equals(IEventSignature x, IEventSignature y)
		{
			return Equals(x, y, true);
		}

		protected bool Equals(IEventSignature x, IEventSignature y, bool canIgnoreOwner)
		{
			if (x == y)
				return true;

			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if (!Equals(x.EventType, y.EventType, false))
				return false;

			if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreMemberOwner) != SignatureComparisonFlags.IgnoreMemberOwner)
			{
				if (!Equals(x.Owner, y.Owner, canIgnoreOwner))
					return false;
			}

			return true;
		}

		public int GetHashCode(IEventSignature obj)
		{
			return GetHashCode(obj, true);
		}

		protected int GetHashCode(IEventSignature obj, bool canIgnoreOwner)
		{
			int hashCode = 0x10000;

			if (obj.Name != null)
				hashCode ^= obj.Name.GetHashCode();

			if (!canIgnoreOwner || (_flags & SignatureComparisonFlags.IgnoreMemberOwner) != SignatureComparisonFlags.IgnoreMemberOwner)
			{
				var owner = obj.Owner;
				if (owner != null)
				{
					hashCode ^= GetHashCode(owner, canIgnoreOwner);
				}
			}

			return hashCode;
		}

		#endregion

		#region Static

		public static bool Equals(IAssembly x, System.Reflection.AssemblyName y, SignatureComparisonFlags flags = SignatureComparisonFlags.None)
		{
			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			if ((flags & SignatureComparisonFlags.IgnoreAssemblyStrongName) != SignatureComparisonFlags.IgnoreAssemblyStrongName)
			{
				string culture = (y.CultureInfo != null) ? y.CultureInfo.Name : null;
				if (x.Culture != culture)
					return false;

				if (!Equals(x.Version, y.Version))
					return false;

				if (!Equals(x.PublicKeyToken, y.GetPublicKeyToken()))
					return false;
			}

			return true;
		}

		public static bool Equals(IModule x, System.Reflection.Module y)
		{
			if (x == null || y == null)
				return false;

			if (x.Name != y.Name)
				return false;

			return true;
		}

		public static bool EqualsArrayDimensions(IReadOnlyList<ArrayDimension> x, IReadOnlyList<ArrayDimension> y)
		{
			if (x.Count != y.Count)
				return false;

			for (int i = 0; i < x.Count; i++)
			{
				if (!x[i].Equals(y[i]))
					return false;
			}

			return true;
		}

		#endregion
	}
}
