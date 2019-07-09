using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;
using AD = AssemblyDefender;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class ResourceFolderDetailsViewModel : NodeDetailsViewModel<ResourceFolderViewModel>
	{
		#region Fields

		private bool _canObfuscate;
		private ICommand _expandAllCommand;
		private List<ResourceViewModel> _resources;

		#endregion

		#region Ctors

		public ResourceFolderDetailsViewModel(ResourceFolderViewModel parent)
			: base(parent)
		{
			_canObfuscate = parent.ProjectAssembly.ObfuscateResources;
			_expandAllCommand = new DelegateCommand<bool>(ExpandAll);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanObfuscate
		{
			get { return _canObfuscate; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand ExpandAllCommand
		{
			get { return _expandAllCommand; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public List<ResourceViewModel> Resources
		{
			get { return _resources; }
		}

		#endregion

		#region Methods

		protected override void OnActivate()
		{
			var resources = Parent.Assembly.Resources;
			var projectResources = Parent.ProjectAssembly.Resources;
			_resources = new List<ResourceViewModel>(resources.Count);

			foreach (var resource in resources)
			{
				_resources.Add(new ResourceViewModel(resource, this));
			}

			_resources.Sort((x, y) => StringLogicalComparer.Default.Compare(x.Name, y.Name));

			if (_resources.Count <= 5)
			{
				ExpandAll(true);
			}
		}

		protected override void OnDeactivate()
		{
			_resources = null;
			base.OnDeactivate();
		}

		private void ExpandAll(bool isExpanded)
		{
			foreach (var resource in _resources)
			{
				resource.IsExpanded = isExpanded;
			}
		}

		#endregion
	}
}
