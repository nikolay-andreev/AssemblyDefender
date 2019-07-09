using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class HistoryNavigator
	{
		#region Fields

		private const int MaxSize = 500;
		private const int CleanBufferSize = 100;
		private bool _frozen;
		private int _index;
		private List<NodeLink> _links = new List<NodeLink>();
		private ProjectShellViewModel _shellViewModel;

		#endregion

		#region Ctors

		internal HistoryNavigator(ProjectShellViewModel shellViewModel)
		{
			_shellViewModel = shellViewModel;
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool HasForward
		{
			get { return _index < _links.Count - 1; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool HasBack
		{
			get { return _index > 0; }
		}

		#endregion

		#region Methods

		public void Add(NodeViewModel nodeViewModel)
		{
			if (nodeViewModel == null)
				throw new ArgumentNullException("nodeViewModel");

			if (_frozen)
				return;

			var link = nodeViewModel.ToLink();
			if (link == null)
				return;

			if (HasForward)
			{
				// Clear forward entries.
				_links.RemoveRange(_index + 1, _links.Count - _index - 1);
			}

			bool duplicate = (_links.Count > 0) && (_links[_links.Count - 1].Equals(link));
			if (!duplicate)
			{
				_links.Add(link);

				if (_links.Count > MaxSize + CleanBufferSize)
				{
					_links.RemoveRange(0, _links.Count - MaxSize);
				}
			}

			_index = _links.Count - 1;

			Commands.GoForward.RaiseCanExecuteChanged();
			Commands.GoBack.RaiseCanExecuteChanged();
		}

		public void Clear()
		{
			_index = 0;
			_links.Clear();

			Commands.GoForward.RaiseCanExecuteChanged();
			Commands.GoBack.RaiseCanExecuteChanged();
		}

		public void Forward()
		{
			if (_frozen)
				return;

			if (!Show(++_index))
			{
				_links.RemoveAt(_index);
				if (_index > 0)
					_index--;
			}

			Commands.GoForward.RaiseCanExecuteChanged();
			Commands.GoBack.RaiseCanExecuteChanged();
		}

		public void Back()
		{
			if (_frozen)
				return;

			if (!Show(--_index))
			{
				_links.RemoveAt(_index);
			}

			Commands.GoForward.RaiseCanExecuteChanged();
			Commands.GoBack.RaiseCanExecuteChanged();
		}

		private bool Show(int index)
		{
			var link = _links[index];

			var selectedViewModel = _shellViewModel.SelectedNode;

			try
			{
				_frozen = true;

				link.Show(_shellViewModel.Project);
			}
			finally
			{
				_frozen = false;
			}

			if (!IsSelected(selectedViewModel, link))
				return false;

			return true;
		}

		private bool IsSelected(NodeViewModel oldViewModel, NodeLink link)
		{
			if (object.ReferenceEquals(_shellViewModel.SelectedNode, oldViewModel))
				return false;

			var selectedViewModel = _shellViewModel.SelectedNode;
			if (selectedViewModel == null)
				return false;

			if (!link.Equals(selectedViewModel.ToLink()))
				return false;

			return true;
		}

		#endregion
	}
}
