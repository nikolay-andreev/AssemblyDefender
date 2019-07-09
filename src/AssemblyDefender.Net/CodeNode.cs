using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AssemblyDefender.Net
{
	public abstract class CodeNode
	{
		#region Fields

		protected bool _isNew;
		protected bool _isChanged;
		protected Module _module;
		protected CodeNode _parent;

		#endregion

		#region Ctors

		protected CodeNode()
		{
		}

		protected CodeNode(CodeNode parent)
		{
			_parent = parent;
			_module = parent._module;
		}

		#endregion

		#region Properties

		public bool IsNew
		{
			get { return _isNew; }
			protected set
			{
				_isNew = value;
				_isChanged = value;
			}
		}

		public bool IsChanged
		{
			get { return _isChanged; }
		}

		public Module Module
		{
			get { return _module; }
		}

		public Assembly Assembly
		{
			get { return _module.Assembly; }
		}

		public AssemblyManager AssemblyManager
		{
			get { return _module.Assembly.AssemblyManager; }
		}

		#endregion

		#region Methods

		public void MakeDirty()
		{
			OnChanged();
		}

		protected internal virtual void OnChanged()
		{
			if (!_isChanged)
			{
				_isChanged = true;
				_parent.OnChanged();
			}
		}

		#endregion
	}
}
