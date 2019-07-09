using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public abstract class NodeViewModel : ViewModel
	{
		#region Fields

		private bool _isChanged;
		private bool _isExpanded;
		private int _index;
		private int _indent;
		private int _childCount;
		private int _visibleChildCount;
		private string _caption;
		private ImageType _image;
		private ProjectShellViewModel _projectShell;

		#endregion

		#region Ctors

		internal NodeViewModel(ViewModel parent)
			: base(parent)
		{
			var parentNode = parent as NodeViewModel;
			if (parentNode != null)
			{
				_indent = parentNode._indent + 1;
				_projectShell = parentNode._projectShell;
			}
			else
			{
				// Project
				_indent = 0;
				_projectShell = (ProjectShellViewModel)parent;
			}
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsChanged
		{
			get { return _isChanged; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				if (value)
					Expand();
				else
					Collapse();
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsSelected
		{
			get { return IsActive; }
			set
			{
				if (value)
				{
					ProjectShell.SelectedNode = this;
				}
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public virtual bool CanCollapse
		{
			get { return true; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public virtual bool HasChildren
		{
			get { return false; }
		}

		/// <summary>
		/// Gets an index into parent node.
		/// </summary>
		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public int Index
		{
			get { return _index; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public int Indent
		{
			get { return _indent; }
		}

		public int ChildCount
		{
			get { return _childCount; }
		}

		public int VisibleChildCount
		{
			get { return _visibleChildCount; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Caption
		{
			get { return _caption; }
			set
			{
				_caption = value;
				OnPropertyChanged("Caption");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ImageType Image
		{
			get { return _image; }
			set
			{
				_image = value;
				OnPropertyChanged("Image");
			}
		}

		public ShellViewModel Shell
		{
			get { return _projectShell.FindParent<ShellViewModel>(); }
		}

		public ProjectShellViewModel ProjectShell
		{
			get { return _projectShell; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public NodeMenu Menu
		{
			get
			{
				var projectShell = ProjectShell;
				int index = (int)NodeType;
				var menu = projectShell.NodeMenus[index];
				if (menu == null)
				{
					menu = CreateMenu();
					projectShell.NodeMenus[index] = menu;
				}

				return menu;
			}
		}

		public abstract NodeViewModelType NodeType
		{
			get;
		}

		#endregion

		#region Methods

		public void Show()
		{
			AppService.UI.ScrollProjectNodeIntoView(this);
			IsSelected = true;
		}

		public int GetPosition()
		{
			var parentNode = Parent as NodeViewModel;
			if (parentNode == null)
				return 0;

			return parentNode.GetChildPosition(_index);
		}

		public NodeViewModel GetChild(int index)
		{
			return _projectShell.Nodes[GetChildPosition(index)];
		}

		public T GetChild<T>(int index)
			where T : NodeViewModel
		{
			return (T)_projectShell.Nodes[GetChildPosition(index)];
		}

		public T FindChild<T>()
			where T : NodeViewModel
		{
			for (int i = 0; i < _childCount; i++)
			{
				var childNode = GetChild(i);
				if (childNode is T)
					return (T)childNode;
			}

			return null;
		}

		public void InsertChild(int index, NodeViewModel nodeViewModel)
		{
			int pos;
			if (_childCount > 0 && index > 0)
			{
				var prevNode = GetChild(index - 1);
				pos = prevNode.GetPosition() + prevNode.VisibleChildCount + 1;
			}
			else
			{
				pos = GetPosition() + 1;
			}

			_projectShell.Nodes.Insert(pos, nodeViewModel);
			nodeViewModel._index = index;

			_childCount++;

			for (int i = index + 1; i < _childCount; i++)
			{
				var nextChild = GetChild(i);
				nextChild._index = i;
			}
		}

		public void RemoveChild(int index)
		{
			var node = GetChild(index);
			RemoveChild(node);
		}

		public void RemoveChild(NodeViewModel childNode)
		{
			int index = childNode.Index;
			int pos = childNode.GetPosition();
			int count = childNode.VisibleChildCount + 1;
			_projectShell.Nodes.RemoveRange(pos, count);

			_childCount--;

			for (int i = index; i < _childCount; i++)
			{
				var nextChild = GetChild(i);
				nextChild._index = i;
			}
		}

		public virtual void Expand()
		{
			if (_isExpanded)
				return;

			if (HasChildren)
			{
				AppService.UI.ShowAppWait();
				LoadChildren();
			}

			_isExpanded = true;
			OnPropertyChanged("IsExpanded");
		}

		public virtual void Collapse()
		{
			if (!_isExpanded)
				return;

			if (!CanCollapse)
				return;

			if (HasChildren)
			{
				UnloadChildren();
			}

			_isExpanded = false;
			OnPropertyChanged("IsExpanded");
		}

		public abstract NodeLink ToLink();

		protected internal virtual void AddProjectNode()
		{
		}

		protected override void OnActivate()
		{
			Menu.Load(this);

			OnPropertyChanged("IsSelected");

			base.OnActivate();
		}

		protected override void OnDeactivate()
		{
			ProjectShell.SelectedWorkspace = null;

			Menu.Unload();

			_isChanged = false;

			OnPropertyChanged("IsSelected");

			base.OnDeactivate();
		}

		protected void ShowSection(ViewModel viewModel)
		{
			ProjectShell.SelectedWorkspace = viewModel;
		}

		internal void AcceptChanges()
		{
			_isChanged = false;
		}

		internal void OnProjectChanged()
		{
			if (_isChanged)
				return;

			_isChanged = true;
			ProjectShell.OnProjectChanged();
		}

		protected abstract NodeMenu CreateMenu();

		internal int GetChildPosition(int index)
		{
			int pos = GetPosition();
			for (int i = 0; i < index; i++)
			{
				var childNode = _projectShell.Nodes[++pos];
				pos += childNode.VisibleChildCount;
			}

			return ++pos;
		}

		protected virtual void LoadChildren(List<NodeViewModel> children)
		{
			Debug.Fail("Should not be called.");
		}

		private void LoadChildren()
		{
			var children = new List<NodeViewModel>();
			LoadChildren(children);

			if (children.Count == 0)
				return;

			// Set index.
			for (int i = 0; i < children.Count; i++)
			{
				children[i]._index = i;
			}

			// Add
			int pos = GetPosition();
			_projectShell.Nodes.InsertRange(pos + 1, children);
			_childCount = children.Count;
			IncreaseVisibleChildCount(_childCount);
		}

		private void UnloadChildren()
		{
			if (_childCount == 0)
				return;

			int pos = GetPosition();
			int count = CalculateVisibleChildCount(pos);
			_projectShell.Nodes.RemoveRange(pos + 1, count);
			DecreaseVisibleChildCount(count);
			_childCount = 0;
		}

		private int CalculateVisibleChildCount(int startPos)
		{
			int pos = startPos;
			for (int i = 0; i < _childCount; i++)
			{
				var childNode = _projectShell.Nodes[++pos];
				pos += childNode.VisibleChildCount;
			}

			return pos - startPos;
		}

		private void IncreaseVisibleChildCount(int num)
		{
			_visibleChildCount += num;

			var parent = Parent as NodeViewModel;
			if (parent != null)
			{
				parent.IncreaseVisibleChildCount(num);
			}
		}

		private void DecreaseVisibleChildCount(int num)
		{
			_visibleChildCount -= num;

			var parent = Parent as NodeViewModel;
			if (parent != null)
			{
				parent.DecreaseVisibleChildCount(num);
			}
		}

		#endregion
	}
}
