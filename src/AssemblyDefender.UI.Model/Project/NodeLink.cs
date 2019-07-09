using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public abstract class NodeLink
	{
		#region Ctors

		public NodeLink()
		{
		}

		#endregion

		#region Properties

		public abstract NodeViewModelType NodeType
		{
			get;
		}

		#endregion

		#region Methods

		public bool Show(ProjectViewModel projectViewModel)
		{
			var node = FindNode(projectViewModel);
			if (node == null)
				return false;

			node.Show();

			return true;
		}

		public abstract bool Equals(NodeLink link);

		public abstract NodeViewModel FindNode(ProjectViewModel projectViewModel);

		#endregion
	}
}
