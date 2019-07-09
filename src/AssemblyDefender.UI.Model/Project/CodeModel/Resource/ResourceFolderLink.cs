using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class ResourceFolderLink : NodeLink
	{
		#region Fields

		private Assembly _assembly;

		#endregion

		#region Ctors

		public ResourceFolderLink(Assembly assembly)
		{
			_assembly = assembly;
		}

		#endregion

		#region Properties

		public Assembly Assembly
		{
			get { return _assembly; }
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.ResourceFolder; }
		}

		#endregion

		#region Methods

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.ResourceFolder)
				return false;

			var resourceFolderLink = (ResourceFolderLink)link;
			if (!object.ReferenceEquals(_assembly, resourceFolderLink._assembly))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			return projectViewModel.FindResourceFolder(_assembly);
		}

		#endregion
	}
}
