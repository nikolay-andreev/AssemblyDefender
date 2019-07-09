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
	public class PropertyParameterCollection : SignatureCollection<TypeSignature>
	{
		#region Fields

		private PropertyDeclaration _property;

		#endregion

		#region Ctors

		internal PropertyParameterCollection(PropertyDeclaration property)
			: base(property)
		{
			_property = property;

			if (property.IsNew)
			{
				IsNew = true;
			}
		}

		#endregion

		#region Properties

		public PropertyDeclaration Property
		{
			get { return _property; }
		}

		#endregion

		#region Methods

		public void CopyTo(PropertyParameterCollection copy)
		{
			foreach (var item in this)
			{
				copy.Add(item);
			}
		}

		internal void Load(IBinaryAccessor accessor, int count)
		{
			_list.Capacity = count;

			for (int i = 0; i < count; i++)
			{
				_list.Add(TypeSignature.Load(accessor, _module));
			}
		}

		#endregion
	}
}
