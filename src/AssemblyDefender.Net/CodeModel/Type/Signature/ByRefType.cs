using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class ByRefType : TypeSignature
	{
		#region Fields

		private TypeSignature _elementType;

		#endregion

		#region Ctors

		private ByRefType()
		{
		}

		public ByRefType(TypeSignature elementType)
		{
			if (elementType == null)
				throw new ArgumentNullException("elementType");

			_elementType = elementType;
		}

		#endregion

		#region Properties

		public override string Name
		{
			get
			{
				string name = _elementType.Name;
				if (name == null)
					return null;

				return name + "&";
			}
		}

		public override string Namespace
		{
			get { return _elementType.Namespace; }
		}

		public override TypeSignature ElementType
		{
			get { return _elementType; }
		}

		public override TypeElementCode ElementCode
		{
			get { return TypeElementCode.ByRef; }
		}

		#endregion

		#region Methods

		public override bool GetSize(Module module, out int size)
		{
			size = 4;
			return true;
		}

		protected internal override void InternMembers(Module module)
		{
			module.AddSignature(ref _elementType);
		}

		#endregion

		#region Static

		internal static ByRefType LoadByRef(IBinaryAccessor accessor, Module module)
		{
			var elementType = Load(accessor, module);

			return new ByRefType(elementType);
		}

		#endregion
	}
}
