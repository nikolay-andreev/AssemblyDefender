using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;
using AssemblyDefender.Common.Collections;

namespace AssemblyDefender
{
	public class BuildResource : Resource
	{
		#region Fields

		internal const int RowSize = 1;
		private int _stateOffset;
		private ModuleState _state;
		private IReadOnlyList<Resource> _satelliteResources;

		#endregion

		#region Ctors

		protected internal BuildResource(BuildModule module, int rid)
			: base(module, rid)
		{
			_state = module.State;
			_stateOffset = _state.GetResourceOffset(rid);
		}

		#endregion

		#region Properties
		
		public bool Obfuscate
		{
			get { return _state.GetTableBit(_stateOffset, 0); }
			set { _state.SetTableBit(_stateOffset, 0, value); }
		}

		public bool IsWpf
		{
			get { return _state.GetTableBit(_stateOffset, 2); }
			set { _state.SetTableBit(_stateOffset, 2, value); }
		}

		public bool HasWpfBaml
		{
			get { return _state.GetTableBit(_stateOffset, 3); }
			set { _state.SetTableBit(_stateOffset, 3, value); }
		}

		public IReadOnlyList<Resource> SatelliteResources
		{
			get
			{
				if (_satelliteResources == null)
				{
					LoadSatelliteResources();
				}

				return _satelliteResources;
			}
		}

		#endregion

		#region Methods

		private void LoadSatelliteResources()
		{
			var assembly = (BuildAssembly)Assembly;
			var satelliteAssemblies = assembly.SatelliteAssemblies;

			string resourceName = Name ?? "";
			string endsWith = ".resources";
			if (satelliteAssemblies.Count > 0 && resourceName.EndsWith(endsWith))
			{
				var resources = new List<Resource>(satelliteAssemblies.Count);

				string baseName = resourceName.Substring(0, resourceName.Length - endsWith.Length);

				foreach (var satelliteAssembly in satelliteAssemblies)
				{
					var resource = LoadSatelliteResource(baseName, satelliteAssembly);
					if (resource != null)
					{
						resources.Add(resource);
					}
				}

				_satelliteResources = new ReadOnlyList<Resource>(resources.ToArray());
			}
			else
			{
				_satelliteResources = ReadOnlyList<Resource>.Empty;
			}
		}

		private Resource LoadSatelliteResource(string baseName, Assembly satelliteAssembly)
		{
			string endsWith = string.Format("{0}.resources", satelliteAssembly.Culture ?? "");

			foreach (var resource in satelliteAssembly.Resources)
			{
				string name = baseName + "." + endsWith;
				if (resource.Name == name)
					return resource;
			}

			return null;
		}

		#endregion
	}
}
