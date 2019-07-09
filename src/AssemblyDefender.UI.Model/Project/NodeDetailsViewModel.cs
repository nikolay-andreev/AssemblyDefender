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
using System.ComponentModel;
using System.Reflection;

namespace AssemblyDefender.UI.Model.Project
{
	public class NodeDetailsViewModel<T> : ViewModel
		where T : NodeViewModel
	{
		#region Fields

		private T _node;

		#endregion

		#region Ctors

		internal NodeDetailsViewModel(T node)
			: base(node)
		{
			_node = node;
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Caption
		{
			get { return _node.Caption; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ImageType Image
		{
			get { return _node.Image; }
		}

		public T Node
		{
			get { return _node; }
		}

		public new T Parent
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
			_node = null;
		}

		protected void OnProjectChanged()
		{
			_node.OnProjectChanged();
		}

		#endregion
	}
}
