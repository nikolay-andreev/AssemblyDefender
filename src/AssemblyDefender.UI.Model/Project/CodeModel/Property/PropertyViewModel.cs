using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.UI;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
	public class PropertyViewModel : NodeViewModel
	{
		#region Fields

		private PropertyDeclaration _property;
		private AD.ProjectProperty _projectProperty;
		private MethodViewModel _getMethodViewModel;
		private MethodViewModel _setMethodViewModel;

		#endregion

		#region Ctors

		public PropertyViewModel(PropertyDeclaration property, AD.ProjectProperty projectProperty, ViewModel parent)
			: base(parent)
		{
			_property = property;
			_projectProperty = projectProperty;

			Caption = NodePrinter.Print(_property);
			Image = ImageType.Node_Properties;
		}

		#endregion

		#region Properties

		public PropertyDeclaration Property
		{
			get { return _property; }
		}

		public AD.ProjectProperty ProjectProperty
		{
			get { return _projectProperty; }
		}

		public MethodViewModel GetMethodViewModel
		{
			get { return _getMethodViewModel; }
		}

		public MethodViewModel SetMethodViewModel
		{
			get { return _setMethodViewModel; }
		}

		public override bool HasChildren
		{
			get
			{
				if (_getMethodViewModel != null ||
					_setMethodViewModel != null)
					return true;

				return false;
			}
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Property; }
		}

		#endregion

		#region Methods

		public override NodeLink ToLink()
		{
			return new PropertyLink(_property);
		}

		public void ShowDetails()
		{
			ShowSection(new PropertyDetailsViewModel(this));
		}

		public void ShowGetMethod()
		{
			if (_getMethodViewModel == null)
				return;

			_getMethodViewModel.Show();
		}

		public void ShowSetMethod()
		{
			if (_setMethodViewModel == null)
				return;

			_setMethodViewModel.Show();
		}

		internal void AddMethods(MethodViewModel getMethodViewModel, MethodViewModel setMethodViewModel)
		{
			if (getMethodViewModel != null)
				_getMethodViewModel = getMethodViewModel;

			if (setMethodViewModel != null)
				_setMethodViewModel = setMethodViewModel;

			// Set image
			if (_getMethodViewModel != null)
				Image = ProjectUtils.GetPropertyImage(_getMethodViewModel.Method);
			else if (_setMethodViewModel != null)
				Image = ProjectUtils.GetPropertyImage(_setMethodViewModel.Method);
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

			if (!projectModule.Properties.ContainsKey(_property))
			{
				moduleViewModel.AddProjectNode();
				projectModule.Properties.Add(_property, _projectProperty);
			}
		}

		protected override void LoadChildren(List<NodeViewModel> children)
		{
			if (_getMethodViewModel != null)
				children.Add(_getMethodViewModel);

			if (_setMethodViewModel != null)
				children.Add(_setMethodViewModel);
		}

		protected override NodeMenu CreateMenu()
		{
			return new PropertyMenu();
		}

		#endregion
	}
}
