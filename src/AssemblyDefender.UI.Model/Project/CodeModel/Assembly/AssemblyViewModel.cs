using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Baml;
using AssemblyDefender.Net;
using AD = AssemblyDefender;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
    public class AssemblyViewModel : NodeViewModel
    {
        #region Fields

        private bool _isValid;
        private bool? _hasXaml;
        private Assembly _assembly;
        private AD.ProjectAssembly _projectAssembly;

        #endregion

        #region Ctors

        public AssemblyViewModel(Assembly assembly, AD.ProjectAssembly projectAssembly, ViewModel parent)
            : base(parent)
        {
            _projectAssembly = projectAssembly;
            _assembly = assembly;

            _isValid = IsValid();

            if (_isValid)
            {
                Caption = assembly.Name;
                Image = ImageType.Node_Assembly;
            }
            else
            {
                Image = ImageType.Node_Assembly_Invalid;
                Caption = _assembly != null ?
                    Caption = assembly.Name :
                    Caption = Path.GetFileNameWithoutExtension(projectAssembly.FilePath);
            }
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

        public bool HasXaml
        {
            get
            {
                if (!_hasXaml.HasValue)
                {
                    _hasXaml = BamlUtils.ContainsWpfResources(_assembly);
                }

                return _hasXaml.Value;
            }
        }

        public override bool HasChildren
        {
            get { return _isValid; }
        }

        public override NodeViewModelType NodeType
        {
            get { return NodeViewModelType.Assembly; }
        }

        #endregion

        #region Methods

        public override NodeLink ToLink()
        {
            return new AssemblyLink(_assembly);
        }

        public ModuleViewModel FindModule(Module module)
        {
            Expand();

            for (int i = 0; i < ChildCount; i++)
            {
                var nodeViewModel = GetChild(i);
                if (nodeViewModel.NodeType == NodeViewModelType.Module)
                {
                    var moduleViewModel = (ModuleViewModel)nodeViewModel;
                    if (object.ReferenceEquals(module, moduleViewModel.Module))
                        return moduleViewModel;
                }
            }

            return null;
        }

        public ResourceFolderViewModel FindResourceFolder()
        {
            Expand();

            for (int i = 0; i < ChildCount; i++)
            {
                var nodeViewModel = GetChild(i);
                if (nodeViewModel.NodeType == NodeViewModelType.ResourceFolder)
                    return (ResourceFolderViewModel)nodeViewModel;
            }

            return null;
        }

        public void ShowDetails()
        {
            ViewModel viewModel;
            if (_assembly == null)
            {
                if (!File.Exists(_projectAssembly.FilePath))
                    viewModel = new AssemblyInvalidViewModel(this, string.Format(Common.SR.FileNotFound, _projectAssembly.FilePath));
                else
                    viewModel = new AssemblyInvalidViewModel(this, string.Format(SR.AssemblyFileNotValid, _projectAssembly.FilePath));
            }
            else if (!_assembly.Module.Image.IsILOnly)
            {
                viewModel = new AssemblyInvalidViewModel(this, string.Format(SR.AssemblyUnmanagedNotSupported, _projectAssembly.FilePath));
            }
            else
            {
                viewModel = new AssemblyDetailsViewModel(this);
            }

            ShowSection(viewModel);
        }

        public void OpenInWindowsExplorer()
        {
            AppService.SelectFileInWindowsExplorer(_projectAssembly.FilePath);
        }

        public void Remove()
        {
            var projectViewModel = FindParent<ProjectViewModel>();
            if (projectViewModel == null)
                return;

            projectViewModel.RemoveAssembly(this, true);
        }

        protected override void OnActivate()
        {
            ShowDetails();
            base.OnActivate();
        }

        protected override void LoadChildren(List<NodeViewModel> children)
        {
            if (_assembly == null)
                return;

            // Modules
            foreach (var module in _assembly.Modules)
            {
                if (!module.Image.IsILOnly)
                    continue;

                AD.ProjectModule projectModule;
                if (!_projectAssembly.Modules.TryGetValue(module.Name, out projectModule))
                {
                    projectModule = new ProjectModule();
                }

                var moduleViewModel = new ModuleViewModel(module, projectModule, this);
                children.Add(moduleViewModel);
            }

            // Resources
            if (_assembly.Resources.Count > 0)
            {
                var resourceFolderViewModel = new ResourceFolderViewModel(_assembly, _projectAssembly, this);
                children.Add(resourceFolderViewModel);
            }
        }

        protected override NodeMenu CreateMenu()
        {
            return new AssemblyMenu();
        }

        private bool IsValid()
        {
            if (_assembly == null)
                return false;

            if (!_assembly.Module.Image.IsILOnly)
                return false;

            return true;
        }

        #endregion
    }
}
