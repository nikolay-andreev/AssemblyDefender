using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AD = AssemblyDefender;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public class ResourceFolderViewModel : NodeViewModel
	{
		#region Fields

		private Assembly _assembly;
		private AD.ProjectAssembly _projectAssembly;

		#endregion

		#region Ctors

		public ResourceFolderViewModel(Assembly assembly, AD.ProjectAssembly projectAssembly, ViewModel parent)
			: base(parent)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (projectAssembly == null)
				throw new ArgumentNullException("projectAssembly");

			_assembly = assembly;
			_projectAssembly = projectAssembly;

			Caption = SR.Resources;
			Image = ImageType.Node_Resource;
		}

		#endregion

		#region Properties

		public Assembly Assembly
		{
			get { return _assembly; }
		}

		public AD.ProjectAssembly ProjectAssembly
		{
			get { return _projectAssembly; }
		}

		public override bool HasChildren
		{
			get { return false; }
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Resource; }
		}

		#endregion

		#region Methods

		public override NodeLink ToLink()
		{
			return new ResourceFolderLink(FindParent<AssemblyViewModel>().Assembly);
		}

		public void ShowDetails()
		{
			ShowSection(new ResourceFolderDetailsViewModel(this));
		}
		
		protected override void OnActivate()
		{
			ShowDetails();
			base.OnActivate();
		}

		protected override NodeMenu CreateMenu()
		{
			return new ResourceFolderMenu();
		}

		#endregion
	}
}
