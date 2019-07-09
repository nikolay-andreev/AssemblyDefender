using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.Baml
{
	public class BamlKnownType : IBamlType
	{
		private BamlKnownTypeCode _knownCode;

		public BamlKnownType()
		{
		}

		public BamlKnownType(BamlKnownTypeCode knownCode)
		{
			_knownCode = knownCode;
		}

		public BamlKnownTypeCode KnownCode
		{
			get { return _knownCode; }
			set { _knownCode = value; }
		}

		BamlTypeKind IBamlType.Kind
		{
			get { return BamlTypeKind.Known; }
		}

		public TypeDeclaration Resolve(Assembly ownerAssembly)
		{
			return null;
		}

		public override string ToString()
		{
			return string.Format("KnownType: {0}", _knownCode.ToString());
		}
	}
}
