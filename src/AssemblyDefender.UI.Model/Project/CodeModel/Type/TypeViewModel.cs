using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AD = AssemblyDefender;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Model.Project
{
	public class TypeViewModel : NodeViewModel
	{
		#region Fields

		private TypeDeclaration _type;
		private AD.ProjectType _projectType;
		private TypeNodeKind _typeKind;

		#endregion

		#region Ctors

		public TypeViewModel(TypeDeclaration type, AD.ProjectType projectType, ViewModel parent)
			: base(parent)
		{
			_type = type;
			_projectType = projectType;
			_typeKind = ProjectUtils.GetTypeKind(type);

			Caption = NodePrinter.Print(type, true, true);
			Image = ProjectUtils.GetTypeImage(type, _typeKind);
		}

		#endregion

		#region Properties

		public TypeDeclaration Type
		{
			get { return _type; }
		}

		public AD.ProjectType ProjectType
		{
			get { return _projectType; }
		}

		public TypeNodeKind TypeKind
		{
			get { return _typeKind; }
		}

		public override bool HasChildren
		{
			get
			{
				if (_type.Methods.Count > 0)
					return true;

				if (_type.Fields.Count > 0)
					return true;

				if (_type.NestedTypes.Count > 0)
					return true;

				return false;
			}
		}

		public override NodeViewModelType NodeType
		{
			get { return NodeViewModelType.Type; }
		}

		#endregion

		#region Methods

		public override NodeLink ToLink()
		{
			return new TypeLink(_type);
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

		public MethodViewModel FindMethod(MethodDeclaration method)
		{
			Expand();

			for (int i = 0; i < ChildCount; i++)
			{
				var nodeViewModel = GetChild(i);
				switch (nodeViewModel.NodeType)
				{
					case NodeViewModelType.Method:
						{
							var methodViewModel = (MethodViewModel)nodeViewModel;
							if (object.ReferenceEquals(method, methodViewModel.Method))
								return methodViewModel;
						}
						break;

					case NodeViewModelType.Property:
					case NodeViewModelType.Event:
						{
							var methodViewModel = FindMethodSemanticNode(method, nodeViewModel);
							if (methodViewModel != null)
							{
								nodeViewModel.Expand();
								return methodViewModel;
							}
						}
						break;
				}
			}

			return null;
		}

		private MethodViewModel FindMethodSemanticNode(MethodDeclaration method, NodeViewModel parent)
		{
			for (int i = 0; i < parent.ChildCount; i++)
			{
				var nodeViewModel = parent.GetChild(i);
				if (nodeViewModel.NodeType == NodeViewModelType.Method)
				{
					var methodViewModel = (MethodViewModel)nodeViewModel;
					if (object.ReferenceEquals(method, methodViewModel.Method))
						return methodViewModel;
				}
			}

			return null;
		}

		public FieldViewModel FindField(FieldDeclaration field)
		{
			Expand();

			for (int i = 0; i < ChildCount; i++)
			{
				var nodeViewModel = GetChild(i);
				if (nodeViewModel.NodeType == NodeViewModelType.Field)
				{
					var fieldViewModel = (FieldViewModel)nodeViewModel;
					if (object.ReferenceEquals(field, fieldViewModel.Field))
						return fieldViewModel;
				}
			}

			return null;
		}

		public PropertyViewModel FindProperty(PropertyDeclaration property)
		{
			Expand();

			for (int i = 0; i < ChildCount; i++)
			{
				var nodeViewModel = GetChild(i);
				if (nodeViewModel.NodeType == NodeViewModelType.Property)
				{
					var propertyViewModel = (PropertyViewModel)nodeViewModel;
					if (object.ReferenceEquals(property, propertyViewModel.Property))
						return propertyViewModel;
				}
			}

			return null;
		}

		public EventViewModel FindEvent(EventDeclaration eventDecl)
		{
			Expand();

			for (int i = 0; i < ChildCount; i++)
			{
				var nodeViewModel = GetChild(i);
				if (nodeViewModel.NodeType == NodeViewModelType.Event)
				{
					var eventViewModel = (EventViewModel)nodeViewModel;
					if (object.ReferenceEquals(eventDecl, eventViewModel.Event))
						return eventViewModel;
				}
			}

			return null;
		}

		public void ShowDetails()
		{
			ShowSection(new TypeDetailsViewModel(this));
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

			if (!projectModule.Types.ContainsKey(_type))
			{
				moduleViewModel.AddProjectNode();
				projectModule.Types.Add(_type, _projectType);
			}
		}

		protected override void LoadChildren(List<NodeViewModel> children)
		{
			var moduleViewModel = FindParent<ModuleViewModel>(true);
			var projectModule = moduleViewModel.ProjectModule;

			// Load methods
			var methods = new Dictionary<IMethodSignature, MethodDeclaration>(SignatureComparer.IgnoreMemberOwner);
			foreach (var method in _type.Methods)
			{
				methods.Add(method, method);
			}

			// Load properties
			List<PropertyViewModel> propertyViewModels = null;
			if (_type.Properties.Count > 0)
			{
				propertyViewModels = new List<PropertyViewModel>(_type.Properties.Count);
				var projectProperties = projectModule.Properties;

				foreach (var property in _type.Properties)
				{
					AD.ProjectProperty projectProperty;
					if (!projectProperties.TryGetValue(property, out projectProperty))
					{
						projectProperty = new AD.ProjectProperty();
					}

					var propertyViewModel = new PropertyViewModel(property, projectProperty, this);

					propertyViewModel.AddMethods(
						CreateMethodSemantic(property.GetMethod, propertyViewModel, moduleViewModel, methods),
						CreateMethodSemantic(property.SetMethod, propertyViewModel, moduleViewModel, methods));

					propertyViewModels.Add(propertyViewModel);
				}
			}

			// Load events
			List<EventViewModel> eventViewModels = null;
			if (_type.Events.Count > 0)
			{
				eventViewModels = new List<EventViewModel>(_type.Events.Count);
				var projectEvents = projectModule.Events;

				foreach (var e in _type.Events)
				{
					AD.ProjectEvent projectEvent;
					if (!projectEvents.TryGetValue(e, out projectEvent))
					{
						projectEvent = new AD.ProjectEvent();
					}

					var eventViewModel = new EventViewModel(e, projectEvent, this);

					eventViewModel.AddMethods(
						CreateMethodSemantic(e.AddMethod, eventViewModel, moduleViewModel, methods),
						CreateMethodSemantic(e.RemoveMethod, eventViewModel, moduleViewModel, methods),
						CreateMethodSemantic(e.InvokeMethod, eventViewModel, moduleViewModel, methods));

					eventViewModels.Add(eventViewModel);
				}
			}

			// Add nested types
			if (_type.NestedTypes.Count > 0)
			{
				int index = children.Count;
				var projectTypes = projectModule.Types;

				foreach (var nestedType in _type.NestedTypes)
				{
					AD.ProjectType projectType;
					if (!projectTypes.TryGetValue(nestedType, out projectType))
					{
						projectType = new AD.ProjectType();
					}

					var typeViewModel = new TypeViewModel(nestedType, projectType, this);
					children.Add(typeViewModel);
				}

				children.Sort(index, children.Count - index, NodeComparer.Default);
			}

			// Add fields
			if (_type.Fields.Count > 0)
			{
				int index = children.Count;
				var projectFields = projectModule.Fields;

				foreach (var field in _type.Fields)
				{
					AD.ProjectField projectField;
					if (!projectFields.TryGetValue(field, out projectField))
					{
						projectField = new AD.ProjectField();
					}

					var fieldViewModel = new FieldViewModel(field, projectField, this);
					children.Add(fieldViewModel);
				}

				children.Sort(index, children.Count - index, NodeComparer.Default);
			}

			// Add methods
			if (methods.Count > 0)
			{
				int index = children.Count;
				var projectMethods = projectModule.Methods;

				foreach (var method in methods.Values)
				{
					AD.ProjectMethod projectMethod;
					if (!projectMethods.TryGetValue(method, out projectMethod))
					{
						projectMethod = new AD.ProjectMethod();
					}

					var methodViewModel = new MethodViewModel(method, projectMethod, this);
					children.Add(methodViewModel);
				}

				children.Sort(index, children.Count - index, NodeComparer.Default);
			}

			// Add properties
			if (propertyViewModels != null)
			{
				int index = children.Count;

				foreach (var propertyViewModel in propertyViewModels)
				{
					children.Add(propertyViewModel);
				}

				children.Sort(index, children.Count - index, NodeComparer.Default);
			}

			// Add events
			if (eventViewModels != null)
			{
				int index = children.Count;

				foreach (var eventViewModel in eventViewModels)
				{
					children.Add(eventViewModel);
				}

				children.Sort(index, children.Count - index, NodeComparer.Default);
			}
		}

		private MethodViewModel CreateMethodSemantic(
			MethodReference methodRef, ViewModel parent, ModuleViewModel moduleViewModel,
			Dictionary<IMethodSignature, MethodDeclaration> methods)
		{
			if (methodRef == null)
				return null;

			MethodDeclaration method;
			if (!methods.TryGetValue(methodRef, out method))
				return null;

			methods.Remove(methodRef);

			AD.ProjectMethod projectMethod;
			if (!moduleViewModel.ProjectModule.Methods.TryGetValue(methodRef, out projectMethod))
			{
				projectMethod = new AD.ProjectMethod();
			}

			return new MethodViewModel(method, projectMethod, parent);
		}

		protected override NodeMenu CreateMenu()
		{
			return new TypeMenu();
		}

		#endregion
	}
}
