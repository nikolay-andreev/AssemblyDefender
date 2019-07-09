using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public abstract class SearchItem : PropertyAwareObject
	{
		#region Fields

		protected string _caption;
		protected ImageType? _image;

		#endregion

		#region Ctors

		protected SearchItem()
		{
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Caption
		{
			get { return _caption; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ImageType Image
		{
			get
			{
				if (!_image.HasValue)
				{
					LoadImage();
				}

				return _image.Value;
			}
		}

		#endregion

		#region Methods

		public bool Show(ProjectViewModel projectViewModel)
		{
			var node = FindNode(projectViewModel);
			if (node == null)
				return false;

			node.Show();

			return true;
		}

		public abstract NodeViewModel FindNode(ProjectViewModel projectViewModel);

		public abstract bool Match(string searchText, StringComparison comparison);

		protected abstract void LoadImage();

		#endregion
	}
}
