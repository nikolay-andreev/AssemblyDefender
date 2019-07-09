using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.UI.Model.Project
{
	public class FieldMenu : NodeMenu
	{
		private FieldViewModel _node;

		public override void Load(NodeViewModel node)
		{
			_node = (FieldViewModel)node;
		}

		public override void Unload()
		{
			_node = null;
		}

		protected override void LoadItems()
		{
		}
	}
}
