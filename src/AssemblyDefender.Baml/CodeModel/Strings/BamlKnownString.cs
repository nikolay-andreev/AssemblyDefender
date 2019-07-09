using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlKnownString : IBamlString
	{
		private BamlKnownStringCode _knownCode;

		public BamlKnownString()
		{
		}

		public BamlKnownString(BamlKnownStringCode knownCode)
		{
			_knownCode = knownCode;
		}

		public string Value
		{
			get
			{
				switch (_knownCode)
				{
					case BamlKnownStringCode.Name:
						return "name";

					case BamlKnownStringCode.Uid:
						return "uid";

					default:
						return "(unknown)";
				}
			}
		}

		public BamlKnownStringCode KnownCode
		{
			get { return _knownCode; }
			set { _knownCode = value; }
		}

		BamlStringKind IBamlString.Kind
		{
			get { return BamlStringKind.Known; }
		}

		public override string ToString()
		{
			return string.Format("KnownString: {0}", Value ?? "");
		}
	}
}
