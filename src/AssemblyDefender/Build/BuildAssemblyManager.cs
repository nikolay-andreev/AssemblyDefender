using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	internal class BuildAssemblyManager : AssemblyManager
	{
		#region Fields

		private Builder _builder;
		private Dictionary<string, ModuleState> _stateByPath;
		private Dictionary<ResolveCacheKey, ICodeNode> _resolveCache;

		#endregion

		#region Ctors

		internal BuildAssemblyManager(Builder builder)
		{
			if (builder == null)
				throw new ArgumentNullException("builder");

			_builder = builder;
			_cacheMode = AssemblyCacheMode.Members;
			_fileLoadMode = PE.FileLoadMode.MemoryMappedFile;
			_stateByPath = new Dictionary<string, ModuleState>(StringComparer.OrdinalIgnoreCase);
			_resolveCache = new Dictionary<ResolveCacheKey, ICodeNode>(new ResolveCacheComparer());
		}

		#endregion

		#region Properties

		internal Builder Builder
		{
			get { return _builder; }
		}

		internal IEnumerable<ModuleState> BuildStates
		{
			get { return _stateByPath.Values; }
		}

		#endregion

		#region Methods

		internal void Scavenge()
		{
			foreach (var state in _stateByPath.Values)
			{
				state.Scavenge();
			}

			_resolveCache.Clear();

			UnloadAllAssemblies();
			InvalidateSignatures();
		}

		internal BuildAssembly AddBuildAssembly(string filePath)
		{
			if (_assemblyByLocation.ContainsKey(filePath))
			{
				throw new AssemblyLoadException(string.Format(SR.DuplicateAssemblyFile, filePath));
			}

			var assembly = LoadAssembly(filePath);
			if (!assembly.Module.Image.IsILOnly)
			{
				throw new NativeCodeException(string.Format(SR.BuildAssemblyILOnlyError, filePath));
			}

			_assemblyByLocation.Add(filePath, assembly);

			AddResolveFolder(Path.GetDirectoryName(filePath));

			return assembly;
		}

		internal ModuleState GetState(string filePath)
		{
			ModuleState state;
			if (!_stateByPath.TryGetValue(filePath, out state))
			{
				state = new ModuleState();
				_stateByPath.Add(filePath, state);
			}

			return state;
		}

		internal void RemoveState(string filePath)
		{
			_stateByPath.Remove(filePath);
		}

		protected override Assembly CreateAssembly(string filePath)
		{
			if (_stateByPath.ContainsKey(filePath))
			{
				return new BuildAssembly(this, filePath);
			}

			return base.CreateAssembly(filePath);
		}

		protected override IAssembly ResolveAssembly(IAssemblySignature assemblySig, IModule context)
		{
			var key = new ResolveCacheKey()
			{
				Signature = assemblySig,
				Context = context,
			};

			ICodeNode node;
			if (!_resolveCache.TryGetValue(key, out node))
			{
				node = base.ResolveAssembly(assemblySig, context);
				_resolveCache.Add(key, node);
			}

			return (IAssembly)node;
		}

		protected override IType ResolveDeclaringType(ITypeSignature typeSig, IModule context)
		{
			var key = new ResolveCacheKey()
			{
				Signature = typeSig,
				Context = context,
			};

			ICodeNode node;
			if (!_resolveCache.TryGetValue(key, out node))
			{
				node = base.ResolveDeclaringType(typeSig, context);
				_resolveCache.Add(key, node);
			}

			return (IType)node;
		}

		public override void Dispose()
		{
			foreach (var state in _stateByPath.Values)
			{
				TryToDeleteFile(state.BuildFilePath);
				TryToDeleteFile(state.StateFilePath);
			}

			_stateByPath = null;
			_resolveCache = null;

			base.Dispose();
		}

		private BuildAssembly LoadAssembly(string filePath)
		{
			try
			{
				return new BuildAssembly(this, filePath);
			}
			catch (Exception ex)
			{
				throw new AssemblyLoadException(string.Format(Net.SR.AssemblyLoadError, filePath), ex);
			}
		}

		private void TryToDeleteFile(string filePath)
		{
			if (filePath == null)
				return;

			try
			{
				if (File.Exists(filePath))
					File.Delete(filePath);
			}
			catch (IOException)
			{
			}
		}

		#endregion

		#region Nested types

		private struct ResolveCacheKey
		{
			internal ISignature Signature;
			internal IModule Context;
		}

		private class ResolveCacheComparer : IEqualityComparer<ResolveCacheKey>
		{
			bool IEqualityComparer<ResolveCacheKey>.Equals(ResolveCacheKey x, ResolveCacheKey y)
			{
				if (!object.ReferenceEquals(x.Context, y.Context))
					return false;

				if (!SignatureComparer.Default.Equals(x.Signature, y.Signature))
					return false;

				return true;
			}

			int IEqualityComparer<ResolveCacheKey>.GetHashCode(ResolveCacheKey obj)
			{
				return SignatureComparer.Default.GetHashCode(obj.Signature) ^ obj.Context.GetHashCode();
			}
		}

		#endregion
	}
}
