using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	internal class SearchTypeItem : SearchItem
	{
		#region Fields

		private TypeDeclaration _type;

		#endregion

		#region Ctors

		internal SearchTypeItem(TypeDeclaration type)
		{
			_type = type;
			_caption = NodePrinter.Print(_type);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Name
		{
			get { return _type.Name; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Namespace
		{
			get { return _type.Namespace; }
		}

		public TypeDeclaration Type
		{
			get { return _type; }
		}

		#endregion

		#region Methods

		public override NodeViewModel FindNode(ProjectViewModel projectViewModel)
		{
			return projectViewModel.FindType(_type);
		}

		public override bool Match(string searchText, StringComparison comparison)
		{
			if (_type.Name != null)
			{
				if (_type.Name.StartsWith(searchText, comparison))
					return true;
			}

			return _caption.StartsWith(searchText, comparison);
		}

		protected override void LoadImage()
		{
			_image = ProjectUtils.GetTypeImage(_type);
		}

		#endregion
	}
}
