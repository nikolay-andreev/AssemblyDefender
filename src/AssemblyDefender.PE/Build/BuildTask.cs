using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.PE
{
	public abstract class BuildTask
	{
		internal int Priority;

		private PEBuilder _pe;

		public PEBuilder PE
		{
			get { return _pe; }
			internal set { _pe = value; }
		}

		public abstract void Build();
	}
}
