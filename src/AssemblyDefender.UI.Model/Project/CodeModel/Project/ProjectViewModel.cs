using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.UI;
using AssemblyDefender.Common;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
    public class ProjectViewModel : NodeViewModel
    {
        #region Fields

        public event EventHandler<DataEventArgs<Assembly>> AssemblyAdded;
        public event EventHandler<DataEventArgs<Assembly>> AssemblyRemoved;
        private AD.Project _project;
        private AssemblyManager _assemblyManager;
        private Dictionary<string, AD.ProjectAssembly> _assemblyByFilePath = new Dictionary<string, AD.ProjectAssembly>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Ctors

        public ProjectViewModel(AD.Project project, ViewModel parent)
            : base(parent)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            _project = project;

            Load();
        }

        #endregion

        #region Properties

        public AD.Project ProjectNode
        {
            get { return _project; }
        }

        public override bool CanCollapse
        {
            get { return false; }
        }

        public override bool HasChildren
        {
            get { return _assemblyByFilePath.Count > 0; }
        }

        public override NodeViewModelType NodeType
        {
            get { return NodeViewModelType.Project; }
        }

        #endregion

        #region Methods

        public override NodeLink ToLink()
        {
            return new ProjectLink();
        }

        public AssemblyViewModel FindAssembly(Assembly assembly)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                var nodeViewModel = GetChild(i);
                if (nodeViewModel.NodeType == NodeViewModelType.Assembly)
                {
                    var assemblyViewModel = (AssemblyViewModel)nodeViewModel;
                    if (object.ReferenceEquals(assembly, assemblyViewModel.Assembly))
                        return assemblyViewModel;
                }
            }

            return null;
        }

        public AssemblyViewModel FindAssembly(string filePath)
        {
            for (int i = 0; i < ChildCount; i++)
            {
                var nodeViewModel = GetChild(i);
                if (nodeViewModel.NodeType == NodeViewModelType.Assembly)
                {
                    var assemblyViewModel = (AssemblyViewModel)nodeViewModel;
                    if (0 == string.Compare(assemblyViewModel.ProjectAssembly.FilePath, filePath, true))
                        return assemblyViewModel;
                }
            }

            return null;
        }

        public ModuleViewModel FindModule(Module module)
        {
            var assemblyViewModel = FindAssembly(module.Assembly);
            if (assemblyViewModel == null)
                return null;

            return assemblyViewModel.FindModule(module);
        }

        public NamespaceViewModel FindNamespace(Module module, string ns)
        {
            var moduleViewModel = FindModule(module);
            if (moduleViewModel == null)
                return null;

            return moduleViewModel.FindNamespace(ns);
        }

        public TypeViewModel FindType(TypeDeclaration type)
        {
            var enclosingType = type.GetEnclosingType();
            if (enclosingType != null)
            {
                var enclosingTypeViewModel = FindType(enclosingType);
                if (enclosingTypeViewModel == null)
                    return null;

                return enclosingTypeViewModel.FindType(type);
            }
            else
            {
                var namespaceViewModel = FindNamespace(type.Module, type.Namespace ?? "");
                if (namespaceViewModel == null)
                    return null;

                return namespaceViewModel.FindType(type);
            }
        }

        public MethodViewModel FindMethod(MethodDeclaration method)
        {
            var type = method.GetOwnerType();

            var typeViewModel = FindType(type);
            if (typeViewModel == null)
                return null;

            return typeViewModel.FindMethod(method);
        }

        public FieldViewModel FindField(FieldDeclaration field)
        {
            var type = field.GetOwnerType();

            var typeViewModel = FindType(type);
            if (typeViewModel == null)
                return null;

            return typeViewModel.FindField(field);
        }

        public PropertyViewModel FindProperty(PropertyDeclaration property)
        {
            var type = property.GetOwnerType();

            var typeViewModel = FindType(type);
            if (typeViewModel == null)
                return null;

            return typeViewModel.FindProperty(property);
        }

        public EventViewModel FindEvent(EventDeclaration eventDecl)
        {
            var type = eventDecl.GetOwnerType();

            var typeViewModel = FindType(type);
            if (typeViewModel == null)
                return null;

            return typeViewModel.FindEvent(eventDecl);
        }

        public ResourceFolderViewModel FindResourceFolder(Assembly assembly)
        {
            var assemblyViewModel = FindAssembly(assembly);
            if (assemblyViewModel == null)
                return null;

            return assemblyViewModel.FindResourceFolder();
        }

        public void ShowDetails()
        {
            ShowSection(new ProjectDetailsViewModel(this));
        }

        public void AddAssembly(string filePath)
        {
            if (ShellUtils.IsProjectFile(filePath))
            {
                var list = new MSBuildAssemblyList(filePath);
                foreach (var msbuildAssembly in list)
                {
                    AddAssembly(msbuildAssembly);
                }
            }
            else
            {
                DoAddAssembly(filePath);
            }
        }

        public void DoAddAssembly(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            if (_assemblyByFilePath.ContainsKey(filePath))
            {
                AppService.UI.ShowMessageDialog(
                    string.Format(SR.AssemblyFileDuplicate, filePath),
                    MessageDialogType.Error);

                return;
            }

            var projectAssembly = new AD.ProjectAssembly();
            projectAssembly.FilePath = filePath;
            _project.Assemblies.Add(projectAssembly);

            OnProjectChanged();

            var assembly = _assemblyManager.LoadAssembly(filePath);
            var assemblyViewModel = new AssemblyViewModel(assembly, projectAssembly, this);

            AddAssembly(assemblyViewModel);
            _assemblyByFilePath.Add(filePath, projectAssembly);
        }

        private void AddAssembly(MSBuildAssembly msbuildAssembly)
        {
            string filePath = msbuildAssembly.FilePath;
            if (_assemblyByFilePath.ContainsKey(filePath))
                return;

            var projectAssembly = new AD.ProjectAssembly();
            projectAssembly.FilePath = filePath;

            if (msbuildAssembly.Sign)
            {
                projectAssembly.Sign = new AD.ProjectAssemblySign()
                {
                    DelaySign = msbuildAssembly.DelaySign,
                    KeyFile = msbuildAssembly.KeyFilePath,
                    Password = msbuildAssembly.KeyPassword,
                };
            }

            _project.Assemblies.Add(projectAssembly);

            OnProjectChanged();

            var assembly = _assemblyManager.LoadAssembly(filePath);
            var assemblyViewModel = new AssemblyViewModel(assembly, projectAssembly, this);

            AddAssembly(assemblyViewModel);
            _assemblyByFilePath.Add(filePath, projectAssembly);

        }

        private void AddAssembly(AssemblyViewModel assemblyViewModel)
        {
            var comparer = StringLogicalComparer.Default;
            int index = 0;
            for (int i = 0; i < ChildCount; i++)
            {
                var existingViewModel = (AssemblyViewModel)GetChild(i);
                if (0 > comparer.Compare(assemblyViewModel.Caption, existingViewModel.Caption))
                    break;

                index++;
            }

            InsertChild(index, assemblyViewModel);
            ProjectShell.OnHierarchyChanged();
            FireAssemblyAdded(assemblyViewModel.Assembly);
        }

        public bool RemoveAssembly(Assembly assembly, bool ask)
        {
            var assemblyViewModel = FindAssembly(assembly);
            if (assemblyViewModel == null)
                return false;

            return RemoveAssembly(assemblyViewModel, ask);
        }

        public bool RemoveAssembly(AssemblyViewModel assemblyViewModel, bool ask)
        {
            if (ask)
            {
                bool? result = AppService.UI.ShowMessageDialog(
                    string.Format(SR.AskRemoveAssembly, assemblyViewModel.Caption),
                    MessageDialogType.Question,
                    false);

                if (!result.HasValue || !result.Value)
                    return false;
            }

            int index = assemblyViewModel.Index;

            var projectAssembly = assemblyViewModel.ProjectAssembly;
            _project.Assemblies.Remove(projectAssembly);

            string filePath = projectAssembly.FilePath;
            _assemblyByFilePath.Remove(filePath);
            RemoveChild(assemblyViewModel);
            OnProjectChanged();
            ProjectShell.OnHierarchyChanged();

            if (ProjectShell.SelectedNode == assemblyViewModel)
            {
                if (index > 0)
                    GetChild(index - 1).Show();
                else
                    Show();
            }

            FireAssemblyRemoved(assemblyViewModel.Assembly);

            return true;
        }

        public void OpenInWindowsExplorer()
        {
            if (ProjectShell.IsNew)
                return;

            AppService.SelectFileInWindowsExplorer(ProjectShell.FilePath);
        }

        protected override void OnActivate()
        {
            ShowDetails();
            base.OnActivate();
        }

        protected override void LoadChildren(List<NodeViewModel> children)
        {
            foreach (var projectAssembly in _assemblyByFilePath.Values)
            {
                if (string.IsNullOrEmpty(projectAssembly.FilePath))
                    continue;

                var assembly = _assemblyManager.LoadAssembly(projectAssembly.FilePath);
                var assemblyViewModel = new AssemblyViewModel(assembly, projectAssembly, this);
                children.Add(assemblyViewModel);
            }

            children.Sort(NodeComparer.Default);
        }

        protected override NodeMenu CreateMenu()
        {
            return new ProjectMenu();
        }

        private void Load()
        {
            if (string.IsNullOrEmpty(ProjectShell.FilePath))
                Caption = SR.NewProjectCaption;
            else
                Caption = Path.GetFileNameWithoutExtension(ProjectShell.FilePath);

            Image = ImageType.Project;

            _assemblyManager = new AssemblyManager();

            // Load children.
            for (int i = _project.Assemblies.Count - 1; i >= 0; i--)
            {
                var projectAssembly = _project.Assemblies[i];

                string filePath = projectAssembly.FilePath;
                if (string.IsNullOrEmpty(filePath) || _assemblyByFilePath.ContainsKey(filePath))
                {
                    _project.Assemblies.RemoveAt(i);
                    continue;
                }

                _assemblyByFilePath.Add(filePath, projectAssembly);
            }
        }

        private void FireAssemblyAdded(Assembly assembly)
        {
            var handler = AssemblyAdded;
            if (handler != null)
            {
                handler(this, new DataEventArgs<Assembly>(assembly));
            }
        }

        private void FireAssemblyRemoved(Assembly assembly)
        {
            var handler = AssemblyRemoved;
            if (handler != null)
            {
                handler(this, new DataEventArgs<Assembly>(assembly));
            }
        }

        #endregion
    }
}
