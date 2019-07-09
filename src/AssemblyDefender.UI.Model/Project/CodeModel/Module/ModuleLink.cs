using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class ModuleLink : NodeLink
	{
		#region Fields

		private Module _module;

		#endregion

		#region Ctors

		public ModuleLink(Module module)
		{
			_module = module;
		}

		#endregion

		#region Properties

		public Module Module
		{
			get { return _module; }
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Module; }
		}

		#endregion

		#region Methods

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Module)
				return false;

			var moduleLink = (ModuleLink)link;
			if (!object.ReferenceEquals(_module, moduleLink._module))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			return projectViewModel.FindModule(_module);
		}

		#endregion
	}
}
