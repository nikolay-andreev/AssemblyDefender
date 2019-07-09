using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	internal static class ProjectUtils
	{
		internal static TypeNodeKind GetTypeKind(TypeDeclaration type)
		{
			if (type.IsInterface)
				return TypeNodeKind.Interface;

			var baseType = type.BaseType;
			if (baseType != null)
			{
				var module = type.Module;

				if (baseType.Equals(module, "ValueType", "System", "mscorlib"))
					return TypeNodeKind.ValueType;

				if (baseType.Equals(module, "Enum", "System", "mscorlib"))
					return TypeNodeKind.Enum;

				if (baseType.Equals(module, "MulticastDelegate", "System", "mscorlib"))
					return TypeNodeKind.Delegate;

				if (baseType.Equals(module, "Delegate", "System", "mscorlib"))
					return TypeNodeKind.Delegate;
			}

			return TypeNodeKind.Class;
		}

		internal static FieldNodeKind GetFieldKind(FieldDeclaration field, TypeDeclaration type, TypeNodeKind typeKind)
		{
			switch (typeKind)
			{
				case TypeNodeKind.Enum:
					{
						if (SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName.Equals(field.FieldType, type))
						{
							return FieldNodeKind.EnumItem;
						}
						else
						{
							return FieldNodeKind.Field;
						}
					}

				default:
					{
						return FieldNodeKind.Field;
					}
			}
		}

		internal static string GetAssemblyOutputFilePath(ProjectAssembly projectAssembly)
		{
			string filePath = projectAssembly.FilePath;

			string fileName;
			if (!string.IsNullOrEmpty(projectAssembly.Name))
			{
				fileName = projectAssembly.Name + Path.GetExtension(filePath);
			}
			else
			{
				fileName = Path.GetFileName(filePath);
			}

			if (!string.IsNullOrEmpty(projectAssembly.OutputPath))
			{
				return Path.Combine(projectAssembly.OutputPath, fileName);
			}
			else
			{
				return Path.Combine(Path.GetDirectoryName(filePath), fileName);
			}
		}

		#region Image

		internal static ImageType GetTypeImage(TypeDeclaration type)
		{
			return GetTypeImage(type, GetTypeKind(type));
		}

		internal static ImageType GetTypeImage(TypeDeclaration type, TypeNodeKind typeKind)
		{
			switch (typeKind)
			{
				case TypeNodeKind.Class:
					switch (type.Visibility)
					{
						case TypeVisibilityFlags.Private:
						case TypeVisibilityFlags.NestedAssembly:
							return ImageType.Node_Class_Sealed;

						case TypeVisibilityFlags.NestedPrivate:
							return ImageType.Node_Class_Private;

						case TypeVisibilityFlags.NestedFamily:
						case TypeVisibilityFlags.NestedFamOrAssem:
						case TypeVisibilityFlags.NestedFamAndAssem:
							return ImageType.Node_Class_Protected;

						default:
							return ImageType.Node_Class;
					}

				case TypeNodeKind.Delegate:
					switch (type.Visibility)
					{
						case TypeVisibilityFlags.Private:
						case TypeVisibilityFlags.NestedAssembly:
							return ImageType.Node_Delegate_Sealed;

						case TypeVisibilityFlags.NestedPrivate:
							return ImageType.Node_Delegate_Private;

						case TypeVisibilityFlags.NestedFamily:
						case TypeVisibilityFlags.NestedFamOrAssem:
						case TypeVisibilityFlags.NestedFamAndAssem:
							return ImageType.Node_Delegate_Protected;

						default:
							return ImageType.Node_Delegate;
					}

				case TypeNodeKind.Enum:
					switch (type.Visibility)
					{
						case TypeVisibilityFlags.Private:
						case TypeVisibilityFlags.NestedAssembly:
							return ImageType.Node_Enum_Sealed;

						case TypeVisibilityFlags.NestedPrivate:
							return ImageType.Node_Enum_Private;

						case TypeVisibilityFlags.NestedFamily:
						case TypeVisibilityFlags.NestedFamOrAssem:
						case TypeVisibilityFlags.NestedFamAndAssem:
							return ImageType.Node_Enum_Protected;

						default:
							return ImageType.Node_Enum;
					}

				case TypeNodeKind.Interface:
					switch (type.Visibility)
					{
						case TypeVisibilityFlags.Private:
						case TypeVisibilityFlags.NestedAssembly:
							return ImageType.Node_Interface_Sealed;

						case TypeVisibilityFlags.NestedPrivate:
							return ImageType.Node_Interface_Private;

						case TypeVisibilityFlags.NestedFamily:
						case TypeVisibilityFlags.NestedFamOrAssem:
						case TypeVisibilityFlags.NestedFamAndAssem:
							return ImageType.Node_Interface_Protected;

						default:
							return ImageType.Node_Interface;
					}

				case TypeNodeKind.ValueType:
					switch (type.Visibility)
					{
						case TypeVisibilityFlags.Private:
						case TypeVisibilityFlags.NestedAssembly:
							return ImageType.Node_ValueType_Sealed;

						case TypeVisibilityFlags.NestedPrivate:
							return ImageType.Node_ValueType_Private;

						case TypeVisibilityFlags.NestedFamily:
						case TypeVisibilityFlags.NestedFamOrAssem:
						case TypeVisibilityFlags.NestedFamAndAssem:
							return ImageType.Node_ValueType_Protected;

						default:
							return ImageType.Node_ValueType;
					}

				default:
					throw new NotImplementedException();
			}
		}

		internal static ImageType GetMethodImage(MethodDeclaration method)
		{
			switch (method.Visibility)
			{
				case MethodVisibilityFlags.Private:
				case MethodVisibilityFlags.PrivateScope:
					return ImageType.Node_Method_Private;

				case MethodVisibilityFlags.Assembly:
					return ImageType.Node_Method_Sealed;

				case MethodVisibilityFlags.Family:
				case MethodVisibilityFlags.FamOrAssem:
				case MethodVisibilityFlags.FamAndAssem:
					return ImageType.Node_Method_Protected;

				default:
					return ImageType.Node_Method;
			}
		}

		internal static ImageType GetFieldImage(FieldDeclaration field, FieldNodeKind fieldKind)
		{
			switch (fieldKind)
			{
				case FieldNodeKind.EnumItem:
					switch (field.Visibility)
					{
						case FieldVisibilityFlags.Private:
						case FieldVisibilityFlags.PrivateScope:
							return ImageType.Node_EnumItem_Private;

						case FieldVisibilityFlags.Assembly:
							return ImageType.Node_EnumItem_Sealed;

						case FieldVisibilityFlags.Family:
						case FieldVisibilityFlags.FamOrAssem:
						case FieldVisibilityFlags.FamAndAssem:
							return ImageType.Node_EnumItem_Protected;

						default:
							return ImageType.Node_EnumItem;
					}

				case FieldNodeKind.Field:
					switch (field.Visibility)
					{
						case FieldVisibilityFlags.Private:
						case FieldVisibilityFlags.PrivateScope:
							return ImageType.Node_Field_Private;

						case FieldVisibilityFlags.Assembly:
							return ImageType.Node_Field_Sealed;

						case FieldVisibilityFlags.Family:
						case FieldVisibilityFlags.FamOrAssem:
						case FieldVisibilityFlags.FamAndAssem:
							return ImageType.Node_Field_Protected;

						default:
							return ImageType.Node_Field;
					}

				default:
					throw new NotImplementedException();
			}
		}

		internal static ImageType GetPropertyImage(MethodDeclaration method)
		{
			switch (method.Visibility)
			{
				case MethodVisibilityFlags.Private:
				case MethodVisibilityFlags.PrivateScope:
					return ImageType.Node_Properties_Private;

				case MethodVisibilityFlags.Assembly:
					return ImageType.Node_Properties_Sealed;

				case MethodVisibilityFlags.Family:
				case MethodVisibilityFlags.FamOrAssem:
				case MethodVisibilityFlags.FamAndAssem:
					return ImageType.Node_Properties_Protected;

				default:
					return ImageType.Node_Properties;
			}
		}

		internal static ImageType GetEventImage(MethodDeclaration method)
		{
			switch (method.Visibility)
			{
				case MethodVisibilityFlags.Private:
				case MethodVisibilityFlags.PrivateScope:
					return ImageType.Node_Event_Private;

				case MethodVisibilityFlags.Assembly:
					return ImageType.Node_Event_Sealed;

				case MethodVisibilityFlags.Family:
				case MethodVisibilityFlags.FamOrAssem:
				case MethodVisibilityFlags.FamAndAssem:
					return ImageType.Node_Event_Protected;

				default:
					return ImageType.Node_Event;
			}
		}

		#endregion
	}
}
