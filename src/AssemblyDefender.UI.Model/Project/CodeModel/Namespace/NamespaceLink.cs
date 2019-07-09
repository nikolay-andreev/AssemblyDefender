using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class NamespaceLink : NodeLink
	{
		#region Fields

		private string _namespace;
		private Module _module;

		#endregion

		#region Ctors

		public NamespaceLink(string ns, Module module)
		{
			_namespace = ns;
			_module = module;
		}

		#endregion

		#region Properties

		public string Namespace
		{
			get { return _namespace; }
		}

		public Module Module
		{
			get { return _module; }
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Namespace; }
		}

		#endregion

		#region Methods

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Namespace)
				return false;

			var namespaceLink = (NamespaceLink)link;
			if (_namespace != namespaceLink._namespace)
				return false;

			if (!object.ReferenceEquals(_module, namespaceLink._module))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			return projectViewModel.FindNamespace(_module, _namespace);
		}

		#endregion
	}
}
