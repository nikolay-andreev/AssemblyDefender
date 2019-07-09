using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class ModuleCollection : CodeNode, IEnumerable<Module>, IReadOnlyList<IModule>
	{
		#region Fields

		private Assembly _assembly;
		private List<Module> _list = new List<Module>();

		#endregion

		#region Ctors

		internal ModuleCollection(Assembly assembly)
			: base(assembly)
		{
			_assembly = assembly;
			_list.Add(_module);

			if (_module.Image != null)
			{
				Load();
			}
			else
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		public Module this[int index]
		{
			get { return _list[index]; }
		}

		IModule IReadOnlyList<IModule>.this[int index]
		{
			get { return _list[index]; }
		}

		public int Count
		{
			get { return _list.Count; }
		}

		#endregion

		#region Methods

		public Module FindByName(string name, bool throwIfMissing = false)
		{
			foreach (var module in this)
			{
				if (0 == string.Compare(module.Name, name, true))
					return module;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.ModuleNotFound, name));
			}

			return null;
		}

		public Module FindByLocation(string location, bool throwIfMissing = false)
		{
			foreach (var module in this)
			{
				if (0 == string.Compare(module.Location, location, true))
					return module;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.ModuleNotFound, location));
			}

			return null;
		}

		public int IndexOf(Module item)
		{
			return _list.IndexOf(item);
		}

		public Module Add()
		{
			return Add(null);
		}

		public Module Add(string location)
		{
			var item = _assembly.CreateModule(location);
			_list.Add(item);
			OnChanged();

			return item;
		}

		public bool Remove(Module item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			int index = IndexOf(item);
			if (index < 0)
				return false;

			RemoveAt(index);
			return true;
		}

		public void RemoveAt(int index)
		{
			if (index <= 0)
				throw new ArgumentOutOfRangeException("index");

			var module = _list[index];
			module.OnDeleted();
			module.Close();

			_list.RemoveAt(index);
			OnChanged();
		}

		public void Clear()
		{
			if (_list.Count < 2)
				return;

			for (int i = _list.Count - 1; i > 1; i--)
			{
				var module = _list[i];
				module.OnDeleted();
				module.Close();
			}

			_list.RemoveRange(1, _list.Count - 1);
			OnChanged();
		}

		public IEnumerator<Module> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator<IModule> IEnumerable<IModule>.GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void Load()
		{
			if (string.IsNullOrEmpty(_assembly.Location))
				return;

			string searchFolder = Path.GetDirectoryName(_assembly.Location);
			if (string.IsNullOrEmpty(searchFolder))
				return;

			var assemblyManager = _assembly.AssemblyManager;

			var primeImage = _module.Image;

			int count = primeImage.GetModuleRefCount();
			for (int rid = 1; rid <= count; rid++)
			{
				ModuleRefRow row;
				primeImage.GetModuleRef(rid, out row);

				string name = primeImage.GetString(row.Name);
				if (name == null)
					continue;

				if (null != FindByName(name))
					continue;

				string filePath = Path.Combine(searchFolder, name);

				try
				{
					_list.Add(_assembly.CreateModule(filePath));
				}
				catch (Exception)
				{
				}
			}
		}

		#endregion
	}
}
