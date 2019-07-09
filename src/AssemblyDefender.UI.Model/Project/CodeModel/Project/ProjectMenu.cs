using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public class ProjectMenu : NodeMenu
	{
		private ProjectViewModel _node;

		public override void Load(NodeViewModel node)
		{
			_node = (ProjectViewModel)node;
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
					Text = SR.AddAssembly,
					Image = ImageType.Node_Assembly_Add,
					Command = Commands.AddAssembly,
				});

			Items.Add(
				new MenuItemViewModel()
				{
					Text = SR.Build,
					Image = ImageType.Compile,
					Command = Commands.Build,
				});

			Items.Add(
				new MenuItemViewModel()
				{
					Text = SR.OpenInWindowsExplorer,
					Image = ImageType.OpenFile,
					Command = new DelegateCommand(OpenInWindowsExplorer, () => _node != null && !_node.ProjectShell.IsNew),
				});
		}

		private void OpenInWindowsExplorer()
		{
			_node.OpenInWindowsExplorer();
		}
	}
}
