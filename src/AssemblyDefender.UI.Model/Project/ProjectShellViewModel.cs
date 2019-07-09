using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;
using AssemblyDefender.UI;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
    public class ProjectShellViewModel : ViewModel
    {
        #region Fields

        public event EventHandler HierarchyChanged;
        private bool _isChanged;
        private string _filePath;
        private AD.Project _project;
        private ViewModel _selectedWorkspace;
        private NodeViewModel _selectedNode;
        private ToolViewModel _selectedTool;
        private ProjectViewModel _projectViewModel;
        private ShellViewModel _shell;
        private HistoryNavigator _navigator;
        private NavigationCollection _nodes = new NavigationCollection();
        private NodeMenu[] _nodeMenus = new NodeMenu[(int)NodeViewModelType.Last + 1];
        private EventTriggering _eventTriggering = EventTriggering.TriggerAll;

        #endregion

        #region Ctors

        private ProjectShellViewModel(string filePath, AD.Project project, ViewModel parent)
            : base(parent)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            _project = project;
            _filePath = filePath;
            _isChanged = string.IsNullOrEmpty(_filePath);
            _navigator = new HistoryNavigator(this);
            _shell = FindParent<ShellViewModel>(true);
        }

        #endregion

        #region Properties

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public bool IsNew
        {
            get { return string.IsNullOrEmpty(_filePath); }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public string FilePath
        {
            get { return _filePath; }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public ViewModel SelectedWorkspace
        {
            get { return _selectedWorkspace; }
            set { Show<ViewModel>(ref _selectedWorkspace, value, "SelectedWorkspace"); }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public NodeViewModel SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (Show<NodeViewModel>(ref _selectedNode, value, "SelectedNode"))
                {
                    if (_selectedWorkspace != null)
                    {
                        _navigator.Add(_selectedNode);
                    }
                }
            }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public ToolViewModel SelectedTool
        {
            get { return _selectedTool; }
            set { Show<ToolViewModel>(ref _selectedTool, value, "SelectedTool"); }
        }

        public ProjectViewModel Project
        {
            get { return _projectViewModel; }
        }

        public ShellViewModel Shell
        {
            get { return _shell; }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public NavigationCollection Nodes
        {
            get { return _nodes; }
        }

        [System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
        public bool HasAssemblies
        {
            get { return _project.Assemblies.Count > 0; }
        }

        internal NodeMenu[] NodeMenus
        {
            get { return _nodeMenus; }
        }

        #endregion

        #region Methods

        public void AddAssembly()
        {
            AppService.UI.ShowAppWait();

            string[] files = AppService.UI.ShowOpenFilesDialog(
                Constants.AddAssemblyFileFilter,
                SR.OpenFileCaption);

            if (files == null)
                return;

            try
            {
                _eventTriggering = EventTriggering.DoNotTriggerHierarchyChanged;

                for (int i = 0; i < files.Length; i++)
                {
                    _projectViewModel.AddAssembly(files[i]);
                }
            }
            finally
            {
                _eventTriggering = EventTriggering.TriggerAll;
            }

            OnHierarchyChanged();
        }

        public void RemoveSelectedAssembly()
        {
            var assemblyViewModel = SelectedNode as AssemblyViewModel;
            if (assemblyViewModel == null)
                return;

            _projectViewModel.RemoveAssembly(assemblyViewModel, true);
        }

        public bool CanRemoveSelectedAssembly()
        {
            return SelectedNode is AssemblyViewModel;
        }

        public void Save(bool saveAs)
        {
            AppService.UI.ShowAppWait();

            if (saveAs || string.IsNullOrEmpty(_filePath))
            {
                _filePath = AppService.UI.ShowSaveFileDialog(Constants.ProjectFileFilter);
                if (string.IsNullOrEmpty(_filePath))
                    return;

                _shell.ProjectMRUList.Add(_filePath);
            }

            if (_selectedNode != null && _selectedNode.IsChanged)
            {
                _selectedNode.AddProjectNode();
            }

            _project.Scavenge();

            try
            {
                _project.SaveFile(_filePath);
            }
            catch (Exception ex)
            {
                AppService.UI.ShowMessageDialog(ex.Message, MessageDialogType.Error);
                return;
            }

            // Update project caption.
            _projectViewModel.Caption = Path.GetFileNameWithoutExtension(_filePath);

            // Update main window caption.
            _shell.WindowTitle = _projectViewModel.Caption;

            AcceptChanges();
        }

        public bool CanSave(bool saveAs)
        {
            return saveAs || _isChanged;
        }

        public void Build()
        {
            if (_isChanged)
            {
                Save(false);

                if (_isChanged)
                    return;
            }

            var viewModel = new BuildViewModel(this);
            AppService.UI.ShowBuild(viewModel);
        }

        public void Refresh()
        {
            if (!string.IsNullOrEmpty(_filePath) && !_isChanged)
            {
                var project = LaodProject(_filePath, true);
                if (project == null)
                    return;

                _project = project;
            }

            try
            {
                _eventTriggering = EventTriggering.DoNotTriggerHierarchyChanged;

                _projectViewModel = new ProjectViewModel(_project, this);
                _nodes.Clear();
                _nodes.Add(_projectViewModel);
                _projectViewModel.Expand();
                _projectViewModel.IsSelected = true;
                _navigator.Clear();
            }
            finally
            {
                _eventTriggering = EventTriggering.TriggerAll;
            }

            OnHierarchyChanged();
        }

        public bool CanGoBack()
        {
            return _navigator.HasBack;
        }

        public void GoBack()
        {
            _navigator.Back();
        }

        public bool CanGoForward()
        {
            return _navigator.HasForward;
        }

        public void GoForward()
        {
            _navigator.Forward();
        }

        public void ExpandAll()
        {
            try
            {
                _nodes.DoNotTriggerEvents = true;

                ExpandAll(_projectViewModel);
            }
            finally
            {
                _nodes.DoNotTriggerEvents = false;
                _nodes.OnCollectionReset();
            }
        }

        public void ExpandAll(NodeViewModel node)
        {
            if (!node.HasChildren)
                return;

            if (!node.IsExpanded)
            {
                node.Expand();
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                ExpandAll(node.GetChild(i));
            }
        }

        public void CollapseAll()
        {
            try
            {
                _nodes.DoNotTriggerEvents = true;

                for (int i = 0; i < _projectViewModel.ChildCount; i++)
                {
                    var childNode = _projectViewModel.GetChild(i);
                    childNode.Collapse();
                }
            }
            finally
            {
                _nodes.DoNotTriggerEvents = false;
                _nodes.OnCollectionReset();
            }
        }

        public void ShowSearch()
        {
            if (_selectedTool != null && _selectedTool is SearchViewModel)
                return;

            SelectedTool = new SearchViewModel(this);
        }

        public void ShowDecodeStackTrace()
        {
            if (_selectedTool != null && _selectedTool is DecodeStackTraceViewModel)
                return;

            SelectedTool = new DecodeStackTraceViewModel(this);
        }

        internal void AcceptChanges()
        {
            _isChanged = false;

            if (_projectViewModel != null)
            {
                _projectViewModel.AcceptChanges();
            }

            if (_selectedNode != null)
            {
                _selectedNode.AcceptChanges();
            }

            Commands.Save.RaiseCanExecuteChanged();
        }

        internal void OnProjectChanged()
        {
            if (_isChanged)
                return;

            _isChanged = true;
            Commands.Save.RaiseCanExecuteChanged();
        }

        internal void OnHierarchyChanged()
        {
            if ((_eventTriggering & EventTriggering.DoNotTriggerHierarchyChanged) == EventTriggering.DoNotTriggerHierarchyChanged)
                return;

            if (HierarchyChanged != null)
            {
                HierarchyChanged(this, EventArgs.Empty);
            }
        }

        public override bool CanDeactivate()
        {
            if (!_isChanged)
                return true;

            bool? result = AppService.UI.ShowMessageDialog(SR.AskSaveChanges, MessageDialogType.Question, true);
            if (result.HasValue)
            {
                if (result.Value)
                {
                    // Yes
                    Save(false);
                    return true;
                }
                else
                {
                    // No
                    return true;
                }
            }
            else
            {
                // Cancel
                return false;
            }
        }

        protected override void OnActivate()
        {
            _eventTriggering = EventTriggering.DoNotTriggerHierarchyChanged;

            if (!IsNew)
            {
                _shell.ProjectMRUList.Add(_filePath);
            }

            StrongNamePasswordCache.AddKeys(_project);

            // Attache events.
            Commands.AddAssembly.Subscribe(AddAssembly);
            Commands.RemoveAssembly.Subscribe(RemoveSelectedAssembly, CanRemoveSelectedAssembly);
            Commands.Save.Subscribe(Save, CanSave);
            Commands.Build.Subscribe(Build);
            Commands.Refresh.Subscribe(Refresh);
            Commands.GoBack.Subscribe(GoBack, CanGoBack);
            Commands.GoForward.Subscribe(GoForward, CanGoForward);
            Commands.ExpandAll.Subscribe(ExpandAll);
            Commands.CollapseAll.Subscribe(CollapseAll);
            Commands.ViewSearch.Subscribe(ShowSearch);
            Commands.ViewDecodeStackTrace.Subscribe(ShowDecodeStackTrace);

            // Add and show project view.
            _projectViewModel = new ProjectViewModel(_project, this);
            _nodes.Add(_projectViewModel);
            _projectViewModel.Expand();
            _projectViewModel.IsSelected = true;

            // Update main window caption.
            _shell.WindowTitle = _projectViewModel.Caption;

            _eventTriggering = EventTriggering.TriggerAll;

            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            // Detach events.
            Commands.AddAssembly.Unsubscribe(AddAssembly);
            Commands.RemoveAssembly.Unsubscribe(RemoveSelectedAssembly);
            Commands.Save.Unsubscribe(Save);
            Commands.Build.Unsubscribe(Build);
            Commands.Refresh.Unsubscribe(Refresh);
            Commands.GoBack.Unsubscribe(GoBack);
            Commands.GoForward.Unsubscribe(GoForward);
            Commands.ExpandAll.Unsubscribe(ExpandAll);
            Commands.CollapseAll.Unsubscribe(CollapseAll);
            Commands.ViewSearch.Unsubscribe(ShowSearch);

            base.OnDeactivate();
        }

        #endregion

        #region Static

        public static ProjectShellViewModel LaodFile(string filePath, ViewModel parent)
        {
            filePath = PathUtils.MakeAbsolutePath(filePath);

            var project = LaodProject(filePath, true);
            if (project == null)
                return null;

            return new ProjectShellViewModel(filePath, project, parent);
        }

        public static ProjectShellViewModel CreateNew(ViewModel parent)
        {
            var project = new AD.Project();

            return new ProjectShellViewModel(null, project, parent);
        }

        private static AD.Project LaodProject(string filePath, bool showErrorMessage = false)
        {
            if (!File.Exists(filePath))
            {
                if (showErrorMessage)
                {
                    AppService.UI.ShowMessageDialog(
                        string.Format(Common.SR.FileNotFound, filePath),
                        MessageDialogType.Error);
                }

                return null;
            }

            var project = AD.Project.LoadFile(filePath);
            if (project == null)
            {
                if (showErrorMessage)
                {
                    AppService.UI.ShowMessageDialog(
                        string.Format(AssemblyDefender.SR.ProjectFileNotValid, filePath),
                        MessageDialogType.Error);
                }

                return null;
            }

            return project;
        }

        #endregion

        #region Nested types

        /// <summary>
        /// Flags for specifying which events to stop triggering.
        /// </summary>
        [Flags]
        private enum EventTriggering
        {
            TriggerAll = 0,
            DoNotTriggerHierarchyChanged = 1,
        }

        #endregion
    }
}
