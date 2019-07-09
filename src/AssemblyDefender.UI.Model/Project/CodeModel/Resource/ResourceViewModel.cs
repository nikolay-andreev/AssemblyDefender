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

namespace AssemblyDefender.UI.Model.Project
{
	public class ResourceViewModel : ElementViewModel<ResourceFolderDetailsViewModel, ResourceFolderViewModel>
	{
		#region Fields

		private bool _isChanged;
		private bool _isExpanded;
		private bool _canObfuscate;
		private Resource _resource;
		private AD.ProjectResource _projectResource;

		#endregion

		#region Ctors

		public ResourceViewModel(Resource resource, ResourceFolderDetailsViewModel parent)
			: base(parent)
		{
			_resource = resource;
			_canObfuscate = parent.CanObfuscate;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _resource.Name; }
		}

		public Resource Resource
		{
			get { return _resource; }
		}

		public AD.ProjectResource ProjectResource
		{
			get
			{
				if (_projectResource == null)
				{
					LoadProject();
				}

				return _projectResource;
			}
		}

		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				_isExpanded = value;

				OnPropertyChanged("IsExpanded");

				if (_isExpanded && _projectResource == null)
				{
					LoadProject();
				}
			}
		}

		public bool Obfuscate
		{
			get
			{
				if (_projectResource == null)
					return false;

				if (_projectResource.ObfuscateChanged)
					return _projectResource.Obfuscate;

				return _canObfuscate;
			}
			set
			{
				if (_canObfuscate == value)
				{
					_projectResource.Obfuscate = false;
					_projectResource.ObfuscateChanged = false;
				}
				else
				{
					_projectResource.Obfuscate = value;
					_projectResource.ObfuscateChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("Obfuscate");
			}
		}

		public bool CanObfuscate
		{
			get { return _canObfuscate; }
		}

		#endregion

		#region Methods

		protected override void OnProjectChanged()
		{
			if (_isChanged)
				return;

			_isChanged = true;

			var projectResources = Node.ProjectAssembly.Resources;
			if (!projectResources.ContainsKey(_resource.Name))
			{
				projectResources.Add(_resource.Name, _projectResource);
			}

			base.OnProjectChanged();
		}

		private void LoadProject()
		{
			var projectResources = Node.ProjectAssembly.Resources;
			if (!projectResources.TryGetValue(_resource.Name, out _projectResource))
			{
				_projectResource = new AD.ProjectResource();
			}

			OnPropertyChanged("Obfuscate");
			OnPropertyChanged("MapResourceName");
		}

		#endregion
	}
}
