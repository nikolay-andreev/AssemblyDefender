using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.UI.Model.Project
{
	public class ProjectLink : NodeLink
	{
		#region Ctors

		public ProjectLink()
		{
		}

		#endregion

		#region Properties

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Project; }
		}

		#endregion

		#region Methods

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Project)
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			return projectViewModel;
		}

		#endregion
	}
}
