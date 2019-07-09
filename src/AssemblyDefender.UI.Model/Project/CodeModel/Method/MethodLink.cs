using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class MethodLink : NodeLink
	{
		#region Fields

		private int _rid;
		private Module _module;

		#endregion

		#region Ctors

		public MethodLink(MethodDeclaration method)
		{
			_rid = method.RID;
			_module = method.Module;
		}

		#endregion

		#region Properties

		public int RID
		{
			get { return _rid; }
		}

		public Module Module
		{
			get { return _module; }
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Method; }
		}

		#endregion

		#region Methods

		public MethodDeclaration Resolve()
		{
			return _module.GetMethod(_rid);
		}

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Method)
				return false;

			var methodLink = (MethodLink)link;

			if (_rid != methodLink._rid)
				return false;

			if (!object.ReferenceEquals(_module, methodLink._module))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			var method = Resolve();
			if (method == null)
				return null;

			return projectViewModel.FindMethod(method);
		}

		#endregion
	}
}
