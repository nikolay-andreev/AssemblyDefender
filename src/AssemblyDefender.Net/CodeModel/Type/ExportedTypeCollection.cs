using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class ExportedTypeCollection : SignatureCollection<TypeReference>
	{
		#region Ctors

		internal ExportedTypeCollection(Module module)
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

		public void CopyTo(ExportedTypeCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		private void Load()
		{
			var image = _module.Image;

			int count = image.GetExportedTypeCount();
			_list.Capacity = count;

			for (int rid = 1; rid <= count; rid++)
			{
				_list.Add(TypeReference.LoadExportedType(_module, rid));
			}
		}

		#endregion
	}
}
