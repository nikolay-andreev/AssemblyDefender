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
	public class EventViewModel : NodeViewModel
	{
		#region Fields

		private EventDeclaration _event;
		private AD.ProjectEvent _projectEvent;
		private AssemblyViewModel _assemblyViewModel;
		private MethodViewModel _addMethodViewModel;
		private MethodViewModel _removeMethodViewModel;
		private MethodViewModel _invokeMethodViewModel;

		#endregion

		#region Ctors

		public EventViewModel(EventDeclaration e, AD.ProjectEvent projectEvent, ViewModel parent)
			: base(parent)
		{
			_event = e;
			_projectEvent = projectEvent;
			_assemblyViewModel = FindParent<AssemblyViewModel>(true);

			Caption = NodePrinter.Print(_event);
			Image = ImageType.Node_Event;
		}

		#endregion

		#region Properties

		public EventDeclaration Event
		{
			get { return _event; }
		}

		public AD.ProjectEvent ProjectEvent
		{
			get { return _projectEvent; }
		}

		public AssemblyViewModel AssemblyViewModel
		{
			get { return _assemblyViewModel; }
		}

		public MethodViewModel AddMethodViewModel
		{
			get { return _addMethodViewModel; }
		}

		public MethodViewModel RemoveMethodViewModel
		{
			get { return _removeMethodViewModel; }
		}

		public MethodViewModel InvokeMethodViewModel
		{
			get { return _invokeMethodViewModel; }
		}

		public override bool HasChildren
		{
			get
			{
				if (_addMethodViewModel != null ||
					_removeMethodViewModel != null ||
					_invokeMethodViewModel != null)
					return true;

				return false;
			}
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Event; }
		}

		#endregion

		#region Methods

		public override NodeLink ToLink()
		{
			return new EventLink(_event);
		}

		public void ShowDetails()
		{
			ShowSection(new EventDetailsViewModel(this));
		}

		public void ShowAddMethod()
		{
			if (_addMethodViewModel == null)
				return;

			_addMethodViewModel.Show();
		}

		public void ShowRemoveMethod()
		{
			if (_removeMethodViewModel == null)
				return;

			_removeMethodViewModel.Show();
		}

		public void ShowInvokeMethod()
		{
			if (_invokeMethodViewModel == null)
				return;

			_invokeMethodViewModel.Show();
		}

		internal void AddMethods(
			MethodViewModel addMethodViewModel,
			MethodViewModel removeMethodViewModel,
			MethodViewModel invokeMethodViewModel)
		{
			if (addMethodViewModel != null)
				_addMethodViewModel = addMethodViewModel;

			if (removeMethodViewModel != null)
				_removeMethodViewModel = removeMethodViewModel;

			if (invokeMethodViewModel != null)
				_invokeMethodViewModel = invokeMethodViewModel;

			// Set image
			if (_addMethodViewModel != null)
				Image = ProjectUtils.GetEventImage(_addMethodViewModel.Method);
			else if (_removeMethodViewModel != null)
				Image = ProjectUtils.GetEventImage(_removeMethodViewModel.Method);
			else if (_invokeMethodViewModel != null)
				Image = ProjectUtils.GetEventImage(_invokeMethodViewModel.Method);
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

			if (!projectModule.Events.ContainsKey(_event))
			{
				moduleViewModel.AddProjectNode();
				projectModule.Events.Add(_event, _projectEvent);
			}
		}

		protected override void LoadChildren(List<NodeViewModel> children)
		{
			if (_addMethodViewModel != null)
				children.Add(_addMethodViewModel);

			if (_removeMethodViewModel != null)
				children.Add(_removeMethodViewModel);

			if (_invokeMethodViewModel != null)
				children.Add(_invokeMethodViewModel);
		}

		protected override NodeMenu CreateMenu()
		{
			return new EventMenu();
		}

		#endregion
	}
}
