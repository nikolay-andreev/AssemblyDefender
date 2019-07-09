using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public class EventMenu : NodeMenu
	{
		private EventViewModel _node;

		public override void Load(NodeViewModel node)
		{
			_node = (EventViewModel)node;

			Items[0].IsEnabled = (_node.AddMethodViewModel != null);
			Items[1].IsEnabled = (_node.RemoveMethodViewModel != null);
			Items[2].IsEnabled = (_node.InvokeMethodViewModel != null);
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
					Text = SR.EventSemanticAdd,
					Image = ImageType.Node_Method_Shortcut,
					Command = new DelegateCommand(ShowAddMethod),
				});

			Items.Add(
				new MenuItemViewModel()
				{
					Text = SR.EventSemanticRemove,
					Image = ImageType.Node_Method_Shortcut,
					Command = new DelegateCommand(ShowRemoveMethod),
				});

			Items.Add(
				new MenuItemViewModel()
				{
					Text = SR.EventSemanticInvoke,
					Image = ImageType.Node_Method_Shortcut,
					Command = new DelegateCommand(ShowInvokeMethod),
				});
		}

		public void ShowAddMethod()
		{
			if (_node == null)
				return;

			_node.ShowAddMethod();
		}

		public void ShowRemoveMethod()
		{
			if (_node == null)
				return;

			_node.ShowRemoveMethod();
		}

		public void ShowInvokeMethod()
		{
			if (_node == null)
				return;

			_node.ShowInvokeMethod();
		}
	}
}
