using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AssemblyDefender.UI.Model.Project;

namespace AssemblyDefender.UI.Shell.Project
{
	internal static class NavigationUtilities
	{
		internal static void MoveUp(NavigationListBox listBox, NodeViewModel node)
		{
			var nodes = node.ProjectShell.Nodes;

			int pos = node.GetPosition();
			if (pos > 0)
			{
				var nextNode = nodes[pos - 1];
				ShowNode(listBox, nextNode);
			}
		}

		internal static void MoveDown(NavigationListBox listBox, NodeViewModel node)
		{
			var nodes = node.ProjectShell.Nodes;

			int pos = node.GetPosition();
			if (pos < nodes.Count - 1)
			{
				var nextNode = nodes[pos + 1];
				ShowNode(listBox, nextNode);
			}
		}

		internal static void MoveLeft(NavigationListBox listBox, NodeViewModel node)
		{
			if (node.IsExpanded)
			{
				if (node.CanCollapse)
				{
					node.IsExpanded = false;
				}
			}
			else
			{
				var parentNode = node.Parent as NodeViewModel;
				if (parentNode != null)
				{
					ShowNode(listBox, parentNode);
				}
			}
		}

		internal static void MoveRight(NavigationListBox listBox, NodeViewModel node)
		{
			if (!node.IsExpanded)
			{
				node.IsExpanded = true;
			}
			else
			{
				if (node.ChildCount > 0)
				{
					var childNode = node.GetChild(0);
					childNode.Show();
				}
			}
		}

		internal static void ShowNode(NavigationListBox listBox, NodeViewModel node)
		{
			ScrollIntoView(listBox, node);
			node.IsSelected = true;
		}

		internal static void ScrollIntoView(NavigationListBox listBox, NodeViewModel node)
		{
			int pos = node.GetPosition();
			if (pos >= listBox.Items.Count)
			{
				Debug.Fail("Internal error");
				throw new InvalidOperationException();
			}

			listBox.ScrollIntoView(listBox.Items[pos]);
		}
	}
}
