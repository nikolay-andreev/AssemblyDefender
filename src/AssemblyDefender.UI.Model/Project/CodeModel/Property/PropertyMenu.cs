using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public class PropertyMenu : NodeMenu
	{
		private PropertyViewModel _node;

		public override void Load(NodeViewModel node)
		{
			_node = (PropertyViewModel)node;

			Items[0].IsEnabled = (_node.GetMethodViewModel != null);
			Items[1].IsEnabled = (_node.SetMethodViewModel != null);
		}

		public override void Unload()
		{
			_node = null;
		}

		public void OnStripChanged(bool strip)
		{
			Items[0].IsEnabled = !strip;
		}

		protected override void LoadItems()
		{
			Items.Add(
				new MenuItemViewModel()
				{
					Text = SR.PropertySemanticGet,
					Image = ImageType.Node_Method_Shortcut,
					Command = new DelegateCommand(ShowGetMethod),
				});

			Items.Add(
				new MenuItemViewModel()
				{
					Text = SR.PropertySemanticSet,
					Image = ImageType.Node_Method_Shortcut,
					Command = new DelegateCommand(ShowSetMethod),
				});
		}

		public void ShowGetMethod()
		{
			if (_node == null)
				return;

			_node.ShowGetMethod();
		}

		public void ShowSetMethod()
		{
			if (_node == null)
				return;

			_node.ShowSetMethod();
		}
	}
}
