using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;
using CA = AssemblyDefender.Net.CustomAttributes;

namespace AssemblyDefender
{
	internal class BuildAssemblyLoader
	{
		#region Fields

		private bool _obfuscateControlFlow;
		private bool _renameMembers;
		private bool _encryptIL;
		private bool _obfuscateResources;
		private bool _obfuscateStrings;
		private bool _removeUnusedMembers;
		private bool _sealTypes;
		private bool _devirtualizeMethods;
		private List<TupleStruct<string, string>> _renamedAssemblyNames;

		#endregion

		#region Ctors

		internal BuildAssemblyLoader()
		{
			_renamedAssemblyNames = new List<TupleStruct<string, string>>();
		}

		#endregion

		#region Properties

		internal bool ObfuscateControlFlow
		{
			get { return _obfuscateControlFlow; }
		}

		internal bool RenameMembers
		{
			get { return _renameMembers; }
		}

		internal bool EncryptIL
		{
			get { return _encryptIL; }
		}

		internal bool ObfuscateResources
		{
			get { return _obfuscateResources; }
		}

		internal bool ObfuscateStrings
		{
			get { return _obfuscateStrings; }
		}

		internal bool RemoveUnusedMembers
		{
			get { return _removeUnusedMembers; }
		}

		internal bool SealTypes
		{
			get { return _sealTypes; }
		}

		internal bool DevirtualizeMethods
		{
			get { return _devirtualizeMethods; }
		}

		internal List<TupleStruct<string, string>> RenamedAssemblyNames
		{
			get { return _renamedAssemblyNames; }
		}

		#endregion

		#region Methods

		private void LoadObfuscationAttribute(LoadState state, string featuresToken, bool exclude)
		{
			if (string.IsNullOrEmpty(featuresToken))
				return;

			featuresToken = featuresToken.ToLower();
			string[] features = featuresToken.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			if (features == null || features.Length == 0)
				return;

			foreach (string feature in features)
			{
				switch (feature)
				{
					case "rename":
						state.RenameMembers = state.CanRenameMembers && !exclude;
						break;

					case "controlflow":
						state.ObfuscateControlFlow = state.CanObfuscateControlFlow && !exclude;
						break;

					case "encryptil":
						state.EncryptIL = state.CanEncryptIL && !exclude;
						break;

					case "strings":
						state.ObfuscateStrings = state.CanObfuscateStrings && !exclude;
						break;

					case "resources":
						state.ObfuscateResources = state.CanObfuscateResources && !exclude;
						break;

					case "remove":
						state.RemoveUnusedMembers = state.CanRemoveUnusedMembers && !exclude;
						break;

					case "seal":
						state.SealTypes = state.CanSealTypes && !exclude;
						break;

					case "devirtualize":
						state.DevirtualizeMethods = state.CanDevirtualizeMethods && !exclude;
						break;

					case "all":
						state.RenameMembers = state.CanRenameMembers && !exclude;
						state.ObfuscateControlFlow = state.CanObfuscateControlFlow && !exclude;
						state.EncryptIL = state.CanEncryptIL && !exclude;
						state.ObfuscateStrings = state.CanObfuscateStrings && !exclude;
						state.ObfuscateResources = state.CanObfuscateResources && !exclude;
						state.RemoveUnusedMembers = state.CanRemoveUnusedMembers && !exclude;
						state.SealTypes = state.CanSealTypes && !exclude;
						state.DevirtualizeMethods = state.CanDevirtualizeMethods && !exclude;
						break;
				}
			}
		}

		#endregion

		#region Assembly

		internal void Load(BuildAssembly assembly, ProjectAssembly projectAssembly)
		{
			var state = new LoadState();
			state.ProjectAssembly = projectAssembly;

			// Set output path
			string outputPath = projectAssembly.OutputPath ?? Path.GetDirectoryName(assembly.Location);
			assembly.OutputPath = outputPath;
			DirectoryUtils.CreateDirectoryIfMissing(assembly.OutputPath);

			if (projectAssembly.IgnoreObfuscationAttribute)
			{
				assembly.IgnoreObfuscationAttribute = true;
				state.IgnoreObfuscationAttribute = true;
			}

			// Load ObfuscationAttribute
			foreach (var attribute in CA.ObfuscationAttribute.FindAll(assembly.CustomAttributes))
			{
				if (attribute.StripAfterObfuscation)
				{
					assembly.StripObfuscationAttribute = true;
					state.StripObfuscationAttributeExists = true;
				}
			}

			if (projectAssembly.SuppressILdasm)
			{
				state.SuppressILdasm = true;
				assembly.SuppressILdasm = true;
			}

			if (projectAssembly.ObfuscateControlFlow)
			{
				_obfuscateControlFlow = true;
				state.CanObfuscateControlFlow = true;
				state.ObfuscateControlFlow = true;
				state.ObfuscateControlFlowAtomic = projectAssembly.ObfuscateControlFlowAtomic;
				assembly.ObfuscateControlFlow = true;
			}

			if (projectAssembly.RenameMembers)
			{
				_renameMembers = true;
				state.CanRenameMembers = true;
				state.RenameMembers = true;
				state.RenamePublicTypes = projectAssembly.RenamePublicTypes;
				state.RenamePublicMethods = projectAssembly.RenamePublicMethods;
				state.RenamePublicFields = projectAssembly.RenamePublicFields;
				state.RenamePublicProperties = projectAssembly.RenamePublicProperties;
				state.RenamePublicEvents = projectAssembly.RenamePublicEvents;
				state.RenameEnumMembers = projectAssembly.RenameEnumMembers;
				state.RenameSerializableTypes = projectAssembly.RenameSerializableMembers;
				state.RenameConfigurationTypes = projectAssembly.RenameConfigurationMembers;
				assembly.RenameMembers = true;
			}

			if (projectAssembly.EncryptIL)
			{
				_encryptIL = true;
				state.CanEncryptIL = true;
				state.EncryptIL = true;
				state.EncryptILAtomic = projectAssembly.EncryptILAtomic;
				assembly.EncryptIL = true;
			}

			if (projectAssembly.ObfuscateResources && assembly.Framework.Version.Major >= 4)
			{
				_obfuscateResources = true;
				state.CanObfuscateResources = true;
				state.ObfuscateResources = true;
				assembly.ObfuscateResources = true;
			}

			if (projectAssembly.ObfuscateStrings)
			{
				_obfuscateStrings = true;
				state.CanObfuscateStrings = true;
				state.ObfuscateStrings = true;
				assembly.ObfuscateStrings = true;
			}

			if (projectAssembly.RemoveUnusedMembers)
			{
				_removeUnusedMembers = true;
				state.CanRemoveUnusedMembers = true;
				state.RemoveUnusedMembers = true;
				state.RemoveUnusedPublicMembers = projectAssembly.RemoveUnusedPublicMembers;
				assembly.RemoveUnusedMembers = true;
			}

			if (projectAssembly.SealTypes)
			{
				_sealTypes = true;
				state.CanSealTypes = true;
				state.SealTypes = true;
				state.SealPublicTypes = projectAssembly.SealPublicTypes;
				assembly.SealTypes = true;
			}

			if (projectAssembly.DevirtualizeMethods)
			{
				_devirtualizeMethods = true;
				state.CanDevirtualizeMethods = true;
				state.DevirtualizeMethods = true;
				state.DevirtualizePublicMethods = projectAssembly.DevirtualizePublicMethods;
				assembly.DevirtualizeMethods = true;
			}

			bool assemblyNameChanged = false;
			string newName;
			string newCulture;
			Version newVersion;
			byte[] newPublicKeyToken;

			if (projectAssembly.NameChanged)
			{
				newName = projectAssembly.Name;

				assembly.NameChanged = true;
				assembly.NewName = newName;

				var module = (BuildModule)assembly.Module;
				module.NameChanged = true;
				module.NewName = newName + Path.GetExtension(module.Name);

				assemblyNameChanged = true;
			}
			else
			{
				newName = assembly.Name;
			}

			if (projectAssembly.CultureChanged)
			{
				newCulture = projectAssembly.Culture;
				assembly.CultureChanged = true;
				assembly.NewCulture = newCulture;
				assemblyNameChanged = true;
			}
			else
			{
				newCulture = assembly.Culture;
			}

			if (projectAssembly.VersionChanged)
			{
				newVersion = projectAssembly.Version;
				assembly.VersionChanged = true;
				assembly.NewVersion = newVersion;
				assemblyNameChanged = true;
			}
			else
			{
				newVersion = assembly.Version;
			}

			if (projectAssembly.TitleChanged)
			{
				assembly.TitleChanged = true;
				assembly.NewTitle = projectAssembly.Title;
			}

			if (projectAssembly.DescriptionChanged)
			{
				assembly.DescriptionChanged = true;
				assembly.NewDescription = projectAssembly.Description;
			}

			if (projectAssembly.CompanyChanged)
			{
				assembly.CompanyChanged = true;
				assembly.NewCompany = projectAssembly.Company;
			}

			if (projectAssembly.ProductChanged)
			{
				assembly.ProductChanged = true;
				assembly.NewProduct = projectAssembly.Product;
			}

			if (projectAssembly.CopyrightChanged)
			{
				assembly.CopyrightChanged = true;
				assembly.NewCopyright = projectAssembly.Copyright;
			}

			if (projectAssembly.TrademarkChanged)
			{
				assembly.TrademarkChanged = true;
				assembly.NewTrademark = projectAssembly.Trademark;
			}

			// PublicKey
			var newPublicKey = GetPublicKey(assembly, projectAssembly);
			if (!CompareUtils.Equals(assembly.PublicKey, newPublicKey, true))
			{
				assembly.PublicKeyChanged = true;

				if (newPublicKey != null)
				{
					newPublicKeyToken = StrongNameUtils.CreateTokenFromPublicKey(newPublicKey);
					assembly.NewPublicKey = newPublicKey;
					assembly.NewPublicKeyToken = newPublicKeyToken;
				}
				else
				{
					newPublicKeyToken = null;
				}

				assemblyNameChanged = true;
			}
			else
			{
				newPublicKeyToken = assembly.PublicKeyToken;
			}

			// Signing
			var projectSign = projectAssembly.Sign;
			if (projectSign != null)
			{
				// Signed with key or delay sign.
				assembly.IsStrongNameSignedAfterBuild = true;

				if (!string.IsNullOrEmpty(projectSign.KeyFile))
				{
					assembly.StrongNameKeyFilePath = projectSign.KeyFile;
					assembly.StrongNameKeyPassword = projectSign.Password;
				}
			}

			if (assemblyNameChanged)
			{
				_renamedAssemblyNames.Add(
					new TupleStruct<string, string>(
						assembly.Name,
						SignaturePrinter.PrintAssembly(newName, newCulture, newVersion, newPublicKeyToken)));
			}

			LoadModules(assembly.Modules, state);
			LoadResources(assembly.Resources, state);

			assembly.StripObfuscationAttributeExists = state.StripObfuscationAttributeExists;
		}

		private byte[] GetPublicKey(BuildAssembly assembly, ProjectAssembly projectAssembly)
		{
			var projectSign = projectAssembly.Sign;
			if (projectSign == null)
				return null;

			if (!string.IsNullOrEmpty(projectSign.KeyFile))
			{
				byte[] keyPair = File.ReadAllBytes(projectSign.KeyFile);

				if (!string.IsNullOrEmpty(projectSign.Password))
				{
					keyPair = StrongNameUtils.ExtractKeyPairFromPKCS12(keyPair, projectSign.Password);
				}

				return StrongNameUtils.ExtractPublicKeyFromKeyPair(keyPair);
			}
			else
			{
				if (projectSign.DelaySign)
				{
					return assembly.PublicKey;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Module

		private void LoadModules(ModuleCollection modules, LoadState state)
		{
			var projectModules = state.ProjectAssembly.Modules;

			foreach (BuildModule module in modules)
			{
				if (!module.Image.IsILOnly)
					continue;

				ProjectModule projectModule;
				projectModules.TryGetValue(module.Name, out projectModule);
				LoadModule(module, projectModule, state);
			}
		}

		private void LoadModule(BuildModule module, ProjectModule projectModule, LoadState state)
		{
			state.ProjectModule = projectModule;

			// Load ObfuscationAttribute
			foreach (var attribute in CA.ObfuscationAttribute.FindAll(module.CustomAttributes))
			{
				if (attribute.StripAfterObfuscation)
				{
					module.StripObfuscationAttribute = true;
					state.StripObfuscationAttributeExists = true;
				}
			}

			// Mark
			if (projectModule != null)
			{
				if (projectModule.NameChanged && !module.IsPrimeModule)
				{
					module.NameChanged = true;
					module.NewName = projectModule.Name;
				}
			}

			LoadModuleMembers(module, projectModule, state);
		}

		private void LoadModuleMembers(BuildModule module, ProjectModule projectModule, LoadState state)
		{
			if (projectModule != null)
			{
				// Backup
				bool obfuscateControlFlowBak = state.ObfuscateControlFlow;
				bool renameMembersBak = state.RenameMembers;
				bool renamePublicTypesBak = state.RenamePublicTypes;
				bool renamePublicMethodsBak = state.RenamePublicMethods;
				bool renamePublicFieldsBak = state.RenamePublicFields;
				bool renamePublicPropertiesBak = state.RenamePublicProperties;
				bool renamePublicEventsBak = state.RenamePublicEvents;
				bool encryptILBak = state.EncryptIL;
				bool obfuscateStringsBak = state.ObfuscateStrings;
				bool removeUnusedMembersBak = state.RemoveUnusedMembers;
				bool sealTypesBak = state.SealTypes;
				bool devirtualizeMethodsBak = state.DevirtualizeMethods;

				// Set new values.
				if (state.CanObfuscateControlFlow && projectModule.ObfuscateControlFlowChanged)
					state.ObfuscateControlFlow = projectModule.ObfuscateControlFlow;

				if (state.CanRenameMembers)
				{
					if (projectModule.RenameMembersChanged)
						state.RenameMembers = projectModule.RenameMembers;

					if (projectModule.RenamePublicTypesChanged)
						state.RenamePublicTypes = projectModule.RenamePublicTypes;

					if (projectModule.RenamePublicMethodsChanged)
						state.RenamePublicMethods = projectModule.RenamePublicMethods;

					if (projectModule.RenamePublicFieldsChanged)
						state.RenamePublicFields = projectModule.RenamePublicFields;

					if (projectModule.RenamePublicPropertiesChanged)
						state.RenamePublicProperties = projectModule.RenamePublicProperties;

					if (projectModule.RenamePublicEventsChanged)
						state.RenamePublicEvents = projectModule.RenamePublicEvents;
				}

				if (state.CanEncryptIL && projectModule.EncryptILChanged)
					state.EncryptIL = projectModule.EncryptIL;

				if (state.CanObfuscateStrings && projectModule.ObfuscateStringsChanged)
					state.ObfuscateStrings = projectModule.ObfuscateStrings;

				if (state.CanRemoveUnusedMembers && projectModule.RemoveUnusedMembersChanged)
					state.RemoveUnusedMembers = projectModule.RemoveUnusedMembers;

				if (state.CanSealTypes && projectModule.SealTypesChanged)
					state.SealTypes = projectModule.SealTypes;

				if (state.CanDevirtualizeMethods && projectModule.DevirtualizeMethodsChanged)
					state.DevirtualizeMethods = projectModule.DevirtualizeMethods;

				// Mark
				LoadNamespaces(module.Types, state);

				// Restore
				state.ObfuscateControlFlow = obfuscateControlFlowBak;
				state.RenameMembers = renameMembersBak;
				state.RenamePublicTypes = renamePublicTypesBak;
				state.RenamePublicMethods = renamePublicMethodsBak;
				state.RenamePublicFields = renamePublicFieldsBak;
				state.RenamePublicProperties = renamePublicPropertiesBak;
				state.RenamePublicEvents = renamePublicEventsBak;
				state.EncryptIL = encryptILBak;
				state.ObfuscateStrings = obfuscateStringsBak;
				state.RemoveUnusedMembers = removeUnusedMembersBak;
				state.SealTypes = sealTypesBak;
				state.DevirtualizeMethods = devirtualizeMethodsBak;
			}
			else
			{
				LoadNamespaces(module.Types, state);
			}
		}

		#endregion

		#region Namespace

		private void LoadNamespaces(TypeDeclarationCollection types, LoadState state)
		{
			if (types.Count == 0)
				return;

			var projectNamespaces = (state.ProjectModule != null) ? state.ProjectModule.Namespaces : null;

			if (projectNamespaces != null && projectNamespaces.Count > 0)
			{
				var typesByNamespace = new Dictionary<string, List<TypeDeclaration>>();
				foreach (var type in types)
				{
					string ns = type.Namespace ?? "";
					List<TypeDeclaration> nsTypes;
					if (!typesByNamespace.TryGetValue(ns, out nsTypes))
					{
						nsTypes = new List<TypeDeclaration>();
						typesByNamespace.Add(ns, nsTypes);
					}

					nsTypes.Add(type);
				}

				foreach (var kvp in typesByNamespace)
				{
					string ns = kvp.Key;
					var nsTypes = kvp.Value;

					ProjectNamespace projectNamespace;
					projectNamespaces.TryGetValue(ns, out projectNamespace);
					LoadNamespace(nsTypes, projectNamespace, state);
				}
			}
			else
			{
				LoadTypes(types, state);
			}
		}

		private void LoadNamespace(IEnumerable<TypeDeclaration> types, ProjectNamespace projectNamespace, LoadState state)
		{
			if (projectNamespace != null)
			{
				// Backup
				bool obfuscateControlFlowBak = state.ObfuscateControlFlow;
				bool renameMembersBak = state.RenameMembers;
				bool renamePublicTypesBak = state.RenamePublicTypes;
				bool renamePublicMethodsBak = state.RenamePublicMethods;
				bool renamePublicFieldsBak = state.RenamePublicFields;
				bool renamePublicPropertiesBak = state.RenamePublicProperties;
				bool renamePublicEventsBak = state.RenamePublicEvents;
				bool encryptILBak = state.EncryptIL;
				bool obfuscateStringsBak = state.ObfuscateStrings;
				bool removeUnusedMembersBak = state.RemoveUnusedMembers;
				bool sealTypesBak = state.SealTypes;
				bool devirtualizeMethodsBak = state.DevirtualizeMethods;

				// Set new values.
				if (state.CanObfuscateControlFlow && projectNamespace.ObfuscateControlFlowChanged)
					state.ObfuscateControlFlow = projectNamespace.ObfuscateControlFlow;

				if (state.CanRenameMembers)
				{
					if (projectNamespace.RenameMembersChanged)
						state.RenameMembers = projectNamespace.RenameMembers;

					if (projectNamespace.RenamePublicTypesChanged)
						state.RenamePublicTypes = projectNamespace.RenamePublicTypes;

					if (projectNamespace.RenamePublicMethodsChanged)
						state.RenamePublicMethods = projectNamespace.RenamePublicMethods;

					if (projectNamespace.RenamePublicFieldsChanged)
						state.RenamePublicFields = projectNamespace.RenamePublicFields;

					if (projectNamespace.RenamePublicPropertiesChanged)
						state.RenamePublicProperties = projectNamespace.RenamePublicProperties;

					if (projectNamespace.RenamePublicEventsChanged)
						state.RenamePublicEvents = projectNamespace.RenamePublicEvents;
				}

				if (state.CanEncryptIL && projectNamespace.EncryptILChanged)
					state.EncryptIL = projectNamespace.EncryptIL;

				if (state.CanObfuscateStrings && projectNamespace.ObfuscateStringsChanged)
					state.ObfuscateStrings = projectNamespace.ObfuscateStrings;

				if (state.CanRemoveUnusedMembers && projectNamespace.RemoveUnusedMembersChanged)
					state.RemoveUnusedMembers = projectNamespace.RemoveUnusedMembers;

				if (state.CanSealTypes && projectNamespace.SealTypesChanged)
					state.SealTypes = projectNamespace.SealTypes;

				if (state.CanDevirtualizeMethods && projectNamespace.DevirtualizeMethodsChanged)
					state.DevirtualizeMethods = projectNamespace.DevirtualizeMethods;

				// Mark
				LoadTypes(types, state);

				// Restore
				state.ObfuscateControlFlow = obfuscateControlFlowBak;
				state.RenameMembers = renameMembersBak;
				state.RenamePublicTypes = renamePublicTypesBak;
				state.RenamePublicMethods = renamePublicMethodsBak;
				state.RenamePublicFields = renamePublicFieldsBak;
				state.RenamePublicProperties = renamePublicPropertiesBak;
				state.RenamePublicEvents = renamePublicEventsBak;
				state.EncryptIL = encryptILBak;
				state.ObfuscateStrings = obfuscateStringsBak;
				state.RemoveUnusedMembers = removeUnusedMembersBak;
				state.SealTypes = sealTypesBak;
				state.DevirtualizeMethods = devirtualizeMethodsBak;
			}
			else
			{
				LoadTypes(types, state);
			}
		}

		#endregion

		#region Type

		private void LoadTypes(IEnumerable<TypeDeclaration> types, LoadState state)
		{
			var projectTypes = (state.ProjectModule != null) ? state.ProjectModule.Types : null;

			if (projectTypes != null && projectTypes.Count > 0)
			{
				foreach (BuildType type in types)
				{
					ProjectType projectType;
					projectTypes.TryGetValue(type, out projectType);
					LoadType(type, projectType, state);
				}
			}
			else
			{
				foreach (BuildType type in types)
				{
					LoadType(type, null, state);
				}
			}
		}

		private void LoadType(BuildType type, ProjectType projectType, LoadState state)
		{
			// Backup
			bool obfuscateControlFlowBak = state.ObfuscateControlFlow;
			bool renameMembersBak = state.RenameMembers;
			bool encryptILBak = state.EncryptIL;
			bool obfuscateStringsBak = state.ObfuscateStrings;
			bool removeUnusedMembersBak = state.RemoveUnusedMembers;
			bool sealTypesBak = state.SealTypes;
			bool devirtualizeMethodsBak = state.DevirtualizeMethods;
			bool applyToMembers = false;

			// Load ObfuscationAttribute
			foreach (var attribute in CA.ObfuscationAttribute.FindAll(type.CustomAttributes))
			{
				if (!state.IgnoreObfuscationAttribute)
				{
					LoadObfuscationAttribute(state, attribute.Feature, attribute.Exclude);
					applyToMembers = attribute.ApplyToMembers;
				}

				if (attribute.StripAfterObfuscation)
				{
					type.StripObfuscationAttribute = true;
					state.StripObfuscationAttributeExists = true;
				}
			}

			// Mark
			var typeState = new TypeLoadState(type, projectType, state);

			if (typeState.CanRename())
			{
				type.Rename = true;

				if (projectType != null)
				{
					if (projectType.NameChanged)
					{
						type.NameChanged = true;
						type.NewName = projectType.Name;
					}

					if (projectType.NamespaceChanged)
					{
						type.NamespaceChanged = true;
						type.NewNamespace = projectType.Namespace;
					}
				}
			}

			if (typeState.CanRemoveUnused())
			{
				type.Strip = true;
			}

			if (typeState.CanSealType())
			{
				type.SealType = true;
			}

			if (applyToMembers)
			{
				LoadTypeMembers(type, projectType, state);
			}

			// Restore
			state.ObfuscateControlFlow = obfuscateControlFlowBak;
			state.RenameMembers = renameMembersBak;
			state.EncryptIL = encryptILBak;
			state.ObfuscateStrings = obfuscateStringsBak;
			state.RemoveUnusedMembers = removeUnusedMembersBak;
			state.SealTypes = sealTypesBak;
			state.DevirtualizeMethods = devirtualizeMethodsBak;

			if (!applyToMembers)
			{
				LoadTypeMembers(type, projectType, state);
			}
		}

		private void LoadTypeMembers(TypeDeclaration type, ProjectType projectType, LoadState state)
		{
			if (projectType != null)
			{
				// Backup
				bool obfuscateControlFlowBak = state.ObfuscateControlFlow;
				bool renameMembersBak = state.RenameMembers;
				bool renamePublicTypesBak = state.RenamePublicTypes;
				bool renamePublicMethodsBak = state.RenamePublicMethods;
				bool renamePublicFieldsBak = state.RenamePublicFields;
				bool renamePublicPropertiesBak = state.RenamePublicProperties;
				bool renamePublicEventsBak = state.RenamePublicEvents;
				bool encryptILBak = state.EncryptIL;
				bool obfuscateStringsBak = state.ObfuscateStrings;
				bool removeUnusedMembersBak = state.RemoveUnusedMembers;
				bool sealTypesBak = state.SealTypes;
				bool devirtualizeMethodsBak = state.DevirtualizeMethods;

				// Set new values.
				if (state.CanObfuscateControlFlow && projectType.ObfuscateControlFlowChanged)
					state.ObfuscateControlFlow = projectType.ObfuscateControlFlow;

				if (state.CanRenameMembers)
				{
					if (projectType.RenameMembersChanged)
						state.RenameMembers = projectType.RenameMembers;

					if (projectType.RenamePublicTypesChanged)
						state.RenamePublicTypes = projectType.RenamePublicTypes;

					if (projectType.RenamePublicMethodsChanged)
						state.RenamePublicMethods = projectType.RenamePublicMethods;

					if (projectType.RenamePublicFieldsChanged)
						state.RenamePublicFields = projectType.RenamePublicFields;

					if (projectType.RenamePublicPropertiesChanged)
						state.RenamePublicProperties = projectType.RenamePublicProperties;

					if (projectType.RenamePublicEventsChanged)
						state.RenamePublicEvents = projectType.RenamePublicEvents;
				}

				if (state.CanEncryptIL && projectType.EncryptILChanged)
					state.EncryptIL = projectType.EncryptIL;

				if (state.CanObfuscateStrings && projectType.ObfuscateStringsChanged)
					state.ObfuscateStrings = projectType.ObfuscateStrings;

				if (state.CanRemoveUnusedMembers && projectType.RemoveUnusedMembersChanged)
					state.RemoveUnusedMembers = projectType.RemoveUnusedMembers;

				if (state.CanSealTypes && projectType.SealTypesChanged)
					state.SealTypes = projectType.SealTypes;

				if (state.CanDevirtualizeMethods && projectType.DevirtualizeMethodsChanged)
					state.DevirtualizeMethods = projectType.DevirtualizeMethods;

				// Load
				LoadMethods(type.Methods, state);
				LoadFields(type.Fields, state);
				LoadProperties(type.Properties, state);
				LoadEvents(type.Events, state);
				LoadTypes(type.NestedTypes, state);

				// Restore
				state.ObfuscateControlFlow = obfuscateControlFlowBak;
				state.RenameMembers = renameMembersBak;
				state.RenamePublicTypes = renamePublicTypesBak;
				state.RenamePublicMethods = renamePublicMethodsBak;
				state.RenamePublicFields = renamePublicFieldsBak;
				state.RenamePublicProperties = renamePublicPropertiesBak;
				state.RenamePublicEvents = renamePublicEventsBak;
				state.EncryptIL = encryptILBak;
				state.ObfuscateStrings = obfuscateStringsBak;
				state.RemoveUnusedMembers = removeUnusedMembersBak;
				state.SealTypes = sealTypesBak;
				state.DevirtualizeMethods = devirtualizeMethodsBak;
			}
			else
			{
				LoadMethods(type.Methods, state);
				LoadFields(type.Fields, state);
				LoadProperties(type.Properties, state);
				LoadEvents(type.Events, state);
				LoadTypes(type.NestedTypes, state);
			}
		}

		#endregion

		#region Method

		private void LoadMethods(MethodDeclarationCollection methods, LoadState state)
		{
			if (methods.Count == 0)
				return;

			var projectMethods = (state.ProjectModule != null) ? state.ProjectModule.Methods : null;

			if (projectMethods != null && projectMethods.Count > 0)
			{
				foreach (BuildMethod method in methods)
				{
					ProjectMethod projectMethod;
					projectMethods.TryGetValue(method, out projectMethod);
					LoadMethod(method, projectMethod, state);
				}
			}
			else
			{
				foreach (BuildMethod method in methods)
				{
					LoadMethod(method, null, state);
				}
			}
		}

		private void LoadMethod(BuildMethod method, ProjectMethod projectMethod, LoadState state)
		{
			// Backup
			bool obfuscateControlFlowBak = state.ObfuscateControlFlow;
			bool renameMembersBak = state.RenameMembers;
			bool encryptILBak = state.EncryptIL;
			bool obfuscateStringsBak = state.ObfuscateStrings;
			bool removeUnusedMembersBak = state.RemoveUnusedMembers;
			bool sealTypesBak = state.SealTypes;
			bool devirtualizeMethodsBak = state.DevirtualizeMethods;

			// Load ObfuscationAttribute
			foreach (var attribute in CA.ObfuscationAttribute.FindAll(method.CustomAttributes))
			{
				if (!state.IgnoreObfuscationAttribute)
				{
					LoadObfuscationAttribute(state, attribute.Feature, attribute.Exclude);
				}

				if (attribute.StripAfterObfuscation)
				{
					method.StripObfuscationAttribute = true;
					state.StripObfuscationAttributeExists = true;
				}
			}

			// Mark
			var methodState = new MethodLoadState(method, projectMethod, state);

			if (methodState.CanObfuscateControlFlow())
			{
				method.ObfuscateControlFlow = true;
			}

			if (methodState.CanRename())
			{
				method.Rename = true;

				if (projectMethod != null)
				{
					if (projectMethod.NameChanged)
					{
						method.NameChanged = true;
						method.NewName = projectMethod.Name;
					}
				}
			}

			if (methodState.CanEncryptIL())
			{
				method.EncryptIL = true;
			}

			if (methodState.CanObfuscateStrings())
			{
				method.ObfuscateStrings = true;
			}

			if (methodState.CanRemoveUnused())
			{
				method.Strip = true;
			}

			if (methodState.CanDevirtualizeMethod())
			{
				method.DevirtualizeMethod = true;
			}

			// Restore
			state.ObfuscateControlFlow = obfuscateControlFlowBak;
			state.RenameMembers = renameMembersBak;
			state.EncryptIL = encryptILBak;
			state.ObfuscateStrings = obfuscateStringsBak;
			state.RemoveUnusedMembers = removeUnusedMembersBak;
			state.SealTypes = sealTypesBak;
			state.DevirtualizeMethods = devirtualizeMethodsBak;
		}

		#endregion

		#region Field

		private void LoadFields(FieldDeclarationCollection fields, LoadState state)
		{
			if (fields.Count == 0)
				return;

			var projectFields = (state.ProjectModule != null) ? state.ProjectModule.Fields : null;

			if (projectFields != null && projectFields.Count > 0)
			{
				foreach (BuildField field in fields)
				{
					ProjectField projectField;
					projectFields.TryGetValue(field, out projectField);
					LoadField(field, projectField, state);
				}
			}
			else
			{
				foreach (BuildField field in fields)
				{
					LoadField(field, null, state);
				}
			}
		}

		private void LoadField(BuildField field, ProjectField projectField, LoadState state)
		{
			// Backup
			bool obfuscateControlFlowBak = state.ObfuscateControlFlow;
			bool renameMembersBak = state.RenameMembers;
			bool encryptILBak = state.EncryptIL;
			bool obfuscateStringsBak = state.ObfuscateStrings;
			bool removeUnusedMembersBak = state.RemoveUnusedMembers;
			bool sealTypesBak = state.SealTypes;
			bool devirtualizeMethodsBak = state.DevirtualizeMethods;

			// Load ObfuscationAttribute
			foreach (var attribute in CA.ObfuscationAttribute.FindAll(field.CustomAttributes))
			{
				if (!state.IgnoreObfuscationAttribute)
				{
					LoadObfuscationAttribute(state, attribute.Feature, attribute.Exclude);
				}

				if (attribute.StripAfterObfuscation)
				{
					field.StripObfuscationAttribute = true;
					state.StripObfuscationAttributeExists = true;
				}
			}

			// Mark
			var fieldState = new FieldLoadState(field, projectField, state);

			if (fieldState.CanRename())
			{
				field.Rename = true;

				if (projectField != null)
				{
					if (projectField.NameChanged)
					{
						field.NameChanged = true;
						field.NewName = projectField.Name;
					}
				}
			}

			if (fieldState.CanObfuscateStrings())
			{
				field.ObfuscateStrings = true;
			}

			if (fieldState.CanRemoveUnused())
			{
				field.Strip = true;
			}

			// Restore
			state.ObfuscateControlFlow = obfuscateControlFlowBak;
			state.RenameMembers = renameMembersBak;
			state.EncryptIL = encryptILBak;
			state.ObfuscateStrings = obfuscateStringsBak;
			state.RemoveUnusedMembers = removeUnusedMembersBak;
			state.SealTypes = sealTypesBak;
			state.DevirtualizeMethods = devirtualizeMethodsBak;
		}

		#endregion

		#region Property

		private void LoadProperties(PropertyDeclarationCollection properties, LoadState state)
		{
			if (properties.Count == 0)
				return;

			var projectProperties = (state.ProjectModule != null) ? state.ProjectModule.Properties : null;

			if (projectProperties != null && projectProperties.Count > 0)
			{
				foreach (BuildProperty property in properties)
				{
					ProjectProperty projectProperty;
					projectProperties.TryGetValue(property, out projectProperty);
					LoadProperty(property, projectProperty, state);
				}
			}
			else
			{
				foreach (BuildProperty property in properties)
				{
					LoadProperty(property, null, state);
				}
			}
		}

		private void LoadProperty(BuildProperty property, ProjectProperty projectProperty, LoadState state)
		{
			// Backup
			bool obfuscateControlFlowBak = state.ObfuscateControlFlow;
			bool renameMembersBak = state.RenameMembers;
			bool encryptILBak = state.EncryptIL;
			bool obfuscateStringsBak = state.ObfuscateStrings;
			bool removeUnusedMembersBak = state.RemoveUnusedMembers;
			bool sealTypesBak = state.SealTypes;
			bool devirtualizeMethodsBak = state.DevirtualizeMethods;

			// Load ObfuscationAttribute
			foreach (var attribute in CA.ObfuscationAttribute.FindAll(property.CustomAttributes))
			{
				if (!state.IgnoreObfuscationAttribute)
				{
					LoadObfuscationAttribute(state, attribute.Feature, attribute.Exclude);
				}

				if (attribute.StripAfterObfuscation)
				{
					property.StripObfuscationAttribute = true;
					state.StripObfuscationAttributeExists = true;
				}
			}

			// Mark
			var propertyState = new PropertyLoadState(property, projectProperty, state);

			if (propertyState.CanRename())
			{
				property.Rename = true;

				if (projectProperty != null)
				{
					if (projectProperty.NameChanged)
					{
						property.NameChanged = true;
						property.NewName = projectProperty.Name;
					}
				}
			}

			if (propertyState.CanRemoveUnused())
			{
				property.Strip = true;
			}

			// Restore
			state.ObfuscateControlFlow = obfuscateControlFlowBak;
			state.RenameMembers = renameMembersBak;
			state.EncryptIL = encryptILBak;
			state.ObfuscateStrings = obfuscateStringsBak;
			state.RemoveUnusedMembers = removeUnusedMembersBak;
			state.SealTypes = sealTypesBak;
			state.DevirtualizeMethods = devirtualizeMethodsBak;
		}

		#endregion

		#region Event

		private void LoadEvents(EventDeclarationCollection events, LoadState state)
		{
			if (events.Count == 0)
				return;

			var projectEvents = (state.ProjectModule != null) ? state.ProjectModule.Events : null;

			if (projectEvents != null && projectEvents.Count > 0)
			{
				foreach (BuildEvent e in events)
				{
					ProjectEvent projectEvent;
					projectEvents.TryGetValue(e, out projectEvent);
					LoadEvent(e, projectEvent, state);
				}
			}
			else
			{
				foreach (BuildEvent e in events)
				{
					LoadEvent(e, null, state);
				}
			}
		}

		private void LoadEvent(BuildEvent e, ProjectEvent projectEvent, LoadState state)
		{
			// Backup
			bool obfuscateControlFlowBak = state.ObfuscateControlFlow;
			bool renameMembersBak = state.RenameMembers;
			bool encryptILBak = state.EncryptIL;
			bool obfuscateStringsBak = state.ObfuscateStrings;
			bool removeUnusedMembersBak = state.RemoveUnusedMembers;
			bool sealTypesBak = state.SealTypes;
			bool devirtualizeMethodsBak = state.DevirtualizeMethods;

			// Load ObfuscationAttribute
			foreach (var attribute in CA.ObfuscationAttribute.FindAll(e.CustomAttributes))
			{
				if (!state.IgnoreObfuscationAttribute)
				{
					LoadObfuscationAttribute(state, attribute.Feature, attribute.Exclude);
				}

				if (attribute.StripAfterObfuscation)
				{
					e.StripObfuscationAttribute = true;
					state.StripObfuscationAttributeExists = true;
				}
			}

			// Mark
			var eventState = new EventLoadState(e, projectEvent, state);

			if (eventState.CanRename())
			{
				e.Rename = true;

				if (projectEvent != null)
				{
					if (projectEvent.NameChanged)
					{
						e.NameChanged = true;
						e.NewName = projectEvent.Name;
					}
				}
			}

			if (eventState.CanRemoveUnused())
			{
				e.Strip = true;
			}

			// Restore
			state.ObfuscateControlFlow = obfuscateControlFlowBak;
			state.RenameMembers = renameMembersBak;
			state.EncryptIL = encryptILBak;
			state.ObfuscateStrings = obfuscateStringsBak;
			state.RemoveUnusedMembers = removeUnusedMembersBak;
			state.SealTypes = sealTypesBak;
			state.DevirtualizeMethods = devirtualizeMethodsBak;
		}

		#endregion

		#region Resource

		private void LoadResources(ResourceCollection resources, LoadState state)
		{
			if (resources.Count == 0)
				return;

			var projectResources = state.ProjectAssembly.Resources;

			if (projectResources.Count > 0)
			{
				foreach (BuildResource resource in resources)
				{
					ProjectResource projectResource;
					projectResources.TryGetValue(resource.Name, out projectResource);
					LoadResource(resource, projectResource, state);
				}
			}
			else
			{
				foreach (BuildResource resource in resources)
				{
					LoadResource(resource, null, state);
				}
			}
		}

		private void LoadResource(BuildResource resource, ProjectResource projectResource, LoadState state)
		{
			if (projectResource != null)
			{
				if (state.ObfuscateResources)
				{
					resource.Obfuscate = projectResource.ObfuscateChanged ? projectResource.Obfuscate : true;
				}
			}
			else
			{
				if (state.ObfuscateResources)
				{
					resource.Obfuscate = true;
				}
			}
		}

		#endregion

		#region Nested types

		private class LoadState
		{
			internal bool ObfuscateControlFlow;
			internal bool ObfuscateControlFlowAtomic;
			internal bool CanObfuscateControlFlow;
			internal bool RenameMembers;
			internal bool RenamePublicTypes;
			internal bool RenamePublicMethods;
			internal bool RenamePublicFields;
			internal bool RenamePublicProperties;
			internal bool RenamePublicEvents;
			internal bool RenameEnumMembers;
			internal bool RenameSerializableTypes;
			internal bool RenameConfigurationTypes;
			internal bool CanRenameMembers;
			internal bool EncryptIL;
			internal bool EncryptILAtomic;
			internal bool CanEncryptIL;
			internal bool ObfuscateStrings;
			internal bool CanObfuscateStrings;
			internal bool ObfuscateResources;
			internal bool CanObfuscateResources;
			internal bool RemoveUnusedMembers;
			internal bool RemoveUnusedPublicMembers;
			internal bool CanRemoveUnusedMembers;
			internal bool SealTypes;
			internal bool SealPublicTypes;
			internal bool CanSealTypes;
			internal bool DevirtualizeMethods;
			internal bool DevirtualizePublicMethods;
			internal bool CanDevirtualizeMethods;
			internal bool SuppressILdasm;
			internal bool IgnoreObfuscationAttribute;
			internal bool StripObfuscationAttributeExists;
			internal ProjectAssembly ProjectAssembly;
			internal ProjectModule ProjectModule;
		}

		private class TypeLoadState
		{
			private bool? _isVisibleOutsideAssembly;
			private BuildType _type;
			private ProjectType _projectType;
			private LoadState _state;

			internal TypeLoadState(BuildType type, ProjectType projectType, LoadState state)
			{
				_type = type;
				_projectType = projectType;
				_state = state;
			}

			private bool IsVisibleOutsideAssembly
			{
				get
				{
					if (!_isVisibleOutsideAssembly.HasValue)
					{
						_isVisibleOutsideAssembly = _type.IsVisibleOutsideAssembly();
					}

					return _isVisibleOutsideAssembly.Value;
				}
			}

			internal bool CanRename()
			{
				if (!_state.CanRenameMembers)
					return false;

				if (_projectType != null && _projectType.RenameChanged)
				{
					if (!_projectType.Rename)
						return false;
				}
				else
				{
					if (!_state.RenameMembers)
						return false;
				}

				if (!_state.RenamePublicTypes && IsVisibleOutsideAssembly)
					return false;

				if (!MemberRenameHelper.CanRename(_type))
					return false;

				return true;
			}

			internal bool CanRemoveUnused()
			{
				if (!_state.CanRemoveUnusedMembers)
					return false;

				if (_projectType != null && _projectType.RemoveUnusedChanged)
				{
					if (!_projectType.RemoveUnused)
						return false;
				}
				else
				{
					if (!_state.RemoveUnusedMembers)
						return false;
				}

				if (!_state.RemoveUnusedPublicMembers && IsVisibleOutsideAssembly)
					return false;

				return true;
			}

			internal bool CanSealType()
			{
				if (!_state.CanSealTypes)
					return false;

				if (_projectType != null && _projectType.SealChanged)
				{
					if (!_projectType.Seal)
						return false;
				}
				else
				{
					if (!_state.SealTypes)
						return false;
				}

				if (!_state.SealPublicTypes && IsVisibleOutsideAssembly)
					return false;

				return true;
			}
		}

		private class MethodLoadState
		{
			private bool _methodBodyLoaded;
			private bool? _isAtomicBody;
			private bool? _isVisibleOutsideAssembly;
			private BuildMethod _method;
			private MethodBody _methodBody;
			private ProjectMethod _projectMethod;
			private LoadState _state;

			internal MethodLoadState(BuildMethod method, ProjectMethod projectMethod, LoadState state)
			{
				_method = method;
				_projectMethod = projectMethod;
				_state = state;
			}

			private bool IsVisibleOutsideAssembly
			{
				get
				{
					if (!_isVisibleOutsideAssembly.HasValue)
					{
						_isVisibleOutsideAssembly = _method.IsVisibleOutsideAssembly();
					}

					return _isVisibleOutsideAssembly.Value;
				}
			}

			private bool IsAtomicBody
			{
				get
				{
					if (!_isAtomicBody.HasValue)
					{
						var methodBody = MethodBody;
						if (methodBody != null)
						{
							_isAtomicBody = ControlFlowHelper.IsAtomic(methodBody);
						}
						else
						{
							_isAtomicBody = false;
						}
					}

					return _isAtomicBody.Value;
				}
			}

			private MethodBody MethodBody
			{
				get
				{
					if (!_methodBodyLoaded)
					{
						if (MethodBody.IsValid(_method))
						{
							_methodBody = MethodBody.Load(_method);
						}

						_methodBodyLoaded = true;
					}

					return _methodBody;
				}
			}

			internal bool CanObfuscateControlFlow()
			{
				if (!_state.CanObfuscateControlFlow)
					return false;

				if (_projectMethod != null && _projectMethod.ObfuscateControlFlowChanged)
				{
					if (!_projectMethod.ObfuscateControlFlow)
						return false;
				}
				else
				{
					if (!_state.ObfuscateControlFlow)
						return false;
				}

				if (!_state.ObfuscateControlFlowAtomic && IsAtomicBody)
					return false;

				return true;
			}

			internal bool CanRename()
			{
				if (!_state.CanRenameMembers)
					return false;

				if (_projectMethod != null && _projectMethod.RenameChanged)
				{
					if (!_projectMethod.Rename)
						return false;
				}
				else
				{
					if (!_state.RenameMembers)
						return false;
				}

				if (!_state.RenamePublicMethods && IsVisibleOutsideAssembly)
					return false;

				if (!MemberRenameHelper.CanRename(_method))
					return false;

				return true;
			}

			internal bool CanEncryptIL()
			{
				if (!_state.CanEncryptIL)
					return false;

				if (_projectMethod != null && _projectMethod.EncryptILChanged)
				{
					if (!_projectMethod.EncryptIL)
						return false;
				}
				else
				{
					if (!_state.EncryptIL)
						return false;
				}

				if (!_method.Module.IsPrimeModule)
					return false;

				if (!_state.EncryptILAtomic && IsAtomicBody)
					return false;

				if (!ILCryptoHelper.CanEncrypt(_method, MethodBody))
					return false;

				return true;
			}

			internal bool CanObfuscateStrings()
			{
				if (!_state.CanObfuscateStrings)
					return false;

				if (_projectMethod != null && _projectMethod.ObfuscateStringsChanged)
				{
					if (!_projectMethod.ObfuscateStrings)
						return false;
				}
				else
				{
					if (!_state.ObfuscateStrings)
						return false;
				}

				if (!_method.Module.IsPrimeModule)
					return false;

				return true;
			}

			internal bool CanRemoveUnused()
			{
				if (!_state.CanRemoveUnusedMembers)
					return false;

				if (_projectMethod != null && _projectMethod.RemoveUnusedChanged)
				{
					if (!_projectMethod.RemoveUnused)
						return false;
				}
				else
				{
					if (!_state.RemoveUnusedMembers)
						return false;
				}

				if (!_state.RemoveUnusedPublicMembers && IsVisibleOutsideAssembly)
					return false;

				return true;
			}

			internal bool CanDevirtualizeMethod()
			{
				if (!_state.CanDevirtualizeMethods)
					return false;

				if (_projectMethod != null && _projectMethod.DevirtualizeChanged)
				{
					if (!_projectMethod.Devirtualize)
						return false;
				}
				else
				{
					if (!_state.DevirtualizeMethods)
						return false;
				}

				if (!_state.DevirtualizePublicMethods && IsVisibleOutsideAssembly)
					return false;

				return true;
			}
		}

		private class FieldLoadState
		{
			private bool? _isVisibleOutsideAssembly;
			private bool? _isEnumType;
			private bool? _isEnumMember;
			private BuildField _field;
			private ProjectField _projectField;
			private LoadState _state;

			internal FieldLoadState(BuildField field, ProjectField projectField, LoadState state)
			{
				_field = field;
				_projectField = projectField;
				_state = state;
			}

			private bool IsVisibleOutsideAssembly
			{
				get
				{
					if (!_isVisibleOutsideAssembly.HasValue)
					{
						_isVisibleOutsideAssembly = GetIsVisibleOutsideAssembly(_field);
					}

					return _isVisibleOutsideAssembly.Value;
				}
			}

			private bool IsEnumType
			{
				get
				{
					if (!_isEnumType.HasValue)
					{
						_isEnumType = GetIsEnumType();
					}

					return _isEnumType.Value;
				}
			}

			private bool IsEnumMember
			{
				get
				{
					if (!_isEnumMember.HasValue)
					{
						_isEnumMember = GetIsEnumMember();
					}

					return _isEnumMember.Value;
				}
			}

			internal bool CanRename()
			{
				if (!_state.CanRenameMembers)
					return false;

				if (_projectField != null && _projectField.RenameChanged)
				{
					if (!_projectField.Rename)
						return false;
				}
				else
				{
					if (!_state.RenameMembers)
						return false;
				}

				if (!_state.RenamePublicFields && IsVisibleOutsideAssembly)
					return false;

				if (!_state.RenameEnumMembers && IsEnumMember)
					return false;

				return true;
			}

			internal bool CanObfuscateStrings()
			{
				if (!_state.CanObfuscateStrings)
					return false;

				if (!_state.ObfuscateStrings)
					return false;

				if (!_field.Module.IsPrimeModule)
					return false;

				return true;
			}

			internal bool CanRemoveUnused()
			{
				if (!_state.CanRemoveUnusedMembers)
					return false;

				if (_projectField != null && _projectField.RemoveUnusedChanged)
				{
					if (!_projectField.RemoveUnused)
						return false;
				}
				else
				{
					if (!_state.RemoveUnusedMembers)
						return false;
				}

				if (!_state.RemoveUnusedPublicMembers && IsVisibleOutsideAssembly)
					return false;

				if (IsEnumType)
					return false;

				return true;
			}

			private bool GetIsVisibleOutsideAssembly(IField field)
			{
				switch (field.Visibility)
				{
					case FieldVisibilityFlags.Public:
						return true;

					case FieldVisibilityFlags.Family:
					case FieldVisibilityFlags.FamOrAssem:
						return !field.Owner.IsSealed;
				}

				return false;
			}

			private bool GetIsEnumType()
			{
				var ownerType = _field.GetOwnerType();
				var baseType = ownerType.BaseType;
				if (baseType == null)
					return false;

				if (!baseType.Equals(_field.Module, "Enum", "System", "mscorlib"))
					return false;

				return true;
			}

			private bool GetIsEnumMember()
			{
				if (!IsEnumType)
					return false;

				var ownerType = _field.GetOwnerType();

				if (!SignatureComparer.IgnoreTypeOwner_IgnoreAssemblyStrongName.Equals(_field.FieldType, ownerType))
					return false;

				return true;
			}
		}

		private class PropertyLoadState
		{
			private bool? _isVisibleOutsideAssembly;
			private BuildProperty _property;
			private ProjectProperty _projectProperty;
			private LoadState _state;

			internal PropertyLoadState(BuildProperty property, ProjectProperty projectProperty, LoadState state)
			{
				_property = property;
				_projectProperty = projectProperty;
				_state = state;
			}

			private bool IsVisibleOutsideAssembly
			{
				get
				{
					if (!_isVisibleOutsideAssembly.HasValue)
					{
						_isVisibleOutsideAssembly = GetIsVisibleOutsideAssembly(_property);
					}

					return _isVisibleOutsideAssembly.Value;
				}
			}

			internal bool CanRename()
			{
				if (!_state.CanRenameMembers)
					return false;

				if (_projectProperty != null && _projectProperty.RenameChanged)
				{
					if (!_projectProperty.Rename)
						return false;
				}
				else
				{
					if (!_state.RenameMembers)
						return false;
				}

				if (!_state.RenamePublicProperties && IsVisibleOutsideAssembly)
					return false;

				return true;
			}

			internal bool CanRemoveUnused()
			{
				if (!_state.CanRemoveUnusedMembers)
					return false;

				if (_projectProperty != null && _projectProperty.RemoveUnusedChanged)
				{
					if (!_projectProperty.RemoveUnused)
						return false;
				}
				else
				{
					if (!_state.RemoveUnusedMembers)
						return false;
				}

				if (!_state.RemoveUnusedPublicMembers && IsVisibleOutsideAssembly)
					return false;

				return true;
			}

			private bool GetIsVisibleOutsideAssembly(IProperty property)
			{
				// Get
				if (property.GetMethod != null && property.GetMethod.Visibility == MethodVisibilityFlags.Public)
					return true;

				// Set
				if (property.SetMethod != null && property.SetMethod.Visibility == MethodVisibilityFlags.Public)
					return true;

				return false;
			}
		}

		private class EventLoadState
		{
			private bool? _isVisibleOutsideAssembly;
			private BuildEvent _event;
			private ProjectEvent _projectEvent;
			private LoadState _state;

			internal EventLoadState(BuildEvent e, ProjectEvent projectEvent, LoadState state)
			{
				_event = e;
				_projectEvent = projectEvent;
				_state = state;
			}

			private bool IsVisibleOutsideAssembly
			{
				get
				{
					if (!_isVisibleOutsideAssembly.HasValue)
					{
						_isVisibleOutsideAssembly = GetIsVisibleOutsideAssembly(_event);
					}

					return _isVisibleOutsideAssembly.Value;
				}
			}

			internal bool CanRename()
			{
				if (!_state.CanRenameMembers)
					return false;

				if (_projectEvent != null && _projectEvent.RenameChanged)
				{
					if (!_projectEvent.Rename)
						return false;
				}
				else
				{
					if (!_state.RenameMembers)
						return false;
				}

				if (!_state.RenamePublicEvents && IsVisibleOutsideAssembly)
					return false;

				return true;
			}

			internal bool CanRemoveUnused()
			{
				if (!_state.CanRemoveUnusedMembers)
					return false;

				if (_projectEvent != null && _projectEvent.RemoveUnusedChanged)
				{
					if (!_projectEvent.RemoveUnused)
						return false;
				}
				else
				{
					if (!_state.RemoveUnusedMembers)
						return false;
				}

				if (!_state.RemoveUnusedPublicMembers && IsVisibleOutsideAssembly)
					return false;

				return true;
			}

			private bool GetIsVisibleOutsideAssembly(IEvent e)
			{
				// Add
				if (e.AddMethod != null && e.AddMethod.Visibility == MethodVisibilityFlags.Public)
					return true;

				// Remove
				if (e.RemoveMethod != null && e.RemoveMethod.Visibility == MethodVisibilityFlags.Public)
					return true;

				// Invoke
				if (e.InvokeMethod != null && e.InvokeMethod.Visibility == MethodVisibilityFlags.Public)
					return true;

				return false;
			}
		}

		#endregion
	}
}
