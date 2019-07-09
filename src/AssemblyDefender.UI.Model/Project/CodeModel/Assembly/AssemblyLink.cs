using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class AssemblyLink : NodeLink
	{
		#region Fields

		private Assembly _assembly;

		#endregion

		#region Ctors

		public AssemblyLink(Assembly assembly)
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
			get { return NodeViewModelType.Assembly; }
		}

		#endregion

		#region Methods

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Assembly)
				return false;

			var assemblyLink = (AssemblyLink)link;
			if (!object.ReferenceEquals(_assembly, assemblyLink._assembly))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			return projectViewModel.FindAssembly(_assembly);
		}

		#endregion
	}
}
