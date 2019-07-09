using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using AssemblyDefender.Net;
using AD = AssemblyDefender;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public class MethodViewModel : NodeViewModel
	{
		#region Fields

		private MethodDeclaration _method;
		private AD.ProjectMethod _projectMethod;

		#endregion

		#region Ctors

		internal MethodViewModel(MethodDeclaration method, AD.ProjectMethod projectMethod, ViewModel parent)
			: base(parent)
		{
			_method = method;
			_projectMethod = projectMethod;

			Caption = NodePrinter.Print(method);
			Image = ProjectUtils.GetMethodImage(method);
		}

		#endregion

		#region Properties

		public MethodDeclaration Method
		{
			get { return _method; }
		}

		public AD.ProjectMethod ProjectMethod
		{
			get { return _projectMethod; }
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Method; }
		}

		#endregion

		#region Methods

		public override NodeLink ToLink()
		{
			return new MethodLink(_method);
		}

		public void ShowDetails()
		{
			ShowSection(new MethodDetailsViewModel(this));
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

			if (!projectModule.Methods.ContainsKey(_method))
			{
				moduleViewModel.AddProjectNode();
				projectModule.Methods.Add(_method, _projectMethod);
			}
		}

		protected override NodeMenu CreateMenu()
		{
			return new MethodMenu();
		}

		#endregion
	}
}
