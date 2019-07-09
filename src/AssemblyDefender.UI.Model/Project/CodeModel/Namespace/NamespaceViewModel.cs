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
	public class NamespaceViewModel : NodeViewModel
	{
		#region Fields

		private AD.ProjectNamespace _projectNamespace;
		private List<TypeDeclaration> _types = new List<TypeDeclaration>();

		#endregion

		#region Ctors

		internal NamespaceViewModel(string name, AD.ProjectNamespace projectNamespace, ViewModel parent)
			: base(parent)
		{
			_projectNamespace = projectNamespace;

			Caption = name;
			Image = ImageType.Node_Namespace;
		}

		#endregion

		#region Properties

		public AD.ProjectNamespace ProjectNamespace
		{
			get { return _projectNamespace; }
		}

		public override bool HasChildren
		{
			get { return true; }
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Namespace; }
		}

		#endregion

		#region Methods

		public override NodeLink ToLink()
		{
			var moduleViewModel = FindParent<ModuleViewModel>(true);
			return new NamespaceLink(Caption, moduleViewModel.Module);
		}

		public TypeViewModel FindType(TypeDeclaration type)
		{
			Expand();

			for (int i = 0; i < ChildCount; i++)
			{
				var nodeViewModel = GetChild(i);
				if (nodeViewModel.NodeType == NodeViewModelType.Type)
				{
					var typeViewModel = (TypeViewModel)nodeViewModel;
					if (object.ReferenceEquals(type, typeViewModel.Type))
						return typeViewModel;
				}
			}

			return null;
		}

		public void ShowDetails()
		{
			ShowSection(new NamespaceDetailsViewModel(this));
		}

		internal void AddType(TypeDeclaration type)
		{
			_types.Add(type);
		}

		protected override void OnActivate()
		{
			ShowDetails();
			base.OnActivate();
		}

		protected override void OnDeactivate()
		{
			if (IsChanged)
			{
				AddProjectNode();
			}

			base.OnDeactivate();
		}

		protected internal override void AddProjectNode()
		{
			var moduleViewModel = FindParent<ModuleViewModel>(true);
			var projectModule = moduleViewModel.ProjectModule;

			if (!projectModule.Namespaces.ContainsKey(Caption))
			{
				moduleViewModel.AddProjectNode();
				projectModule.Namespaces.Add(Caption, _projectNamespace);
			}
		}

		protected override void LoadChildren(List<NodeViewModel> children)
		{
			var moduleViewModel = FindParent<ModuleViewModel>(true);
			var projectModule = moduleViewModel.ProjectModule;
			var projectTypes = projectModule.Types;

			foreach (var type in _types)
			{
				AD.ProjectType projectType;
				if (!projectTypes.TryGetValue(type, out projectType))
				{
					projectType = new AD.ProjectType();
				}

				var typeViewModel = new TypeViewModel(type, projectType, this);
				children.Add(typeViewModel);
			}

			children.Sort(NodeComparer.Default);
		}

		protected override NodeMenu CreateMenu()
		{
			return new NamespaceMenu();
		}

		#endregion
	}
}
