using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	public class ModuleReferenceCollection : SignatureCollection<ModuleReference>
	{
		#region Ctors

		internal ModuleReferenceCollection(Module module)
			: base(module)
		{
			if (module.IsNew)
			{
				IsNew = true;
			}
			else
			{
				Load();
			}
		}

		#endregion

		#region Methods

		public void CopyTo(ModuleReferenceCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		private void Load()
		{
			var image = _module.Image;

			int count = image.GetModuleRefCount();
			_list.Capacity = count;

			for (int rid = 1; rid <= count; rid++)
			{
				_list.Add(ModuleReference.LoadRef(_module, rid));
			}
		}

		#endregion
	}
}
