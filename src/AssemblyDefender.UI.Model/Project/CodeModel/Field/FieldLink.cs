using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class FieldLink : NodeLink
	{
		#region Fields

		private int _rid;
		private Module _module;

		#endregion

		#region Ctors

		public FieldLink(FieldDeclaration field)
		{
			_rid = field.RID;
			_module = field.Module;
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
			get { return NodeViewModelType.Field; }
		}

		#endregion

		#region Methods

		public FieldDeclaration Resolve()
		{
			return _module.GetField(_rid);
		}

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Field)
				return false;

			var fieldLink = (FieldLink)link;

			if (_rid != fieldLink._rid)
				return false;

			if (!object.ReferenceEquals(_module, fieldLink._module))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			var field = Resolve();
			if (field == null)
				return null;

			return projectViewModel.FindField(field);
		}

		#endregion
	}
}
