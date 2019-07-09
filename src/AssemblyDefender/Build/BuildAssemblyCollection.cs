using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	public class BuildAssemblyCollection : IReadOnlyList<BuildAssembly>, IEnumerable<BuildAssembly>
	{
		#region Fields

		private HashList<string> _files;
		private BuildAssemblyManager _assemblyManager;

		#endregion

		#region Ctors

		internal BuildAssemblyCollection(BuildAssemblyManager assemblyManager)
		{
			_assemblyManager = assemblyManager;
			_files = new HashList<string>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Properties

		public BuildAssembly this[int index]
		{
			get { return (BuildAssembly)_assemblyManager.LoadAssembly(_files[index], true); }
		}

		public BuildAssembly this[string filePath]
		{
			get { return (BuildAssembly)_assemblyManager.LoadAssembly(filePath, true); }
		}

		public int Count
		{
			get { return _files.Count; }
		}

		#endregion

		#region Methods

		public BuildAssembly Add(string filePath)
		{
			if (_files.Contains(filePath))
			{
				throw new BuildException(string.Format(SR.DuplicateAssemblyFile, filePath));
			}

			var assembly = _assemblyManager.AddBuildAssembly(filePath);

			_files.Add(filePath);

			return assembly;
		}

		public IEnumerator<BuildAssembly> GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Nested types

		private struct Enumerator : IEnumerator<BuildAssembly>
		{
			private int _index;
			private BuildAssemblyCollection _assemblies;

			internal Enumerator(BuildAssemblyCollection assemblies)
			{
				_assemblies = assemblies;
				_index = -1;
			}

			public BuildAssembly Current
			{
				get
				{
					if (_index < 0)
						return null;

					return _assemblies[_index];
				}
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public bool MoveNext()
			{
				if (_index + 1 == _assemblies.Count)
					return false;

				_index++;
				return true;
			}

			public void Reset()
			{
				_index = -1;
			}

			public void Dispose()
			{
			}
		}

		#endregion
	}
}
