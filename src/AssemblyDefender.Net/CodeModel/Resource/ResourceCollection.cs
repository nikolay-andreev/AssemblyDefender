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
	public class ResourceCollection : MemberNodeCollection<Resource>
	{
		#region Ctors

		internal ResourceCollection(Module module)
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

		public Resource Find(string name, bool throwIfMissing = false)
		{
			foreach (var resource in this)
			{
				if (resource.Name == name)
					return resource;
			}

			if (throwIfMissing)
			{
				throw new CodeModelException(string.Format(SR.ResourceNotFound, name, Assembly.Name));
			}

			return null;
		}

		public void CopyTo(ResourceCollection copy)
		{
			foreach (var item in this)
			{
				item.CopyTo(copy.Add());
			}
		}

		protected override Resource GetItem(int rid)
		{
			return _module.GetResource(rid);
		}

		protected override Resource CreateItem()
		{
			return _module.CreateResource(0, 0);
		}

		private void Load()
		{
			var image = _module.Image;

			int count = image.GetManifestResourceCount();
			_list.Capacity = count;

			for (int rid = 1; rid <= count; rid++)
			{
				_list.Add(rid);
			}
		}

		#endregion
	}
}
