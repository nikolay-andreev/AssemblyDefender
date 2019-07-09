using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public abstract class NodeMenu : PropertyAwareObject
	{
		#region Fields

		private List<MenuItemViewModel> _items;

		#endregion

		#region Ctors

		public NodeMenu()
		{
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public List<MenuItemViewModel> Items
		{
			get
			{
				if (_items == null)
				{
					_items = new List<MenuItemViewModel>();
					LoadItems();
				}

				return _items;
			}
		}

		#endregion

		#region Methods

		public virtual void Load(NodeViewModel node)
		{
		}

		public virtual void Unload()
		{
		}

		protected abstract void LoadItems();

		#endregion
	}
}
