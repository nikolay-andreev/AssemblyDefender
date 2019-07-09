using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	public class FileReferenceCollection : SignatureCollection<FileReference>
	{
		#region Ctors

		internal FileReferenceCollection(Module module)
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

		public void CopyTo(FileReferenceCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		private void Load()
		{
			var image = _module.Image;

			int count = image.GetFileCount();
			_list.Capacity = count;

			for (int rid = 1; rid <= count; rid++)
			{
				_list.Add(FileReference.Load(_module, rid));
			}
		}

		#endregion
	}
}
