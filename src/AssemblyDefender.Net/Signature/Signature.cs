using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public abstract class Signature : ISignature
	{
		public abstract SignatureType SignatureType
		{
			get;
		}

		protected internal virtual void InternMembers(Module module)
		{
		}
	}
}
