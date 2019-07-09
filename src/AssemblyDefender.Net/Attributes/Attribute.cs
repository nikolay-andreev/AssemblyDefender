using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.CustomAttributes
{
	public abstract class Attribute : IBinarySerialize
	{
		public abstract void Build(CustomAttribute customAttribute);

		public abstract void Serialize(IBinaryAccessor accessor);

		public abstract void Deserialize(IBinaryAccessor accessor);
	}
}
