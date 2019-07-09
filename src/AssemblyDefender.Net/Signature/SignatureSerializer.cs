using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;
using AssemblyDefender.Common.Serialization;

namespace AssemblyDefender.Net
{
	public class SignatureSerializer : ObjectSerializer<ISignature>
	{
		#region Ctors

		public SignatureSerializer()
			: base(0)
		{
		}

		public SignatureSerializer(int capacity)
			: base(capacity)
		{
		}

		public SignatureSerializer(IBinaryAccessor accessor)
			: base(accessor, SignatureComparer.Default)
		{
		}

		#endregion

		#region Methods

		protected override ISignature Read(int pos)
		{
			switch ((SignatureType)_blob.ReadByte(ref pos))
			{
				case SignatureType.Assembly:
					return ReadAssembly(ref pos);

				case SignatureType.Module:
					return ReadModule(ref pos);

				case SignatureType.File:
					return ReadFile(ref pos);

				case SignatureType.Type:
					return ReadType(ref pos);

				case SignatureType.Method:
					return ReadMethod(ref pos);

				case SignatureType.Field:
					return ReadField(ref pos);

				case SignatureType.Property:
					return ReadProperty(ref pos);

				case SignatureType.Event:
					return ReadEvent(ref pos);

				default:
					throw new NotImplementedException();
			}
		}

		protected override void Write(ISignature signature)
		{
			int pos = _blob.Length;

			switch (signature.SignatureType)
			{
				case SignatureType.Assembly:
					WriteAssembly(ref pos, (IAssemblySignature)signature);
					break;

				case SignatureType.Module:
					WriteModule(ref pos, (IModuleSignature)signature);
					break;

				case SignatureType.File:
					WriteFile(ref pos, (IFileSignature)signature);
					break;

				case SignatureType.Type:
					WriteType(ref pos, (ITypeSignature)signature);
					break;

				case SignatureType.Method:
					WriteMethod(ref pos, (IMethodSignature)signature);
					break;

				case SignatureType.Field:
					WriteField(ref pos, (IFieldSignature)signature);
					break;

				case SignatureType.Property:
					WriteProperty(ref pos, (IPropertySignature)signature);
					break;

				case SignatureType.Event:
					WriteEvent(ref pos, (IEventSignature)signature);
					break;

				default:
					throw new NotImplementedException();
			}
		}

		protected override bool CanKeepAlive(ISignature item)
		{
			return _keepAlive && (item is Signature);
		}

		private T ReadReferenced<T>(ref int pos)
			where T : Signature
		{
			return (T)this[_blob.ReadInt32(ref pos)];
		}

		private T[] ReadReferenced<T>(ref int pos, int count)
			where T : Signature
		{
			var signatures = new T[count];
			for (int i = 0; i < count; i++)
			{
				signatures[i] = (T)this[_blob.ReadInt32(ref pos)];
			}

			return signatures;
		}

		private void WriteReferenced(ref int pos, ISignature signature)
		{
			_blob.Write(ref pos, (int)Add(signature));
		}

		private void WriteReferenced(ref int pos, IReadOnlyList<ISignature> signatures)
		{
			for (int i = 0; i < signatures.Count; i++)
			{
				_blob.Write(ref pos, (int)Add(signatures[i]));
			}
		}

		protected virtual string ReadString(ref int pos)
		{
			return _blob.ReadLengthPrefixedString(ref pos, Encoding.UTF8);
		}

		protected virtual void WriteString(ref int pos, string value)
		{
			_blob.WriteLengthPrefixedString(ref pos, value, Encoding.UTF8);
		}

		#region Assembly

		private AssemblyReference ReadAssembly(ref int pos)
		{
			string name = ReadString(ref pos);
			string culture = ReadString(ref pos);

			Version version = null;
			if (_blob.ReadBoolean(ref pos))
			{
				version = new Version(
					_blob.ReadUInt16(ref pos),
					_blob.ReadUInt16(ref pos),
					_blob.ReadUInt16(ref pos),
					_blob.ReadUInt16(ref pos));
			}

			byte[] publicKeyToken = null;
			int publicKeyTokenCount = _blob.Read7BitEncodedInt(ref pos);
			if (publicKeyTokenCount > 0)
			{
				publicKeyToken = _blob.ReadBytes(ref pos, publicKeyTokenCount);
			}

			return new AssemblyReference(name, culture, version, publicKeyToken);
		}

		private void WriteAssembly(ref int pos, IAssemblySignature assembly)
		{
			_blob.Write(ref pos, (byte)SignatureType.Assembly);
			WriteString(ref pos, assembly.Name);
			WriteString(ref pos, assembly.Culture);

			// Version
			var version = assembly.Version;
			if (version != null)
			{
				_blob.Write(ref pos, true);
				_blob.Write(ref pos, (ushort)version.Major);
				_blob.Write(ref pos, (ushort)version.Minor);
				_blob.Write(ref pos, (ushort)version.Build);
				_blob.Write(ref pos, (ushort)version.Revision);
			}
			else
			{
				_blob.Write(ref pos, false);
			}

			// PublicKeyToken
			var publicKeyToken = assembly.PublicKeyToken;
			if (publicKeyToken != null)
			{
				_blob.Write7BitEncodedInt(ref pos, publicKeyToken.Length);
				_blob.Write(ref pos, publicKeyToken);
			}
			else
			{
				_blob.Write(ref pos, (byte)0);
			}
		}

		#endregion

		#region Module

		private ModuleReference ReadModule(ref int pos)
		{
			string name = ReadString(ref pos);
			return new ModuleReference(name);
		}

		private void WriteModule(ref int pos, IModuleSignature module)
		{
			_blob.Write(ref pos, (byte)SignatureType.Module);
			WriteString(ref pos, module.Name);
		}

		#endregion

		#region File

		private FileReference ReadFile(ref int pos)
		{
			string name = ReadString(ref pos);

			bool containsMetadata = _blob.ReadBoolean(ref pos);

			byte[] hashValue = null;
			int hashValueCount = _blob.Read7BitEncodedInt(ref pos);
			if (hashValueCount > 0)
			{
				hashValue = _blob.ReadBytes(ref pos, hashValueCount);
			}

			return new FileReference(name, containsMetadata, hashValue);
		}

		private void WriteFile(ref int pos, IFileSignature file)
		{
			_blob.Write(ref pos, (byte)SignatureType.File);
			WriteString(ref pos, file.Name);

			_blob.Write(ref pos, (bool)file.ContainsMetadata);

			var hashValue = file.HashValue;
			if (hashValue != null)
			{
				_blob.Write7BitEncodedInt(ref pos, hashValue.Length);
				_blob.Write(ref pos, hashValue);
			}
			else
			{
				_blob.Write(ref pos, (byte)0);
			}
		}

		#endregion

		#region Type

		private TypeSignature ReadType(ref int pos)
		{
			switch ((TypeElementCode)_blob.ReadByte(ref pos))
			{
				case TypeElementCode.Array:
					{
						int arrayDimensionCount = _blob.Read7BitEncodedInt(ref pos);
						var arrayDimensions = new ArrayDimension[arrayDimensionCount];
						for (int i = 0; i < arrayDimensionCount; i++)
						{
							int? lowerBound = ReadArrayDimensionBound(ref pos);
							int? upperBound = ReadArrayDimensionBound(ref pos);
							arrayDimensions[i] = new ArrayDimension(lowerBound, upperBound);
						}

						var elementType = ReadReferenced<TypeSignature>(ref pos);

						return new ArrayType(elementType, arrayDimensions);
					}

				case TypeElementCode.ByRef:
					{
						var elementType = ReadReferenced<TypeSignature>(ref pos);

						return new ByRefType(elementType);
					}

				case TypeElementCode.CustomModifier:
					{
						var modifierType = (CustomModifierType)_blob.ReadByte(ref pos);
						var modifier = ReadReferenced<TypeSignature>(ref pos);
						var elementType = ReadReferenced<TypeSignature>(ref pos);

						return new CustomModifier(elementType, modifier, modifierType);
					}

				case TypeElementCode.FunctionPointer:
					{
						var callSite = ReadReferenced<CallSite>(ref pos);

						return new FunctionPointer(callSite);
					}

				case TypeElementCode.GenericParameter:
					{
						bool isMethod = _blob.ReadBoolean(ref pos);
						int position = _blob.Read7BitEncodedInt(ref pos);

						return new GenericParameterType(isMethod, position);
					}

				case TypeElementCode.GenericType:
					{
						int genericArgumentCount = _blob.Read7BitEncodedInt(ref pos);
						var genericArguments = ReadReferenced<TypeSignature>(ref pos, genericArgumentCount);
						var declaringType = (TypeReference)ReadReferenced<TypeSignature>(ref pos);

						return new GenericTypeReference(declaringType, genericArguments);
					}

				case TypeElementCode.Pinned:
					{
						var elementType = ReadReferenced<TypeSignature>(ref pos);

						return new PinnedType(elementType);
					}

				case TypeElementCode.Pointer:
					{
						var elementType = ReadReferenced<TypeSignature>(ref pos);

						return new PointerType(elementType);
					}

				case TypeElementCode.DeclaringType:
					{
						string name = ReadString(ref pos);
						string ns = ReadString(ref pos);
						bool? isValueType = _blob.ReadNullableBoolean(ref pos);

						Signature owner = null;
						if (_blob.ReadBoolean(ref pos))
						{
							owner = ReadReferenced<Signature>(ref pos);
						}

						return new TypeReference(name, ns, owner, isValueType);
					}

				default:
					throw new NotImplementedException();
			}
		}

		private int? ReadArrayDimensionBound(ref int pos)
		{
			if (_blob.ReadBoolean(ref pos))
				return _blob.Read7BitEncodedInt(ref pos);
			else
				return null;
		}

		private void WriteType(ref int pos, ITypeSignature type)
		{
			_blob.Write(ref pos, (byte)SignatureType.Type);
			_blob.Write(ref pos, (byte)type.ElementCode);

			switch (type.ElementCode)
			{
				case TypeElementCode.Array:
					{
						var arrayDimensions = type.ArrayDimensions;
						_blob.Write7BitEncodedInt(ref pos, arrayDimensions.Count);

						for (int i = 0; i < arrayDimensions.Count; i++)
						{
							var arrayDimension = arrayDimensions[i];
							WriteArrayDimensionBound(ref pos, arrayDimension.LowerBound);
							WriteArrayDimensionBound(ref pos, arrayDimension.UpperBound);
						}

						_blob.Length += 4;
						WriteReferenced(ref pos, type.ElementType);
					}
					break;

				case TypeElementCode.ByRef:
					{
						_blob.Length += 4;
						WriteReferenced(ref pos, type.ElementType);
					}
					break;

				case TypeElementCode.CustomModifier:
					{
						CustomModifierType modifierType;
						ITypeSignature modifier = type.GetCustomModifier(out modifierType);

						_blob.Write(ref pos, (byte)modifierType);

						_blob.Length += 8;

						WriteReferenced(ref pos, modifier);
						WriteReferenced(ref pos, type.ElementType);
					}
					break;

				case TypeElementCode.FunctionPointer:
					{
						_blob.Length += 4;
						WriteReferenced(ref pos, type.GetFunctionPointer());
					}
					break;

				case TypeElementCode.GenericParameter:
					{
						bool isMethod;
						int position;
						type.GetGenericParameter(out isMethod, out position);

						_blob.Write(ref pos, (bool)isMethod);
						_blob.Write7BitEncodedInt(ref pos, position);
					}
					break;

				case TypeElementCode.GenericType:
					{
						var genericArguments = type.GenericArguments;
						_blob.Write7BitEncodedInt(ref pos, genericArguments.Count);

						_blob.Length += (genericArguments.Count + 1) * 4;

						WriteReferenced(ref pos, genericArguments);
						WriteReferenced(ref pos, type.DeclaringType);
					}
					break;

				case TypeElementCode.Pinned:
					{
						_blob.Length += 4;
						WriteReferenced(ref pos, type.ElementType);
					}
					break;

				case TypeElementCode.Pointer:
					{
						_blob.Length += 4;
						WriteReferenced(ref pos, type.ElementType);
					}
					break;

				case TypeElementCode.DeclaringType:
					{
						WriteString(ref pos, type.Name);
						WriteString(ref pos, type.Namespace);
						_blob.Write(ref pos, (bool?)SignatureUtils.TryGetIsValueType(type));

						var owner = type.Owner;
						if (owner != null)
						{
							_blob.Write(ref pos, true);
							_blob.Length += 4;
							WriteReferenced(ref pos, owner);
						}
						else
						{
							_blob.Write(ref pos, false);
						}
					}
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private void WriteArrayDimensionBound(ref int pos, int? bound)
		{
			if (bound.HasValue)
			{
				_blob.Write(ref pos, true);
				_blob.Write7BitEncodedInt(ref pos, bound.Value);
			}
			else
			{
				_blob.Write(ref pos, false);
			}
		}

		#endregion

		#region Method

		private MethodSignature ReadMethod(ref int pos)
		{
			if (_blob.ReadBoolean(ref pos))
			{
				// Generic method
				int genericArgumentCount = _blob.Read7BitEncodedInt(ref pos);
				var genericArguments = ReadReferenced<TypeSignature>(ref pos, genericArgumentCount);
				var declaringMethod = ReadReferenced<MethodReference>(ref pos);

				return new GenericMethodReference(declaringMethod, genericArguments);
			}
			else
			{
				// Declaring method
				string name = ReadString(ref pos);
				bool hasThis = _blob.ReadBoolean(ref pos);
				bool explicitThis = _blob.ReadBoolean(ref pos);
				var callConv = (MethodCallingConvention)_blob.ReadByte(ref pos);

				int varArgIndex = -1;
				if (callConv == MethodCallingConvention.VarArgs)
					varArgIndex = _blob.Read7BitEncodedInt(ref pos);

				int genericParameterCount = _blob.Read7BitEncodedInt(ref pos);

				int argumentCount = _blob.Read7BitEncodedInt(ref pos);

				TypeSignature owner = null;
				if (_blob.ReadBoolean(ref pos))
				{
					owner = ReadReferenced<TypeSignature>(ref pos);
				}

				var returnType = ReadReferenced<TypeSignature>(ref pos);

				var arguments = ReadReferenced<TypeSignature>(ref pos, argumentCount);

				var callSite = new CallSite(
					hasThis,
					explicitThis,
					callConv,
					returnType,
					arguments,
					varArgIndex,
					genericParameterCount);

				if (owner == null)
					return callSite;

				return new MethodReference(name, owner, callSite);
			}
		}

		private void WriteMethod(ref int pos, IMethodSignature method)
		{
			_blob.Write(ref pos, (byte)SignatureType.Method);

			var genericArguments = method.GenericArguments;
			if (genericArguments.Count > 0)
			{
				// Generic method
				_blob.Write(ref pos, true);
				_blob.Write7BitEncodedInt(ref pos, genericArguments.Count);

				_blob.Length += (genericArguments.Count + 1) * 4;

				WriteReferenced(ref pos, genericArguments);
				WriteReferenced(ref pos, method.DeclaringMethod);
			}
			else
			{
				// Declaring method
				_blob.Write(ref pos, false);
				WriteString(ref pos, method.Name);
				_blob.Write(ref pos, (bool)method.HasThis);
				_blob.Write(ref pos, (bool)method.ExplicitThis);
				_blob.Write(ref pos, (byte)method.CallConv);

				if (method.CallConv == MethodCallingConvention.VarArgs)
					_blob.Write7BitEncodedInt(ref pos, method.VarArgIndex);

				_blob.Write7BitEncodedInt(ref pos, method.GenericParameterCount);

				var arguments = method.Arguments;
				_blob.Write7BitEncodedInt(ref pos, arguments.Count);

				var owner = method.Owner;
				bool writeOwner = (owner != null);
				_blob.Write(ref pos, (bool)writeOwner);

				int addLength = (arguments.Count + 1) * 4;
				if (writeOwner)
					addLength += 4;

				_blob.Length += addLength;

				if (writeOwner)
				{
					WriteReferenced(ref pos, owner);
				}

				WriteReferenced(ref pos, method.ReturnType);
				WriteReferenced(ref pos, method.Arguments);
			}
		}

		#endregion

		#region Field

		private FieldReference ReadField(ref int pos)
		{
			string name = ReadString(ref pos);

			TypeSignature owner = null;
			if (_blob.ReadBoolean(ref pos))
			{
				owner = ReadReferenced<TypeSignature>(ref pos);
			}

			var fieldType = ReadReferenced<TypeSignature>(ref pos);

			return new FieldReference(name, fieldType, owner);
		}

		private void WriteField(ref int pos, IFieldSignature field)
		{
			_blob.Write(ref pos, (byte)SignatureType.Field);
			WriteString(ref pos, field.Name);

			var owner = field.Owner;
			bool writeOwner = (owner != null);
			_blob.Write(ref pos, (bool)writeOwner);

			int addLength = 4;
			if (writeOwner)
				addLength += 4;

			_blob.Length += addLength;

			if (writeOwner)
			{
				WriteReferenced(ref pos, owner);
			}

			WriteReferenced(ref pos, field.FieldType);
		}

		#endregion

		#region Property

		private PropertyReference ReadProperty(ref int pos)
		{
			string name = ReadString(ref pos);

			int argumentCount = _blob.Read7BitEncodedInt(ref pos);

			TypeSignature owner = null;
			if (_blob.ReadBoolean(ref pos))
			{
				owner = ReadReferenced<TypeSignature>(ref pos);
			}

			var returnType = ReadReferenced<TypeSignature>(ref pos);

			var arguments = ReadReferenced<TypeSignature>(ref pos, argumentCount);

			return new PropertyReference(name, owner, returnType, arguments);
		}

		private void WriteProperty(ref int pos, IPropertySignature property)
		{
			_blob.Write(ref pos, (byte)SignatureType.Property);
			WriteString(ref pos, property.Name);

			var arguments = property.Arguments;
			_blob.Write7BitEncodedInt(ref pos, arguments.Count);

			var owner = property.Owner;
			bool writeOwner = (owner != null);
			_blob.Write(ref pos, (bool)writeOwner);

			int addLength = (arguments.Count + 1) * 4;
			if (writeOwner)
				addLength += 4;

			_blob.Length += addLength;

			if (writeOwner)
			{
				WriteReferenced(ref pos, owner);
			}

			WriteReferenced(ref pos, property.ReturnType);
			WriteReferenced(ref pos, arguments);
		}

		#endregion

		#region Event

		private EventReference ReadEvent(ref int pos)
		{
			string name = ReadString(ref pos);

			TypeSignature owner = null;
			if (_blob.ReadBoolean(ref pos))
			{
				owner = ReadReferenced<TypeSignature>(ref pos);
			}

			var eventType = ReadReferenced<TypeSignature>(ref pos);

			return new EventReference(name, eventType, owner);
		}

		private void WriteEvent(ref int pos, IEventSignature e)
		{
			_blob.Write(ref pos, (byte)SignatureType.Event);
			WriteString(ref pos, e.Name);

			var owner = e.Owner;
			bool writeOwner = (owner != null);
			_blob.Write(ref pos, (bool)writeOwner);

			int addLength = 4;
			if (writeOwner)
				addLength += 4;

			_blob.Length += addLength;

			if (writeOwner)
			{
				WriteReferenced(ref pos, owner);
			}

			WriteReferenced(ref pos, e.EventType);
		}

		#endregion

		#endregion
	}
}
