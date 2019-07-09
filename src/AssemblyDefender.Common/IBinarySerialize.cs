using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Common
{
	public interface IBinarySerialize
	{
		void Serialize(IBinaryAccessor accessor);

		void Deserialize(IBinaryAccessor accessor);
	}
}
