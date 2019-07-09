using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class TypeLink : NodeLink
	{
		#region Fields

		private int _rid;
		private Module _module;

		#endregion

		#region Ctors

		public TypeLink(TypeDeclaration type)
		{
			_rid = type.RID;
			_module = type.Module;
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
			get { return NodeViewModelType.Type; }
		}

		#endregion

		#region Methods

		public TypeDeclaration Resolve()
		{
			return _module.GetType(_rid);
		}

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Type)
				return false;

			var typeLink = (TypeLink)link;

			if (_rid != typeLink._rid)
				return false;

			if (!object.ReferenceEquals(_module, typeLink._module))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			var type = Resolve();
			if (type == null)
				return null;

			return projectViewModel.FindType(type);
		}

		#endregion
	}
}
