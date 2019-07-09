using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Fusion;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class AssemblyManager : IEnumerable<Assembly>, IDisposable
	{
		#region Fields

		protected bool _resolveIgnoreGAC;
		protected bool _resolveIgnoreCodeBase;
		protected FileLoadMode _fileLoadMode;
		protected AssemblyCacheMode _cacheMode = AssemblyCacheMode.Type;
		protected HashList<string> _resolveSearchFolders = new HashList<string>(StringComparer.OrdinalIgnoreCase);
		protected HashList<ISignature> _signatureCache = new HashList<ISignature>(CacheSignatureComparer.Instance);
		protected Dictionary<string, Assembly> _assemblyByLocation = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
		protected static Dictionary<string, string> _gacAssemblyPathByAssemblyName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Ctors

		public AssemblyManager()
		{
		}

		#endregion

		#region Properties

		public bool ResolveIgnoreGAC
		{
			get { return _resolveIgnoreGAC; }
			set { _resolveIgnoreGAC = value; }
		}

		public bool ResolveIgnoreCodeBase
		{
			get { return _resolveIgnoreCodeBase; }
			set { _resolveIgnoreCodeBase = value; }
		}

		public FileLoadMode FileLoadMode
		{
			get { return _fileLoadMode; }
			set { _fileLoadMode = value; }
		}

		public AssemblyCacheMode CacheMode
		{
			get { return _cacheMode; }
			set { _cacheMode = value; }
		}

		#endregion

		#region Methods

		public Assembly[] GetAssemblies()
		{
			var assemblies = new Assembly[_assemblyByLocation.Count];
			_assemblyByLocation.Values.CopyTo(assemblies, 0);

			return assemblies;
		}

		private IAssembly GetAssembly(IAssemblySignature assemblySig, bool throwIfMissing = false)
		{
			var comparer = SignatureComparer.Default;
			foreach (var assembly in _assemblyByLocation.Values)
			{
				if (comparer.Equals(assembly, assemblySig))
					return assembly;
			}

			if (throwIfMissing)
			{
				throw new ResolveReferenceException(string.Format(SR.AssemblyNotFound, assemblySig.ToString()));
			}

			return null;
		}

		public Assembly GetAssembly(string location, bool throwIfMissing = false)
		{
			Assembly assembly;
			if (!_assemblyByLocation.TryGetValue(location, out assembly))
			{
				if (throwIfMissing)
				{
					throw new CodeModelException(string.Format(SR.AssemblyNotFound, location));
				}

				return null;
			}

			return assembly;
		}

		public Assembly LoadAssembly(string location, bool throwOnFailure = false)
		{
			Assembly assembly;
			if (!_assemblyByLocation.TryGetValue(location, out assembly))
			{
				try
				{
					assembly = CreateAssembly(location);
				}
				catch (Exception ex)
				{
					if (throwOnFailure)
					{
						throw new AssemblyLoadException(string.Format(SR.AssemblyLoadError, location), ex);
					}

					return null;
				}

				_assemblyByLocation.Add(location, assembly);
			}

			return assembly;
		}

		public virtual void UnloadAssembly(string location)
		{
			Assembly assembly;
			if (_assemblyByLocation.TryGetValue(location, out assembly))
			{
				UnloadAssembly(assembly);
			}
		}

		public virtual void UnloadAssembly(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (object.ReferenceEquals(assembly.AssemblyManager, this))
			{
				throw new InvalidOperationException();
			}

			assembly.Close();
			_assemblyByLocation.Remove(assembly.Location);
		}

		public virtual void UnloadAllAssemblies()
		{
			foreach (var assembly in _assemblyByLocation.Values)
			{
				assembly.Close();
			}

			_assemblyByLocation.Clear();
		}

		public void AddResolveFolder(string path)
		{
			_resolveSearchFolders.TryAdd(path);
		}

		public virtual void Dispose()
		{
			GC.SuppressFinalize(this);
			UnloadAllAssemblies();
			_signatureCache = null;
		}

		public IEnumerator<Assembly> GetEnumerator()
		{
			return _assemblyByLocation.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IType MakeFunctionPointerType(ICallSite callSite)
		{
			return (new ReferencedFunctionPointer(this, callSite)).Intern();
		}

		public IType MakeGenericParameterType(bool isMethod, int position)
		{
			return (new ReferencedGenericParameterType(this, isMethod, position)).Intern();
		}

		public ICallSite MakeCallSite(
			bool hasThis, bool explicitThis, int varArgIndex,
			MethodCallingConvention callConv, IType returnType, IReadOnlyList<IType> arguments)
		{
			return (new ReferencedCallSite(this, hasThis, explicitThis, varArgIndex, callConv, returnType, arguments)).Intern();
		}

		public virtual void InvalidateSignatures()
		{
			_signatureCache.Clear();

			foreach (var assembly in _assemblyByLocation.Values)
			{
				assembly.InvalidatedSignatures();
			}
		}

		protected virtual Assembly CreateAssembly(string location)
		{
			return new Assembly(this, location);
		}

		internal void InternNode<T>(ref T node)
			where T : ISignature
		{
			int index;
			if (!_signatureCache.TryAdd(node, out index))
			{
				node = (T)_signatureCache[index];
			}
		}

		#endregion

		#region Resolving

		protected bool ResolveGAC(IAssemblySignature assemblySig, out string foundPath)
		{
			string assemblyName;
			if (assemblySig.IsStrongNameSigned)
				assemblyName = assemblySig.ToString();
			else
				assemblyName = assemblySig.Name;

			return ResolveGAC(assemblyName, out foundPath);
		}

		protected bool ResolveGAC(string assemblyName, out string foundPath)
		{
			if (!_gacAssemblyPathByAssemblyName.TryGetValue(assemblyName, out foundPath))
			{
				lock (_gacAssemblyPathByAssemblyName)
				{
					if (!_gacAssemblyPathByAssemblyName.TryGetValue(assemblyName, out foundPath))
					{

						if (!AssemblyCache.FindAssemblyPath(assemblyName, out foundPath))
							return false;

						_gacAssemblyPathByAssemblyName.Add(assemblyName, foundPath);
					}
				}
			}

			return true;
		}

		#region Assembly

		public IAssembly Resolve(IAssemblySignature assemblySig, IModule context, bool throwOnFailure = false)
		{
			var assembly = ResolveAssembly(assemblySig, context);
			if (assembly == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(SR.AssemblyResolveError, assemblySig.ToReflectionString()));
				}

				return null;
			}

			return assembly;
		}

		public IReadOnlyList<IAssembly> Resolve(IReadOnlyList<IAssemblySignature> signatures, IModule context, bool throwOnFailure = false)
		{
			int count = signatures.Count;
			if (count == 0)
				return ReadOnlyList<IAssembly>.Empty;

			var assemblies = new List<IAssembly>(count);
			for (int i = 0; i < count; i++)
			{
				var assembly = Resolve(signatures[i], context, throwOnFailure);
				if (assembly != null)
				{
					assemblies.Add(assembly);
				}
			}

			return new ReadOnlyList<IAssembly>(assemblies.ToArray());
		}

		protected virtual IAssembly ResolveAssembly(IAssemblySignature assemblySig, IModule context)
		{
			return ResolveAssembly(assemblySig, context.Location);
		}

		protected virtual IAssembly ResolveAssembly(IAssemblySignature assemblySig, string codeBase)
		{
			var assembly = GetAssembly(assemblySig);
			if (assembly != null)
				return assembly;

			string foundPath;
			if (!ResolveAssemblyPath(assemblySig, codeBase, out foundPath))
				return null;

			return LoadAssembly(foundPath);
		}

		protected bool ResolveAssemblyPath(IAssemblySignature assemblySig, string codeBase, out string foundPath)
		{
			bool resolveFromGAC = (!_resolveIgnoreGAC && assemblySig.IsStrongNameSigned);
			if (resolveFromGAC)
			{
				if (ResolveGAC(assemblySig, out foundPath))
					return true;
			}

			if (!_resolveIgnoreCodeBase && !string.IsNullOrEmpty(codeBase))
			{
				if (ResolveAssemblyFromPath(Path.GetDirectoryName(codeBase), assemblySig.Name, out foundPath))
					return true;

				foreach (string searchPath in _resolveSearchFolders)
				{
					if (ResolveAssemblyFromPath(searchPath, assemblySig.Name, out foundPath))
						return true;
				}
			}

			if (resolveFromGAC)
			{
				if (ResolveGAC(assemblySig.Name, out foundPath))
					return true;
			}

			foundPath = null;
			return false;
		}

		protected bool ResolveAssemblyFromPath(string searchPath, string assemblyName, out string foundPath)
		{
			if (string.IsNullOrEmpty(searchPath))
			{
				foundPath = null;
				return false;
			}

			foundPath = Path.Combine(searchPath, assemblyName + CodeModelUtils.DllExtension);
			if (File.Exists(foundPath))
				return true;

			foundPath = Path.Combine(searchPath, assemblyName + CodeModelUtils.ExeExtension);
			if (File.Exists(foundPath))
				return true;

			return false;
		}

		#endregion

		#region Module

		public IModule Resolve(IModuleSignature moduleSig, IModule context, bool throwOnFailure = false)
		{
			var module = ResolveModule(moduleSig, context);
			if (module == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(SR.ModuleResolveError, moduleSig.ToString()));
				}

				return null;
			}

			return module;
		}

		public IReadOnlyList<IModule> Resolve(IReadOnlyList<IModuleSignature> signatures, IModule context, bool throwOnFailure = false)
		{
			int count = signatures.Count;
			if (count == 0)
				return ReadOnlyList<IModule>.Empty;

			var modules = new List<IModule>(count);
			for (int i = 0; i < count; i++)
			{
				var module = Resolve(signatures[i], context, throwOnFailure);
				if (module != null)
				{
					modules.Add(module);
				}
			}

			return new ReadOnlyList<IModule>(modules.ToArray());
		}

		protected IModule ResolveModule(IModuleSignature moduleSig, IModule context)
		{
			return context.Assembly.GetModule(moduleSig.Name);
		}

		#endregion

		#region Type

		public IType Resolve(ITypeSignature typeSig, ICodeNode context, bool throwOnFailure = false)
		{
			var type = ResolveType(typeSig, context.Module, context);
			if (type == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(SR.TypeResolveError, typeSig.ToReflectionString()));
				}

				return null;
			}

			return type;
		}

		public IReadOnlyList<IType> Resolve(IReadOnlyList<ITypeSignature> signatures, ICodeNode context, bool throwOnFailure = false)
		{
			int count = signatures.Count;
			if (count == 0)
				return ReadOnlyList<IType>.Empty;

			var types = new List<IType>(count);
			for (int i = 0; i < count; i++)
			{
				var type = Resolve(signatures[i], context, throwOnFailure);
				if (type != null)
				{
					types.Add(type);
				}
			}

			return new ReadOnlyList<IType>(types.ToArray());
		}

		protected IReadOnlyList<IType> ResolveTypes(IReadOnlyList<ITypeSignature> signatures, IModule context, ICodeNode genericContext)
		{
			int count = signatures.Count;
			if (count == 0)
				return ReadOnlyList<IType>.Empty;

			var types = new IType[count];
			for (int i = 0; i < count; i++)
			{
				var type = ResolveType(signatures[i], context, genericContext);
				if (type == null)
					return null;

				types[i] = type;
			}

			return new ReadOnlyList<IType>(types);
		}

		protected virtual IType ResolveType(ITypeSignature typeSig, IModule context, ICodeNode genericContext)
		{
			switch (typeSig.ElementCode)
			{
				case TypeElementCode.Array:
					{
						var elementType = ResolveType(typeSig.ElementType, context, genericContext);
						if (elementType == null)
							return null;

						return elementType.MakeArrayType(typeSig.ArrayDimensions);
					}

				case TypeElementCode.ByRef:
					{
						var elementType = ResolveType(typeSig.ElementType, context, genericContext);
						if (elementType == null)
							return null;

						return elementType.MakeByRefType();
					}

				case TypeElementCode.CustomModifier:
					{
						var elementType = ResolveType(typeSig.ElementType, context, genericContext);
						if (elementType == null)
							return null;

						CustomModifierType modifierType;
						var modifierSig = typeSig.GetCustomModifier(out modifierType);
						var modifier = ResolveType(modifierSig, context, genericContext);

						return elementType.MakeCustomModifierType(modifier, modifierType);
					}

				case TypeElementCode.FunctionPointer:
					{
						var callSite = ResolveCallSite(typeSig.GetFunctionPointer(), context, genericContext);
						return MakeFunctionPointerType(callSite);
					}

				case TypeElementCode.GenericParameter:
					{
						bool isMethod;
						int position;
						typeSig.GetGenericParameter(out isMethod, out position);

						if (genericContext != null)
						{
							var type = genericContext.GetGenericArgument(isMethod, position);
							if (type != null)
								return type;
						}

						return MakeGenericParameterType(isMethod, position);
					}

				case TypeElementCode.GenericType:
					{
						var declaringType = ResolveType(typeSig.DeclaringType, context, genericContext);
						if (declaringType == null)
							return null;

						var genericArgumentSigs = typeSig.GenericArguments;
						if (genericArgumentSigs.Count == 0)
							return declaringType;

						var genericArguments = ResolveTypes(genericArgumentSigs, context, genericContext);
						if (genericArguments == null)
							return null;

						return declaringType.MakeGenericType(genericArguments);
					}

				case TypeElementCode.Pinned:
					{
						var elementType = ResolveType(typeSig.ElementType, context, genericContext);
						if (elementType == null)
							return null;

						return elementType.MakePinnedType();
					}

				case TypeElementCode.Pointer:
					{
						var elementType = ResolveType(typeSig.ElementType, context, genericContext);
						if (elementType == null)
							return null;

						return elementType.MakePointerType();
					}

				case TypeElementCode.DeclaringType:
					{
						return ResolveDeclaringType(typeSig, context);
					}

				default:
					throw new NotImplementedException();
			}
		}

		protected virtual IType ResolveDeclaringType(ITypeSignature typeSig, IModule context)
		{
			var owner = typeSig.Owner;
			if (owner == null)
			{
				if (context.IsPrimeModule)
					return context.Assembly.GetTypeOrExportedType(typeSig.Name, typeSig.Namespace);
				else
					return context.GetType(typeSig.Name, typeSig.Namespace);
			}

			switch (owner.SignatureType)
			{
				case SignatureType.Assembly:
					{
						var assembly = ResolveAssembly((IAssemblySignature)owner, context.Module);
						if (assembly == null)
							return null;

						return assembly.GetTypeOrExportedType(typeSig.Name, typeSig.Namespace);
					}

				case SignatureType.Module:
					{
						var module = context.Assembly.GetModule(((IModuleSignature)owner).Name);
						if (module == null)
							return null;

						return module.GetType(typeSig.Name, typeSig.Namespace);
					}

				case SignatureType.Type:
					{
						var enclosingType = ResolveDeclaringType((ITypeSignature)owner, context);
						if (enclosingType == null)
							return null;

						return enclosingType.GetNestedType(typeSig.Name, typeSig.Namespace);
					}

				default:
					throw new NotImplementedException();
			}
		}

		#endregion

		#region Method

		public IMethod Resolve(IMethodSignature methodSig, ICodeNode context, bool polymorphic = false, bool throwOnFailure = false)
		{
			var method = ResolveMethod(methodSig, context.Module, context, polymorphic);
			if (method == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(SR.MethodResolveError, methodSig.ToString()));
				}

				return null;
			}

			return method;
		}

		public IReadOnlyList<IMethod> Resolve(IReadOnlyList<IMethodSignature> signatures, ICodeNode context, bool polymorphic = false, bool throwOnFailure = false)
		{
			int count = signatures.Count;
			if (count == 0)
				return ReadOnlyList<IMethod>.Empty;

			var methods = new List<IMethod>(count);
			for (int i = 0; i < count; i++)
			{
				var method = Resolve(signatures[i], context, throwOnFailure);
				if (method != null)
				{
					methods.Add(method);
				}
			}

			return new ReadOnlyList<IMethod>(methods.ToArray());
		}

		protected IMethod ResolveMethod(IMethodSignature methodSig, IModule context, ICodeNode genericContext, bool polymorphic)
		{
			var declaringMethodSig = methodSig.DeclaringMethod;
			if (declaringMethodSig == null)
				return null;

			var method = ResolveDeclaringMethod(declaringMethodSig, context, genericContext, polymorphic);
			if (method == null)
				return null;

			var genericArgumentSigs = methodSig.GenericArguments;
			if (genericArgumentSigs.Count > 0)
			{
				var genericArguments = new IType[genericArgumentSigs.Count];
				for (int i = 0; i < genericArguments.Length; i++)
				{
					var genericArgument = ResolveType(genericArgumentSigs[i], context, genericContext);
					if (genericArgument == null)
						return null;

					genericArguments[i] = genericArgument;
				}

				method = method.MakeGenericMethod(new ReadOnlyList<IType>(genericArguments));
			}

			return method;
		}

		protected IMethod ResolveDeclaringMethod(IMethodSignature methodSig, IModule context, ICodeNode genericContext, bool polymorphic)
		{
			var ownerType = ResolveType(methodSig.Owner, context, genericContext);
			if (ownerType == null)
				return null;

			var comparer = new ResolveSignatureComparer(context, ownerType.Module);

			// Find
			foreach (var method in ownerType.Methods)
			{
				if (comparer.Equals(methodSig, method.DeclaringMethod))
				{
					return method;
				}
			}

			// Find polymorphic
			if (polymorphic)
			{
				ownerType = ownerType.BaseType;

				while (ownerType != null)
				{
					foreach (var method in ownerType.Methods)
					{
						if (comparer.Equals(methodSig, method, method))
						{
							return method;
						}
					}

					ownerType = ownerType.BaseType;
				}
			}

			return null;
		}

		private ICallSite ResolveCallSite(IMethodSignature methodSig, IModule context, ICodeNode genericContext)
		{
			var returnType = ResolveType(methodSig.ReturnType, context, null);
			if (returnType == null)
				return null;

			var arguments = new IType[methodSig.GetArgumentCountNoVarArgs()];
			for (int i = 0; i < arguments.Length; i++)
			{
				var argumentType = ResolveType(methodSig.Arguments[i], context, null);
				if (argumentType == null)
					return null;

				arguments[i] = argumentType;
			}

			return MakeCallSite(
				methodSig.HasThis,
				methodSig.ExplicitThis,
				methodSig.VarArgIndex,
				methodSig.CallConv,
				returnType,
				ReadOnlyList<IType>.Create(arguments));
		}

		#endregion

		#region Field

		public IField Resolve(IFieldSignature fieldSig, ICodeNode context, bool throwOnFailure = false)
		{
			var field = ResolveField(fieldSig, context.Module, context);
			if (field == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(SR.FieldResolveError, fieldSig.ToString()));
				}

				return null;
			}

			return field;
		}

		public IReadOnlyList<IField> Resolve(IReadOnlyList<IFieldSignature> signatures, ICodeNode context, bool throwOnFailure = false)
		{
			int count = signatures.Count;
			if (count == 0)
				return ReadOnlyList<IField>.Empty;

			var fields = new List<IField>(count);
			for (int i = 0; i < count; i++)
			{
				var field = Resolve(signatures[i], context, throwOnFailure);
				if (field != null)
				{
					fields.Add(field);
				}
			}

			return new ReadOnlyList<IField>(fields.ToArray());
		}

		protected IField ResolveField(IFieldSignature fieldSig, IModule context, ICodeNode genericContext)
		{
			var ownerType = ResolveType(fieldSig.Owner, context, genericContext);
			if (ownerType == null)
				return null;

			var comparer = new ResolveSignatureComparer(context, ownerType.Module);

			// Find
			foreach (var field in ownerType.Fields)
			{
				if (comparer.Equals(fieldSig, field.DeclaringField))
				{
					return field;
				}
			}

			return null;
		}

		#endregion

		#region Property

		public IProperty Resolve(IPropertySignature propertySig, ICodeNode context, bool throwOnFailure = false)
		{
			var property = ResolveProperty(propertySig, context.Module, context);
			if (property == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(SR.PropertyResolveError, propertySig.ToString()));
				}

				return null;
			}

			return property;
		}

		public IReadOnlyList<IProperty> Resolve(IReadOnlyList<IPropertySignature> signatures, ICodeNode context, bool throwOnFailure = false)
		{
			int count = signatures.Count;
			if (count == 0)
				return ReadOnlyList<IProperty>.Empty;

			var properties = new List<IProperty>(count);
			for (int i = 0; i < count; i++)
			{
				var property = Resolve(signatures[i], context, throwOnFailure);
				if (property != null)
				{
					properties.Add(property);
				}
			}

			return new ReadOnlyList<IProperty>(properties.ToArray());
		}

		protected IProperty ResolveProperty(IPropertySignature propertySig, IModule context, ICodeNode genericContext)
		{
			var ownerType = ResolveType(propertySig.Owner, context, genericContext);
			if (ownerType == null)
				return null;

			var comparer = new ResolveSignatureComparer(context, ownerType.Module);

			// Find
			foreach (var property in ownerType.Properties)
			{
				if (comparer.Equals(propertySig, property.DeclaringProperty))
				{
					return property;
				}
			}

			return null;
		}

		#endregion

		#region Event

		public IEvent Resolve(IEventSignature eventSig, ICodeNode context, bool throwOnFailure = false)
		{
			var e = ResolveEvent(eventSig, context.Module, context);
			if (e == null)
			{
				if (throwOnFailure)
				{
					throw new ResolveReferenceException(string.Format(SR.EventResolveError, eventSig.ToString()));
				}

				return null;
			}

			return e;
		}

		public IReadOnlyList<IEvent> Resolve(IReadOnlyList<IEventSignature> signatures, ICodeNode context, bool throwOnFailure = false)
		{
			int count = signatures.Count;
			if (count == 0)
				return ReadOnlyList<IEvent>.Empty;

			var events = new List<IEvent>(count);
			for (int i = 0; i < count; i++)
			{
				var e = Resolve(signatures[i], context, throwOnFailure);
				if (e != null)
				{
					events.Add(e);
				}
			}

			return new ReadOnlyList<IEvent>(events.ToArray());
		}

		protected IEvent ResolveEvent(IEventSignature eventSig, IModule context, ICodeNode genericContext)
		{
			var ownerType = ResolveType(eventSig.Owner, context, genericContext);
			if (ownerType == null)
				return null;

			var comparer = new ResolveSignatureComparer(context, ownerType.Module);

			// Find
			foreach (var e in ownerType.Events)
			{
				if (comparer.Equals(eventSig, e.DeclaringEvent))
				{
					return e;
				}
			}

			return null;
		}

		#endregion

		#endregion

		#region Nested types

		/// <summary>
		/// Enforce comparision by calling equals directly.
		/// </summary>
		protected class CacheSignatureComparer : IEqualityComparer<ISignature>
		{
			internal static readonly CacheSignatureComparer Instance = new CacheSignatureComparer();

			bool IEqualityComparer<ISignature>.Equals(ISignature x, ISignature y)
			{
				switch (x.SignatureType)
				{
					case SignatureType.Type:
						return Equals(x as IType, y as IType);

					case SignatureType.Method:
						return Equals(x as IMethod, y as IMethod);

					case SignatureType.Field:
						return Equals(x as IField, y as IField);

					case SignatureType.Property:
						return Equals(x as IProperty, y as IProperty);

					case SignatureType.Event:
						return Equals(x as IEvent, y as IEvent);

					default:
						throw new InvalidOperationException();
				}
			}

			int IEqualityComparer<ISignature>.GetHashCode(ISignature obj)
			{
				return SignatureComparer.Default.GetHashCode(obj);
			}

			private bool Equals(IType x, IType y)
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
							if (!SignatureComparer.EqualsArrayDimensions(x.ArrayDimensions, y.ArrayDimensions))
								return false;

							if (!object.ReferenceEquals(x.ElementType, y.ElementType))
								return false;

							return true;
						}

					case TypeElementCode.ByRef:
						{
							if (!object.ReferenceEquals(x.ElementType, y.ElementType))
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

							if (!object.ReferenceEquals(x.ElementType, y.ElementType))
								return false;

							if (!object.ReferenceEquals(xModifier, yModifier))
								return false;

							return true;
						}

					case TypeElementCode.FunctionPointer:
						{
							if (!object.ReferenceEquals(x.GetFunctionPointer(), y.GetFunctionPointer()))
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
							if (!object.ReferenceEquals(x.DeclaringType, y.DeclaringType))
								return false;

							if (!Equals(x.GenericArguments, y.GenericArguments))
								return false;

							return true;
						}

					case TypeElementCode.Pinned:
						{
							if (!object.ReferenceEquals(x.ElementType, y.ElementType))
								return false;

							return true;
						}

					case TypeElementCode.Pointer:
						{
							if (!object.ReferenceEquals(x.ElementType, y.ElementType))
								return false;

							return true;
						}

					case TypeElementCode.DeclaringType:
						throw new InvalidOperationException();

					default:
						throw new NotImplementedException();
				}
			}

			private bool Equals(IMethod x, IMethod y)
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

				if (!object.ReferenceEquals(x.Owner, y.Owner))
					return false;

				if (!object.ReferenceEquals(x.ReturnType, y.ReturnType))
					return false;

				if (x.GenericParameters.Count != y.GenericParameters.Count)
					return false;

				if (!Equals(x.Parameters, y.Parameters))
					return false;

				if (!Equals(x.GenericArguments, y.GenericArguments))
					return false;

				return true;
			}

			private bool Equals(IField x, IField y)
			{
				if (x == y)
					return true;

				if (x == null || y == null)
					return false;

				if (x.Name != y.Name)
					return false;

				if (!object.ReferenceEquals(x.Owner, y.Owner))
					return false;

				if (!object.ReferenceEquals(x.FieldType, y.FieldType))
					return false;

				return true;
			}

			private bool Equals(IProperty x, IProperty y)
			{
				if (x == y)
					return true;

				if (x == null || y == null)
					return false;

				if (x.Name != y.Name)
					return false;

				if (!object.ReferenceEquals(x.Owner, y.Owner))
					return false;

				if (!object.ReferenceEquals(x.ReturnType, y.ReturnType))
					return false;

				if (!Equals(x.Parameters, y.Parameters))
					return false;

				return true;
			}

			private bool Equals(IEvent x, IEvent y)
			{
				if (x == y)
					return true;

				if (x == null || y == null)
					return false;

				if (x.Name != y.Name)
					return false;

				if (!object.ReferenceEquals(x.Owner, y.Owner))
					return false;

				if (!object.ReferenceEquals(x.EventType, y.EventType))
					return false;

				return true;
			}

			private bool Equals(IReadOnlyList<IType> x, IReadOnlyList<IType> y)
			{
				if (x.Count != y.Count)
					return false;

				for (int i = 0; i < x.Count; i++)
				{
					if (!object.ReferenceEquals(x[i], y[i]))
						return false;
				}

				return true;
			}

			private bool Equals(IReadOnlyList<IMethodParameter> x, IReadOnlyList<IMethodParameter> y)
			{
				if (x.Count != y.Count)
					return false;

				for (int i = 0; i < x.Count; i++)
				{
					if (!object.ReferenceEquals(x[i].Type, y[i].Type))
						return false;
				}

				return true;
			}
		}

		#endregion
	}
}
