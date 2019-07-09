using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Reflection style parser.
	/// </summary>
	internal class ReflectionSignatureParser
	{
		#region Fields

		private ReflectionSignatureLexer _lexer;

		#endregion

		#region Ctors

		internal ReflectionSignatureParser(string inputText)
		{
			_lexer = new ReflectionSignatureLexer(inputText);
		}

		#endregion

		#region Methods

		internal AssemblyReference ParseAssembly()
		{
			_lexer.ReadWhiteSpaces();
			if (_lexer.IsAtEndOfInput)
				return null;

			string name = _lexer.ReadIdentity().Trim();
			if (string.IsNullOrEmpty(name))
				return null;

			string culture = null;
			Version version = null;
			byte[] publicKeyToken = null;
			var processorArchitecture = ProcessorArchitecture.None;

			while (_lexer.Peek() == ',')
			{
				_lexer.Move(); // ','

				string key = _lexer.ReadIdentity().Trim();
				if (key == string.Empty)
					return null;

				if (_lexer.Peek() != '=')
					return null;

				_lexer.Move(); // '='

				string value = _lexer.ReadIdentity().Trim();
				if (value == string.Empty)
					return null;

				switch (key.ToLower())
				{
					case "version":
						{
							try
							{
								version = new Version(value);
							}
							catch (Exception)
							{
								return null;
							}
						}
						break;

					case "culture":
						{
							culture = value;
						}
						break;

					case "publickeytoken":
						{
							if (0 != string.Compare(value, "null", StringComparison.OrdinalIgnoreCase))
							{
								if (value.Length != 16)
									return null;

								try
								{
									publicKeyToken = ConvertUtils.HexToByteArray(value, false);
								}
								catch (Exception)
								{
									return null;
								}
							}
						}
						break;

					case "processorarchitecture":
						{
							switch (value.ToLower())
							{
								case "none":
									break;

								case "msil":
									processorArchitecture = ProcessorArchitecture.MSIL;
									break;

								case "x86":
									processorArchitecture = ProcessorArchitecture.X86;
									break;

								case "ia64":
									processorArchitecture = ProcessorArchitecture.IA64;
									break;

								case "amd64":
									processorArchitecture = ProcessorArchitecture.Amd64;
									break;

								default:
									return null;
							}
						}
						break;
				}
			}

			return new AssemblyReference(name, culture, version, publicKeyToken, processorArchitecture);
		}

		internal TypeSignature ParseType()
		{
			return ParseType(true);
		}

		internal TypeReference ParseTypeRef()
		{
			return ParseTypeRef(true);
		}

		private TypeSignature ParseType(bool parseAssembly)
		{
			var typeRef = ParseTypeRef(false);
			if (typeRef == null)
				return null;

			var typeSig = (TypeSignature)typeRef;

			_lexer.ReadWhiteSpaces();

			if (IsGenericArguments())
			{
				typeSig = ParseGenericTypeRef(typeRef);
				_lexer.ReadWhiteSpaces();
			}

			bool breakFlag = false;
			while (!_lexer.IsAtEndOfInput)
			{
				switch (_lexer.Peek())
				{
					case '[':
						{
							typeSig = ParseArrayType(typeSig);
							if (typeSig == null)
								return null;
						}
						break;

					case '&':
						{
							_lexer.Move(); // &
							typeSig = new ByRefType(typeSig);
						}
						break;

					case '*':
						{
							_lexer.Move(); // *
							typeSig = new PointerType(typeSig);
						}
						break;

					default:
						{
							breakFlag = true;
						}
						break;
				}

				if (breakFlag)
					break;

				_lexer.ReadWhiteSpaces();
			}

			_lexer.ReadWhiteSpaces();

			if (parseAssembly && _lexer.Peek() == ',')
			{
				_lexer.Move();

				var assembly = ParseAssembly();
				if (assembly == null)
					return null;

				while (typeRef.Owner != null && typeRef.Owner.SignatureType == SignatureType.Type)
				{
					typeRef = (TypeReference)typeRef.Owner;
				}

				typeRef.SetOwner(assembly);
			}

			return typeSig;
		}

		private TypeSignature ParseArrayType(TypeSignature elementType)
		{
			_lexer.Move(); // [

			_lexer.ReadWhiteSpaces();

			int rank = 0;

			while (!_lexer.IsAtEndOfInput)
			{
				if (_lexer.Peek() == ']')
					break;

				char ch = (char)_lexer.Read();
				if (ch == ',')
				{
					rank++;
				}
				else if (ch == '*')
				{
					if (rank > 0)
						return null;

					break;
				}
				else
				{
					return null;
				}

				_lexer.ReadWhiteSpaces();
			}

			if (_lexer.Read() != ']')
				return null;

			if (rank > 0)
				rank++; // first rank

			return new ArrayType(elementType, rank);
		}

		private TypeReference ParseTypeRef(bool parseAssembly)
		{
			string fullName = _lexer.ReadIdentity().Trim();
			if (string.IsNullOrEmpty(fullName))
				return null;

			string name;
			string ns;

			int lastDotIndex = fullName.LastIndexOf('.');
			if (lastDotIndex > 0 && lastDotIndex < fullName.Length - 1)
			{
				name = fullName.Substring(lastDotIndex + 1);
				ns = fullName.Substring(0, lastDotIndex);
			}
			else
			{
				name = fullName;
				ns = null;
			}

			var typeRef = new TypeReference(name, ns);
			var nestedTypeRef = typeRef;

			_lexer.ReadWhiteSpaces();

			while (_lexer.Peek() == '+')
			{
				_lexer.Move();

				name = _lexer.ReadIdentity().Trim();
				if (string.IsNullOrEmpty(name))
					return null;

				nestedTypeRef = new TypeReference(name, null, typeRef);
				typeRef = nestedTypeRef;

				_lexer.ReadWhiteSpaces();
			}

			if (parseAssembly && _lexer.Peek() == ',')
			{
				_lexer.Read();

				var assembly = ParseAssembly();
				if (assembly == null)
					return null;

				typeRef.SetOwner(assembly);
			}

			return nestedTypeRef;
		}

		private TypeSignature ParseGenericTypeRef(TypeReference typeRef)
		{
			_lexer.Move(); // '['

			var genericArguments = ParseGenericArguments();
			if (genericArguments == null)
				return null;

			_lexer.ReadWhiteSpaces();

			if (_lexer.Read() != ']')
				return null;

			return new GenericTypeReference(typeRef, genericArguments);
		}

		private TypeSignature[] ParseGenericArguments()
		{
			var list = new List<TypeSignature>();

			while (true)
			{
				var genericArgument = ParseGenericArgument();
				if (genericArgument == null)
					return null;

				list.Add(genericArgument);

				_lexer.ReadWhiteSpaces();

				if (_lexer.Peek() != ',')
					break;

				_lexer.Move();
			}

			return list.ToArray();
		}

		private TypeSignature ParseGenericArgument()
		{
			_lexer.ReadWhiteSpaces();

			bool hasBrace = (_lexer.Peek() == '[');

			if (hasBrace && _lexer.Read() != '[')
				return null;

			var genericArgument = ParseType(hasBrace);
			if (genericArgument == null)
				return null;

			_lexer.ReadWhiteSpaces();

			if (hasBrace && _lexer.Read() != ']')
				return null;

			return genericArgument;
		}

		private bool IsGenericArguments()
		{
			if (_lexer.Peek() != '[')
				return false;

			switch (_lexer.Peek(1, true))
			{
				// Array
				case ']':
				case ',':
				case '*':
					return false;

				default:
					return true;
			}
		}

		#endregion
	}
}
