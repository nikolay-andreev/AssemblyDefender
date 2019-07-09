using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.Baml
{
	public class BamlKnownProperty : IBamlProperty
	{
		private BamlKnownPropertyCode _knownCode;

		public BamlKnownProperty()
		{
		}

		public BamlKnownProperty(BamlKnownPropertyCode knownCode)
		{
			_knownCode = knownCode;
		}

		public BamlKnownPropertyCode KnownCode
		{
			get { return _knownCode; }
			set { _knownCode = value; }
		}

		BamlPropertyKind IBamlProperty.Kind
		{
			get { return BamlPropertyKind.Known; }
		}

		public MemberNode Resolve(Assembly ownerAssembly)
		{
			return null;
		}

		public override string ToString()
		{
			return string.Format("KnownProperty: {0}", _knownCode.ToString());
		}
	}
}
