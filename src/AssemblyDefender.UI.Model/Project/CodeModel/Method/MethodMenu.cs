using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.UI.Model.Project
{
	public class MethodMenu : NodeMenu
	{
		private MethodViewModel _node;

		public override void Load(NodeViewModel node)
		{
			_node = (MethodViewModel)node;
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
