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
	public class MethodOverrideCollection : SignatureCollection<MethodSignature>
	{
		#region Fields

		private MethodDeclaration _method;

		#endregion

		#region Ctors

		internal MethodOverrideCollection(MethodDeclaration method)
			: base(method)
		{
			_method = method;

			if (method.IsNew)
			{
				IsNew = true;
			}
			else
			{
				Load();
			}
		}

		#endregion

		#region Properties

		public MethodDeclaration Method
		{
			get { return _method; }
		}

		#endregion

		#region Methods

		public void CopyTo(MethodOverrideCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		private void Load()
		{
			var image = _module.Image;

			int[] implementations;
			image.GetMethodImplsByBody(_method.RID, out implementations);

			int count = implementations.Length;
			_list.Capacity = count;

			for (int i = 0; i < count; i++)
			{
				_list.Add(MethodSignature.Load(_module, MetadataToken.DecompressMethodDefOrRef(implementations[i])));
			}
		}

		#endregion
	}
}
