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
	public class ModuleViewModel : NodeViewModel
	{
		#region Fields

		private Module _module;
		private AD.ProjectModule _projectModule;

		#endregion

		#region Ctors

		public ModuleViewModel(Module module, AD.ProjectModule projectModule, ViewModel parent)
			: base(parent)
		{
			_module = module;
			_projectModule = projectModule;

			Caption = _module.Name;
			Image = ImageType.Node_Module;
		}

		#endregion

		#region Properties

		public Module Module
		{
			get { return _module; }
		}

		public AD.ProjectModule ProjectModule
		{
			get { return _projectModule; }
		}

		public override bool HasChildren
		{
			get
			{
				if (_module.Types.Count > 0)
					return true;

				return false;
			}
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Module; }
		}

		#endregion

		#region Methods

		public override NodeLink ToLink()
		{
			return new ModuleLink(_module);
		}

		public NamespaceViewModel FindNamespace(string ns)
		{
			Expand();

			for (int i = 0; i < ChildCount; i++)
			{
				var nodeViewModel = GetChild(i);
				if (nodeViewModel.NodeType == NodeViewModelType.Namespace)
				{
					var namespaceViewModel = (NamespaceViewModel)nodeViewModel;
					if (namespaceViewModel.Caption == ns)
						return namespaceViewModel;
				}
			}

			return null;
		}

		public void ShowDetails()
		{
			ShowSection(new ModuleDetailsViewModel(this));
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
			var assemblyViewModel = FindParent<AssemblyViewModel>(true);
			var projectAssembly = assemblyViewModel.ProjectAssembly;

			if (!projectAssembly.Modules.ContainsKey(_module.Name))
			{
				projectAssembly.Modules.Add(_module.Name, _projectModule);
			}
		}

		protected override void LoadChildren(List<NodeViewModel> children)
		{
			var viewModelByID = new Dictionary<string, NamespaceViewModel>();

			foreach (var type in _module.Types)
			{
				string ns = type.Namespace ?? "";

				NamespaceViewModel viewModel;
				if (!viewModelByID.TryGetValue(ns, out viewModel))
				{
					AD.ProjectNamespace projectNamespace;
					if (!_projectModule.Namespaces.TryGetValue(ns, out projectNamespace))
					{
						projectNamespace = new AD.ProjectNamespace();
					}

					viewModel = new NamespaceViewModel(ns, projectNamespace, this);
					viewModelByID.Add(ns, viewModel);
					children.Add(viewModel);
				}

				viewModel.AddType(type);
			}

			children.Sort(NodeComparer.Default);
		}

		protected override NodeMenu CreateMenu()
		{
			return new ModuleMenu();
		}

		#endregion
	}
}
