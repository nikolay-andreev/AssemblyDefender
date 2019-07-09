using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public class AssemblyMenu : NodeMenu
	{
		private AssemblyViewModel _node;

		public override void Load(NodeViewModel node)
		{
			_node = (AssemblyViewModel)node;
		}

		public override void Unload()
		{
			_node = null;
		}

		protected override void LoadItems()
		{
			Items.Add(
				new MenuItemViewModel()
				{
					Text = SR.OpenInWindowsExplorer,
					Image = ImageType.OpenFile,
					Command = new DelegateCommand(OpenInWindowsExplorer),
				});

			Items.Add(
				new MenuItemViewModel()
				{
					IsSeparator = true,
				});

			Items.Add(
				new MenuItemViewModel()
				{
					Text = SR.Remove,
					Image = ImageType.Delete,
					Command = new DelegateCommand(Remove),
				});
		}

		private void OpenInWindowsExplorer()
		{
			_node.OpenInWindowsExplorer();
		}

		private void Remove()
		{
			if (_node == null)
				return;

			_node.Remove();
		}
	}
}
