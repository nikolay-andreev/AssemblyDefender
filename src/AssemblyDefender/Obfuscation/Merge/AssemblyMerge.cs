using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
    /// <summary>
    /// Merge multiple assemblies into one.
    /// </summary>
    public class AssemblyMerge
    {
        #region Fields

        private Assembly _assembly;
        private Random _random;
        private HashSet<AssemblyReference> _assemblyRefs;
        private HashSet<ModuleReference> _moduleRefs;
        private HashSet<FileReference> _fileRefs;
        private HashSet<TypeReference> _exportedTypes;
        private HashSet<string> _resourceNames;
        private HashSet<TypeReference> _typeRefs;
        private List<MethodReference> _startupMethods;
        private List<MergeAssemblyState> _mergedAssemblies;
        private List<MergeModuleState> _mergedModules;
        private Dictionary<string, MergeAssemblyState> _mergedAssemblyByName;
        private Dictionary<string, MergeModuleState> _mergedModuleByName;
        private Dictionary<TypeReference, Signature> _exportedTypeToOwner;

        #endregion

        #region Ctors

        public AssemblyMerge(Assembly assembly)
            : this(assembly, null)
        {
        }

        public AssemblyMerge(Assembly assembly, Random random)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            _assembly = assembly;
            _random = random ?? new Random();
            _assemblyRefs = new HashSet<AssemblyReference>(SignatureComparer.IgnoreAssemblyStrongName);
            _moduleRefs = new HashSet<ModuleReference>();
            _fileRefs = new HashSet<FileReference>();
            _exportedTypes = new HashSet<TypeReference>(SignatureComparer.IgnoreAssemblyStrongName);
            _resourceNames = new HashSet<string>();
            _typeRefs = new HashSet<TypeReference>(SignatureComparer.IgnoreTypeOwner);
            _startupMethods = new List<MethodReference>();
            _mergedAssemblies = new List<MergeAssemblyState>();
            _mergedModules = new List<MergeModuleState>();
            _mergedAssemblyByName = new Dictionary<string, MergeAssemblyState>();
            _mergedModuleByName = new Dictionary<string, MergeModuleState>();
            _exportedTypeToOwner = new Dictionary<TypeReference, Signature>(SignatureComparer.IgnoreTypeOwner);

            Load();
        }

        #endregion

        #region Properties

        public Assembly Assembly
        {
            get { return _assembly; }
        }

        #endregion

        #region Methods

        public void Add(Assembly assembly, MergeDuplicateBehavior duplicateBehavior = MergeDuplicateBehavior.Rename)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            if (_mergedAssemblyByName.ContainsKey(assembly.Name))
                return;

            var state = LoadAssembly(assembly, duplicateBehavior);
            _mergedAssemblies.Add(state);
            _mergedAssemblyByName.Add(assembly.Name, state);
        }

        public void Add(Module module, MergeDuplicateBehavior duplicateBehavior = MergeDuplicateBehavior.Rename)
        {
            if (module == null)
                throw new ArgumentNullException("module");

            if (module.IsPrimeModule)
                return;

            if (!object.ReferenceEquals(_assembly, module.Assembly))
                return;

            if (_mergedModuleByName.ContainsKey(module.Name))
                return;

            var state = LoadModule(module, duplicateBehavior);
            _mergedModules.Add(state);
            _mergedModuleByName.Add(module.Name, state);
        }

        public string[] GetAssemblyNames()
        {
            string[] names = new string[_mergedAssemblies.Count + 1];
            names[0] = _assembly.Name;

            for (int i = 1; i < names.Length; i++)
            {
                names[i] = _mergedAssemblies[i - 1].Assembly.Name;
            }

            return names;
        }

        public void Merge()
        {
            if (_mergedAssemblies.Count == 0 && _mergedModules.Count == 0)
                return;

            CollectMainReferences();

            // Relocate main assembly.
            {
                var relocator = new SignatureRelocator(this, null);
                relocator.Build(_assembly.Module);
            }

            // Merge modules.
            foreach (var mergedModule in _mergedModules)
            {
                MergeModule(mergedModule);
            }

            // Merge assemblies.
            foreach (var mergedAssembly in _mergedAssemblies)
            {
                MergeAssembly(mergedAssembly);
            }

            AddMainReferences();

            CreateStartupMethods();

            UnloadMergedModules();
        }

        private void MergeAssembly(MergeAssemblyState state)
        {
            var relocator = new SignatureRelocator(this, state.Modules[0]);

            // Resources
            foreach (var resource in state.Resources)
            {
                var mergedResource = _assembly.Resources.Add();
                resource.CopyTo(mergedResource);
                relocator.Build(mergedResource);
            }

            MergeModuleReferences(state);

            // Modules
            foreach (var moduleState in state.Modules)
            {
                MergeModule(moduleState);
            }
        }

        private void MergeModule(MergeModuleState state)
        {
            var module = _assembly.Module;
            var mergedModule = state.Module;
            var relocator = new SignatureRelocator(this, state);

            // Types
            foreach (var type in state.Types)
            {
                var mergedType = module.Types.Add();
                type.CopyTo(mergedType);
                relocator.Build(mergedType);

                bool isGlobal = mergedType.IsGlobal();

                var typeRef = mergedType.ToReference(mergedType.Module);

                string newName;
                if (state.RenamedTypeNames.TryGetValue(typeRef, out newName))
                {
                    mergedType.Name = newName;
                }

                if (isGlobal)
                {
                    MergeGlobalType(mergedType);
                }
            }

            // Assembly references
            foreach (var assemblyRef in mergedModule.AssemblyReferences)
            {
                if (_assembly.Name == assemblyRef.Name)
                    continue;

                if (_mergedAssemblyByName.ContainsKey(assemblyRef.Name))
                    continue;

                if (!_assemblyRefs.Contains(assemblyRef))
                    _assemblyRefs.Add(assemblyRef);
            }

            // Exproted types
            foreach (var exportedType in mergedModule.ExportedTypes)
            {
                var enclosingExportedType = (TypeReference)exportedType.GetOutermostType();
                if (!_exportedTypeToOwner.ContainsKey(enclosingExportedType))
                    continue;

                if (!_exportedTypes.Contains(exportedType))
                    _exportedTypes.Add(exportedType);
            }
        }

        private void MergeGlobalType(TypeDeclaration type)
        {
            type.BaseType = TypeReference.GetPrimitiveType(PrimitiveTypeCode.Object, _assembly);

            // If .cctor method exists - rename and add to startup methods.
            var cctorMethod = type.Methods.FindStaticConstructor();
            if (cctorMethod != null)
            {
                cctorMethod.Name = _random.NextString(12);
                cctorMethod.Visibility = MethodVisibilityFlags.Assembly;
                cctorMethod.IsStatic = true;
                cctorMethod.IsHideBySig = true;
                cctorMethod.IsRuntimeSpecialName = false;
                cctorMethod.CodeType = MethodCodeTypeFlags.CIL;

                // Add to startup methods.
                _startupMethods.Add(cctorMethod.ToReference(cctorMethod.Module));
            }
        }

        private void MergeModuleReferences(MergeAssemblyState state)
        {
            var moduleState = state.Modules[0];
            var mergedModule = moduleState.Module;

            foreach (var moduleRef in mergedModule.ModuleReferences)
            {
                if (state.ModuleByName.ContainsKey(moduleRef.Name))
                    continue;

                if (!_moduleRefs.Contains(moduleRef))
                    _moduleRefs.Add(moduleRef);
            }
        }

        private void CollectMainReferences()
        {
            var module = _assembly.Module;

            // Assembly references.
            {
                foreach (var assemblyRef in module.AssemblyReferences)
                {
                    if (_mergedAssemblyByName.ContainsKey(assemblyRef.Name))
                        continue;

                    if (!_assemblyRefs.Contains(assemblyRef))
                        _assemblyRefs.Add(assemblyRef);
                }

                module.AssemblyReferences.Clear();
            }

            // Module references.
            {
                foreach (var moduleRef in module.ModuleReferences)
                {
                    if (_mergedModuleByName.ContainsKey(moduleRef.Name))
                        continue;

                    if (!_moduleRefs.Contains(moduleRef))
                        _moduleRefs.Add(moduleRef);
                }

                module.ModuleReferences.Clear();
            }

            // File references.
            {
                foreach (var fileRef in module.Files)
                {
                    if (_mergedModuleByName.ContainsKey(fileRef.Name))
                        continue;

                    if (!_fileRefs.Contains(fileRef))
                        _fileRefs.Add(fileRef);
                }

                module.Files.Clear();
            }

            // Exported types.
            {
                foreach (var exportedType in module.ExportedTypes)
                {
                    var enclosingExportedType = (TypeReference)exportedType.GetOutermostType();
                    if (!_exportedTypeToOwner.ContainsKey(enclosingExportedType))
                        continue;

                    if (!_exportedTypes.Contains(exportedType))
                        _exportedTypes.Add(exportedType);
                }

                module.ExportedTypes.Clear();
            }
        }

        private void AddMainReferences()
        {
            var module = _assembly.Module;

            foreach (var assemblyRef in _assemblyRefs)
            {
                module.AssemblyReferences.Add(assemblyRef);
            }

            foreach (var moduleRef in _moduleRefs)
            {
                module.ModuleReferences.Add(moduleRef);
            }

            foreach (var fileRef in _fileRefs)
            {
                module.Files.Add(fileRef);
            }

            foreach (var exportedType in _exportedTypes)
            {
                module.ExportedTypes.Add(exportedType);
            }
        }

        private void CreateStartupMethods()
        {
            if (_startupMethods.Count == 0)
                return;

            var globalType = _assembly.Module.Types.GetOrCreateGlobal();

            var method = globalType.Methods.GetOrCreateStaticConstructor();

            if (!MethodBody.IsValid(method))
                return;

            var methodBody = MethodBody.Load(method);

            if (methodBody.MaxStackSize < 2)
                methodBody.MaxStackSize = 2;

            var instructions = methodBody.Instructions;
            for (int i = 0; i < _startupMethods.Count; i++)
            {
                instructions.Insert(i, new Instruction(OpCodes.Call, _startupMethods[i]));
            }

            methodBody.Build(method);
        }

        private void UnloadMergedModules()
        {
            for (int i = _assembly.Modules.Count - 1; i > 0; i--)
            {
                var module = _assembly.Modules[i];
                if (_mergedModuleByName.ContainsKey(module.Name))
                    _assembly.Modules.RemoveAt(i);
            }
        }

        #endregion

        #region Loading

        private void Load()
        {
            var module = _assembly.Module;

            // Types
            foreach (var type in module.Types)
            {
                var typeRef = type.ToReference();
                if (!_typeRefs.Contains(typeRef))
                    _typeRefs.Add(typeRef);
            }

            // Resources
            foreach (var resource in _assembly.Resources)
            {
                if (!_resourceNames.Contains(resource.Name))
                    _resourceNames.Add(resource.Name);
            }

            // Exported types
            for (int i = 0; i < _assembly.ExportedTypes.Count; i++)
            {
                var exportedType = (TypeReference)_assembly.ExportedTypes[i].GetOutermostType();

                if (!_exportedTypeToOwner.ContainsKey(exportedType))
                    _exportedTypeToOwner.Add(exportedType, exportedType.Owner);

                if (!_typeRefs.Contains(exportedType))
                    _typeRefs.Add(exportedType);
            }
        }

        private MergeAssemblyState LoadAssembly(Assembly assembly, MergeDuplicateBehavior duplicateBehavior)
        {
            var state = new MergeAssemblyState();
            state.Assembly = assembly;
            state.DuplicateBehavior = duplicateBehavior;

            LoadModules(assembly.Modules, state);
            LoadResources(assembly.Resources, state);
            LoadExportedTypes(assembly.ExportedTypes, state);

            return state;
        }

        private void LoadModules(ModuleCollection modules, MergeAssemblyState state)
        {
            foreach (var module in modules)
            {
                var image = module.Image;
                if (image != null && !image.IsILOnly)
                    continue;

                if (state.ModuleByName.ContainsKey(module.Name))
                    continue;

                var moduleState = LoadModule(module, state.DuplicateBehavior);
                state.Modules.Add(moduleState);
                state.ModuleByName.Add(module.Name, moduleState);
            }
        }

        private MergeModuleState LoadModule(Module module, MergeDuplicateBehavior duplicateBehavior)
        {
            var state = new MergeModuleState();
            state.Module = module;
            state.DuplicateBehavior = duplicateBehavior;

            LoadTypes(module.Types, state);

            return state;
        }

        private void LoadTypes(TypeDeclarationCollection types, MergeModuleState state)
        {
            foreach (var type in types)
            {
                LoadType(type, state);
            }
        }

        private void LoadType(TypeDeclaration type, MergeModuleState state)
        {
            if (type.IsGlobal())
            {
                LoadGlobalType(type, state);
                return;
            }

            var typeRef = type.ToReference();

            // Check existing type.
            if (!_typeRefs.Contains(typeRef))
            {
                // Type does not exists.
                _typeRefs.Add(typeRef);
                state.Types.Add(type);
                return;
            }

            // Check exported type.
            Signature exportedTypeOwner;
            if (_exportedTypeToOwner.TryGetValue(typeRef, out exportedTypeOwner))
            {
                // Type exists as exported type.
                switch (exportedTypeOwner.SignatureType)
                {
                    case SignatureType.Module:
                        {
                            var moduleRef = (ModuleReference)exportedTypeOwner;
                            if (state.Module.Name == moduleRef.Name)
                            {
                                _exportedTypeToOwner.Remove(typeRef);
                                state.Types.Add(type);
                                return;
                            }
                        }
                        break;

                    case SignatureType.Assembly:
                        {
                            var assemblyRef = (AssemblyReference)exportedTypeOwner;
                            if (state.Module.Assembly.Name == assemblyRef.Name)
                            {
                                _exportedTypeToOwner.Remove(typeRef);
                                state.Types.Add(type);
                                return;
                            }
                        }
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            // Type is duplicate.
            switch (state.DuplicateBehavior)
            {
                case MergeDuplicateBehavior.Rename:
                    {
                        int index = 1;

                        var newTypeRef = typeRef;
                        do
                        {
                            string newName = typeRef.Name + (++index).ToString();
                            newTypeRef = new TypeReference(newName, typeRef.Namespace);
                        }
                        while (_typeRefs.Contains(newTypeRef));

                        _typeRefs.Add(newTypeRef);
                        state.Types.Add(type);
                        state.RenamedTypeNames.Add(typeRef, newTypeRef.Name);
                    }
                    return;

                case MergeDuplicateBehavior.Skip:
                    return;

                case MergeDuplicateBehavior.Throw:
                    throw new AssemblyDefenderException(string.Format(SR.MergeDuplicatedType, type.FullName, type.Module.ToString()));

                default:
                    throw new InvalidOperationException();
            }
        }

        private void LoadGlobalType(TypeDeclaration type, MergeModuleState state)
        {
            if (!type.HasMembers())
                return;

            TypeReference typeRef;
            do
            {
                string newName = _random.NextString(12);
                typeRef = new TypeReference(newName);
            }
            while (_typeRefs.Contains(typeRef));

            _typeRefs.Add(typeRef);
            state.Types.Add(type);
            state.RenamedTypeNames.Add(type.ToReference(), typeRef.Name);
        }

        private void LoadResources(ResourceCollection resources, MergeAssemblyState state)
        {
            foreach (var resource in resources)
            {
                LoadResource(resource, state);
            }
        }

        private void LoadResource(Resource resource, MergeAssemblyState state)
        {
            if (_resourceNames.Contains(resource.Name))
            {
                switch (state.DuplicateBehavior)
                {
                    case MergeDuplicateBehavior.Rename: // Reaname is not supported on resources.
                    case MergeDuplicateBehavior.Skip:
                        return;

                    case MergeDuplicateBehavior.Throw:
                        throw new AssemblyDefenderException(string.Format(SR.MergeDuplicatedResource, resource.Name, state.Assembly.ToString()));

                    default:
                        throw new InvalidOperationException();
                }
            }

            _resourceNames.Add(resource.Name);
            state.Resources.Add(resource);
        }

        private void LoadExportedTypes(ExportedTypeCollection exportedTypes, MergeAssemblyState state)
        {
            foreach (var exportedType in exportedTypes)
            {
                LoadExportedType(exportedType, state);
            }
        }

        private void LoadExportedType(TypeReference exportedType, MergeAssemblyState state)
        {
            exportedType = (TypeReference)exportedType.GetOutermostType();

            if (_typeRefs.Contains(exportedType))
                return;

            if (_exportedTypeToOwner.ContainsKey(exportedType))
                return;

            _typeRefs.Add(exportedType);
            _exportedTypeToOwner.Add(exportedType, exportedType.Owner);
        }

        #endregion

        #region Nested types

        private class MergeAssemblyState
        {
            internal Assembly Assembly;
            internal MergeDuplicateBehavior DuplicateBehavior;
            internal List<Resource> Resources = new List<Resource>();
            internal List<MergeModuleState> Modules = new List<MergeModuleState>();
            internal Dictionary<string, MergeModuleState> ModuleByName = new Dictionary<string, MergeModuleState>();
        }

        private class MergeModuleState
        {
            internal Module Module;
            internal MergeDuplicateBehavior DuplicateBehavior;
            internal List<TypeDeclaration> Types = new List<TypeDeclaration>();
            internal Dictionary<TypeReference, string> RenamedTypeNames =
                new Dictionary<TypeReference, string>(SignatureComparer.IgnoreTypeOwner);
        }

        private class SignatureRelocator : SignatureBuilder
        {
            private AssemblyMerge _merge;
            private MergeModuleState _moduleState;

            internal SignatureRelocator(AssemblyMerge merge, MergeModuleState moduleState)
            {
                _merge = merge;
                _moduleState = moduleState;
            }

            public override void Build(Module module)
            {
                Build(module.Resources);
                Build(module.Types);
                Build(module.CustomAttributes);
            }

            public override bool Build(ref AssemblyReference assemblyRef)
            {
                if (_merge._assembly.Name == assemblyRef.Name)
                {
                    assemblyRef = null;
                    return true;
                }

                if (_merge._mergedAssemblyByName.ContainsKey(assemblyRef.Name))
                {
                    assemblyRef = null;
                    return true;
                }

                return false;
            }

            public override bool Build(ref ModuleReference moduleRef)
            {
                if (_merge._mergedModuleByName.ContainsKey(moduleRef.Name))
                {
                    moduleRef = null;
                    return true;
                }

                return false;
            }

            public override bool Build(ref TypeReference typeRef)
            {
                bool changed = false;

                string name = typeRef.Name;

                var owner = typeRef.Owner;
                if (owner != null)
                {
                    changed |= BuildTypeRef(typeRef, ref owner, ref name);
                }
                else
                {
                    changed |= BuildLocalTypeRef(typeRef, ref name);
                }

                if (!changed)
                    return false;

                typeRef = new TypeReference(name, typeRef.Namespace, owner, typeRef.IsValueType);
                return true;
            }

            private bool BuildTypeRef(TypeReference typeRef, ref Signature owner, ref string name)
            {
                switch (owner.SignatureType)
                {
                    case SignatureType.Assembly:
                        {
                            var assemblyRef = (AssemblyReference)owner;
                            if (!BuildTypeRef(typeRef, ref assemblyRef, ref name))
                                return false;

                            owner = assemblyRef;
                            return true;
                        }

                    case SignatureType.Module:
                        {
                            var moduleRef = (ModuleReference)owner;
                            if (!BuildTypeRef(typeRef, ref moduleRef, ref name))
                                return false;

                            owner = moduleRef;
                            return true;
                        }

                    case SignatureType.Type:
                        {
                            var typeSig = (TypeSignature)owner;
                            if (Build(ref typeSig))
                            {
                                owner = typeSig;
                                return true;
                            }

                            return false;
                        }

                    default:
                        throw new InvalidOperationException();
                }
            }

            private bool BuildTypeRef(TypeReference typeRef, ref AssemblyReference assemblyRef, ref string name)
            {
                if (_merge._assembly.Name == assemblyRef.Name)
                {
                    assemblyRef = null;
                    return true;
                }

                MergeAssemblyState assemblyState;
                if (_merge._mergedAssemblyByName.TryGetValue(assemblyRef.Name, out assemblyState))
                {
                    var moduleState = assemblyState.Modules[0];

                    string newName;
                    if (moduleState.RenamedTypeNames.TryGetValue(typeRef, out newName))
                    {
                        name = newName;
                    }

                    assemblyRef = null;
                    return true;
                }

                return false;
            }

            private bool BuildTypeRef(TypeReference typeRef, ref ModuleReference moduleRef, ref string name)
            {
                MergeModuleState moduleState = null;

                if (_moduleState != null)
                {
                    MergeAssemblyState assemblyState;
                    if (!_merge._mergedAssemblyByName.TryGetValue(_moduleState.Module.Assembly.Name, out assemblyState))
                        return false;

                    if (!assemblyState.ModuleByName.TryGetValue(moduleRef.Name, out moduleState))
                        return false;
                }
                else
                {
                    if (!_merge._mergedModuleByName.TryGetValue(moduleRef.Name, out moduleState))
                        return false;
                }

                string newName;
                if (moduleState.RenamedTypeNames.TryGetValue(typeRef, out newName))
                {
                    name = newName;
                }

                moduleRef = null;
                return true;
            }

            private bool BuildLocalTypeRef(TypeReference typeRef, ref string name)
            {
                if (_moduleState == null)
                    return false;

                bool changed = false;

                string newName;
                if (_moduleState.RenamedTypeNames.TryGetValue(typeRef, out newName))
                {
                    name = newName;
                    changed = true;
                }

                return changed;
            }
        }

        #endregion
    }
}
