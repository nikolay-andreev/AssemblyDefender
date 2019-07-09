using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	// TODO
	/// <summary>
	/// Generate new names for hierarchy members.
	/// </summary>
	public class MemberNameGenerator
	{
		#region Fields

		private int _stringIndex;
		private int _stringLength;
		private int _charSetLength;
		private int[] _pointers;
		private char[] _chars;
		private char[] _charSet;
		private string _mainTypeNamespace;
		private Random _random;
		private BuildLog _log;
		private HashList<string> _strings;
		private HashList<string> _existingStrings;

		#endregion

		#region Ctors

		public MemberNameGenerator(BuildLog log, Random random)
		{
			if (log == null)
				throw new ArgumentNullException("log");

			if (random == null)
				throw new ArgumentNullException("random");

			_random = random;
			_log = log;
			_charSet = AsciiCharSet;
			_charSet = _charSet.NewCopy();
			_charSet.Shuffle(_random);
			_charSetLength = _charSet.Length;
			_stringLength = 1;
			_chars = new char[_stringLength];
			_pointers = new int[_stringLength];
			_strings = new HashList<string>();
			_existingStrings = new HashList<string>();

			foreach (string name in log.NewNames)
			{
				_existingStrings.Add(name);
			}
		}

		#endregion

		#region Properties

		public int StringIndex
		{
			get { return _stringIndex; }
			set { _stringIndex = value; }
		}

		public string MainTypeNamespace
		{
			get { return _mainTypeNamespace; }
		}

		#endregion

		#region Methods

		public void Reset()
		{
			_stringIndex = 0;
		}

		public string GenerateString()
		{
			string s;
			do
			{
				if (_stringIndex < _strings.Count)
					s = _strings[_stringIndex];
				else
					s = GenerateNextString();

				_stringIndex++;
			}
			while (_existingStrings.Contains(s));

			return s;
		}

		public string GenerateUniqueString()
		{
			string s;
			do
			{
				s = GenerateNextString();
			}
			while (!_existingStrings.TryAdd(s));

			return s;
		}

		public void Generate()
		{
			// Main namesapce
			_mainTypeNamespace = _log.MainTypeNamespace;

			if (string.IsNullOrEmpty(_mainTypeNamespace))
			{
				_mainTypeNamespace = GenerateUniqueString();
				_log.MainTypeNamespace = _mainTypeNamespace;
			}
		}

		public void Generate(BuildAssembly assembly)
		{
			Reset();

			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				module.MainTypeName = GenerateString();
				GenerateTypes(module.Types);
				GenerateDelegateTypes(module.DelegateTypes);
				GenerateILCryptoInvokeTypes(module.ILCryptoInvokeTypes);
			}

			foreach (BuildModule module in assembly.Modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				foreach (BuildType type in module.Types)
				{
					GenerateMembers(type);
				}
			}
		}

		private void GenerateTypes(IEnumerable<TypeDeclaration> types)
		{
			foreach (BuildType type in types)
			{
				if (!type.Rename)
					continue;

				// Name
				if (!type.NameChanged)
				{
					if (type.NamespaceChanged)
					{
						type.NewName = GetMappedName(type.Name);
					}
					else
					{
						type.NewName = GetMappedName(type.FullName);

						// Namespace
						if (!string.IsNullOrEmpty(type.Namespace))
						{
							type.NewNamespace = _mainTypeNamespace;
							type.NamespaceChanged = true;
						}
					}

					type.NameChanged = true;
				}
			}
		}

		private void GenerateNestedTypes(IEnumerable<TypeDeclaration> types)
		{
			foreach (BuildType type in types)
			{
				if (!type.Rename)
					continue;

				// Name
				if (!type.NameChanged)
				{
					type.NewName = GetMappedName(type.Name);
					type.NameChanged = true;
				}
			}
		}

		private void GenerateDelegateTypes(IEnumerable<DelegateType> delegateTypes)
		{
			foreach (var delegateType in delegateTypes)
			{
				if (delegateType.DeclaringType == null)
				{
					delegateType.DeclaringType = new TypeReference(GenerateString(), _mainTypeNamespace, false);
				}
			}
		}

		private void GenerateILCryptoInvokeTypes(StateObjectList<ILCryptoInvokeType> invokeTypes)
		{
			int stringIndex = _stringIndex;
			_stringIndex = 0;
			string invokeMethodName = GenerateString();
			_stringIndex = stringIndex;

			foreach (var invokeType in invokeTypes)
			{
				invokeType.TypeName = GenerateString();
				invokeType.InvokeMethodName = invokeMethodName;
			}
		}

		private void GenerateMembers(BuildType type)
		{
			Reset();
			GenerateMethods(type.Methods);
			GenerateFields(type.Fields);
			GenerateProperties(type.Properties);
			GenerateEvents(type.Events);

			Reset();
			GenerateNestedTypes(type.NestedTypes);

			foreach (BuildType nestedType in type.NestedTypes)
			{
				GenerateMembers(nestedType);
			}
		}

		private void GenerateMethods(IEnumerable<MethodDeclaration> methods)
		{
			foreach (BuildMethod method in methods)
			{
				if (!method.Rename)
					continue;

				if (method.NameChanged)
					continue;

				if (method.Usage == BuildMethodUsage.BamlProperty)
				{
					string prefix = method.Name.Substring(0, 3);
					string name = method.Name.Substring(3);
					method.NewName = prefix + GetMappedName(name);
					method.NameChanged = true;
				}
				else
				{
					method.NewName = GetMappedName(method.Name);
					method.NameChanged = true;
				}
			}
		}

		private void GenerateFields(IEnumerable<FieldDeclaration> fields)
		{
			foreach (BuildField field in fields)
			{
				if (!field.Rename)
					continue;

				if (field.NameChanged)
					continue;

				field.NewName = GetMappedName(field.Name);
				field.NameChanged = true;
			}
		}

		private void GenerateProperties(IEnumerable<PropertyDeclaration> properties)
		{
			foreach (BuildProperty property in properties)
			{
				if (!property.Rename)
					continue;

				if (property.NameChanged)
					continue;

				property.NewName = GetMappedName(property.Name);
				property.NameChanged = true;
			}
		}

		private void GenerateEvents(IEnumerable<EventDeclaration> events)
		{
			foreach (BuildEvent e in events)
			{
				if (!e.Rename)
					continue;

				if (e.NameChanged)
					continue;

				e.NewName = GetMappedName(e.Name);
				e.NameChanged = true;
			}
		}

		private string GenerateNextString()
		{
			// Generate
			for (int i = 0; i < _stringLength; i++)
			{
				_chars[i] = _charSet[_pointers[i]];
			}

			// Create string and add to cache
			string s = new string(_chars);
			_strings.Add(s);

			// Move to next
			for (int i = _stringLength - 1; i >= 0; i--)
			{
				int pointer = _pointers[i] + 1;
				if (pointer < _charSetLength)
				{
					_pointers[i] = pointer;
					break;
				}

				if (i == 0)
				{
					// Increase string length
					_stringLength++;
					_chars = new char[_stringLength];
					_pointers = new int[_stringLength];
					break;
				}

				_pointers[i] = 0;
			}

			return s;
		}

		private string GetMappedName(string oldName)
		{
			if (oldName == null)
				return null;

			string newName;
			if (!_log.TryGetNewName(oldName, out newName))
			{
				//newName = GenerateUniqueString();
				newName = oldName.Replace('.', '_') + "_" + _random.NextString(5);
				//newName = oldName.Replace('.', '_') + "_NPA";
				_log.AddName(oldName, newName);
			}

			return newName;
		}

		#endregion

		#region Loading

		public void Load(BuildAssembly assembly)
		{
			foreach (BuildModule module in assembly.Modules)
			{
				Load(module);
			}
		}

		private void Load(BuildModule module)
		{
			foreach (BuildType type in module.Types)
			{
				Load(type);
			}

			foreach (var exportedType in module.ExportedTypes)
			{
				_existingStrings.TryAdd(exportedType.GetOutermostType().Name);
			}
		}

		private void Load(BuildType type)
		{
			_existingStrings.TryAdd(type.Name);
			LoadNamespace(type.Namespace);

			if (type.NameChanged)
				_existingStrings.TryAdd(type.NewName);

			if (type.NamespaceChanged)
				LoadNamespace(type.NewNamespace);

			foreach (BuildMethod method in type.Methods)
			{
				_existingStrings.TryAdd(method.Name);

				if (method.NameChanged)
					_existingStrings.TryAdd(method.NewName);
			}

			foreach (BuildField field in type.Fields)
			{
				_existingStrings.TryAdd(field.Name);

				if (field.NameChanged)
					_existingStrings.TryAdd(field.NewName);
			}

			foreach (BuildProperty property in type.Properties)
			{
				_existingStrings.TryAdd(property.Name);

				if (property.NameChanged)
					_existingStrings.TryAdd(property.NewName);
			}

			foreach (BuildEvent e in type.Events)
			{
				_existingStrings.TryAdd(e.Name);

				if (e.NameChanged)
					_existingStrings.TryAdd(e.NewName);
			}

			foreach (BuildType nestedType in type.NestedTypes)
			{
				Load(nestedType);
			}
		}

		private void LoadNamespace(string ns)
		{
			if (string.IsNullOrEmpty(ns))
				return;

			string[] names = ns.Split('.', true);

			foreach (string name in names)
			{
				_existingStrings.TryAdd(name);
			}
		}

		#endregion

		#region Character sets

		private static char[] AsciiCharSet = new char[]
		{
			'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
			'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
		};

		#endregion
	}
}
