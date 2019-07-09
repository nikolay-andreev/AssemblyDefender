using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;

namespace AssemblyDefender.UI.Model.Project
{
	public class PropertyLink : NodeLink
	{
		#region Fields

		private int _rid;
		private Module _module;

		#endregion

		#region Ctors

		public PropertyLink(PropertyDeclaration property)
		{
			_rid = property.RID;
			_module = property.Module;
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
			get { return NodeViewModelType.Property; }
		}

		#endregion

		#region Methods

		public PropertyDeclaration Resolve()
		{
			return _module.GetProperty(_rid);
		}

		public override bool Equals(NodeLink link)
		{
			if (link.NodeType != NodeViewModelType.Property)
				return false;

			var propertyLink = (PropertyLink)link;

			if (_rid != propertyLink._rid)
				return false;

			if (!object.ReferenceEquals(_module, propertyLink._module))
				return false;

			return true;
		}

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			var property = Resolve();
			if (property == null)
				return null;

			return projectViewModel.FindProperty(property);
		}

		#endregion
	}
}
