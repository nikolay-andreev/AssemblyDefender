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
	public class FieldViewModel : NodeViewModel
	{
		#region Fields

		private FieldDeclaration _field;
		private AD.ProjectField _projectField;
		private FieldNodeKind _fieldKind;

		#endregion

		#region Ctors

		public FieldViewModel(FieldDeclaration field, AD.ProjectField projectField, ViewModel parent)
			: base(parent)
		{
			_field = field;
			_projectField = projectField;

			var typeViewModel = FindParent<TypeViewModel>();
			_fieldKind = ProjectUtils.GetFieldKind(field, typeViewModel.Type, typeViewModel.TypeKind);

			Caption = NodePrinter.Print(field, _fieldKind);
			Image = ProjectUtils.GetFieldImage(field, _fieldKind);
		}

		#endregion

		#region Properties

		public FieldDeclaration Field
		{
			get { return _field; }
		}

		public AD.ProjectField ProjectField
		{
			get { return _projectField; }
		}

		public FieldNodeKind FieldKind
		{
			get { return _fieldKind; }
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Field; }
		}

		#endregion

		#region Methods

		public override NodeLink ToLink()
		{
			return new FieldLink(_field);
		}

		public void ShowDetails()
		{
			ShowSection(new FieldDetailsViewModel(this));
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

			if (!projectModule.Fields.ContainsKey(_field))
			{
				moduleViewModel.AddProjectNode();
				projectModule.Fields.Add(_field, _projectField);
			}
		}

		protected override NodeMenu CreateMenu()
		{
			return new FieldMenu();
		}

		#endregion
	}
}
