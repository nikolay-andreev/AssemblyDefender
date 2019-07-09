using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	public class AssemblyReferenceCollection : SignatureCollection<AssemblyReference>
	{
		#region Ctors

		internal AssemblyReferenceCollection(Module module)
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

		public AssemblyReference Find(string assemblyName, bool? isStrongNameSigned)
		{
			foreach (var assemblyRef in this)
			{
				if (assemblyRef.Name != assemblyName)
					continue;

				if (isStrongNameSigned.HasValue && isStrongNameSigned.Value != assemblyRef.IsStrongNameSigned)
					continue;

				return assemblyRef;
			}

			return null;
		}

		public void CopyTo(AssemblyReferenceCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		private void Load()
		{
			var image = _module.Image;

			int count = image.GetAssemblyRefCount();
			_list.Capacity = count;

			for (int rid = 1; rid <= count; rid++)
			{
				_list.Add(AssemblyReference.LoadRef(_module, rid));
			}
		}

		#endregion
	}
}
