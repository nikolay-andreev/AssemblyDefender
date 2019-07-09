using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class EventLink : NodeLink
	{
		#region Fields

		private int _rid;
		private Module _module;

		#endregion

		#region Ctors

		public EventLink(EventDeclaration eventDecl)
		{
			_rid = eventDecl.RID;
			_module = eventDecl.Module;
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
			get { return NodeViewModelType.Event; }
		}

		#endregion

		#region Methods

		public EventDeclaration Resolve()
		{
			return _module.GetEvent(_rid);
		}

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Event)
				return false;

			var eventLink = (EventLink)link;

			if (_rid != eventLink._rid)
				return false;

			if (!object.ReferenceEquals(_module, eventLink._module))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			var eventDecl = Resolve();
			if (eventDecl == null)
				return null;

			return projectViewModel.FindEvent(eventDecl);
		}

		#endregion
	}
}
