using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.UI;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
	public class ElementViewModel<TParent, TNode> : ViewModel
		where TParent : ViewModel
		where TNode : NodeViewModel
	{
		#region Fields

		private TParent _parent;
		private TNode _node;

		#endregion

		#region Ctors

		internal ElementViewModel(TParent parent)
			: base(parent)
		{
			_parent = parent;

			var viewModel = (ViewModel)parent;
			while (viewModel != null)
			{
				_node = viewModel as TNode;
				if (_node != null)
					break;

				viewModel = viewModel.Parent;
			}
		}

		#endregion

		#region Properties

		public new TParent Parent
		{
			get { return _parent; }
		}

		public TNode Node
		{
			get { return _node; }
		}

		public ShellViewModel Shell
		{
			get { return FindParent<ShellViewModel>(); }
		}

		public ProjectShellViewModel ProjectShell
		{
			get { return FindParent<ProjectShellViewModel>(); }
		}

		#endregion

		#region Methods

		protected override void OnDispose()
		{
			base.OnDispose();
			_parent = null;
			_node = null;
		}

		protected virtual void OnProjectChanged()
		{
			_node.OnProjectChanged();
		}

		#endregion
	}
}
