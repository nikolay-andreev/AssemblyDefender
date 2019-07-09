using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;

namespace AssemblyDefender.UI.Model.Project
{
	internal class NodeComparer : IComparer<NodeViewModel>
	{
		internal static readonly NodeComparer Default = new NodeComparer();

		public int Compare(NodeViewModel x, NodeViewModel y)
		{
			return StringLogicalComparer.Default.Compare(x.Caption, y.Caption);
		}
	}
}
