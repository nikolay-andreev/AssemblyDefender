using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;
using AD = AssemblyDefender;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class FieldDetailsViewModel : NodeDetailsViewModel<FieldViewModel>
	{
		#region Fields

		private bool _isVisibleOutsideAssembly;
		private FieldDeclaration _field;
		private AD.ProjectField _projectField;
		private NodeProperties _parentProperties;

		#endregion

		#region Ctors

		public FieldDetailsViewModel(FieldViewModel parent)
			: base(parent)
		{
			_projectField = Parent.ProjectField;

			_field = Parent.Field;
			_isVisibleOutsideAssembly = _field.IsVisibleOutsideAssembly();

			_parentProperties = new NodeProperties();
			_parentProperties.LoadParent(Parent);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Name
		{
			get
			{
				return _projectField.NameChanged ? _projectField.Name : _field.Name;
			}
			set
			{
				string name = (value ?? "").Trim();
				if (string.IsNullOrEmpty(name))
				{
					throw new ShellException(SR.EmptyStringNotValid);
				}

				_projectField.Name = name;
				OnProjectChanged();
				OnPropertyChanged("Name");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNameChanged
		{
			get { return _projectField.NameChanged; }
			set
			{
				if (_projectField.NameChanged == value)
					return;

				_projectField.NameChanged = value;

				if (value)
				{
					_projectField.Name = _field.Name;
				}

				OnProjectChanged();
				OnPropertyChanged("Name");
				OnPropertyChanged("IsNameChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool Rename
		{
			get
			{
				if (!CanRename)
					return false;

				if (_projectField.RenameChanged)
					return _projectField.Rename;

				return _parentProperties.RenameMembers;
			}
			set
			{
				if (_parentProperties.RenameMembers == value)
				{
					_projectField.Rename = false;
					_projectField.RenameChanged = false;
				}
				else
				{
					_projectField.Rename = value;
					_projectField.RenameChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("Rename");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanRename
		{
			get
			{
				if (!_parentProperties.CanRenameMembers)
					return false;

				if (!_parentProperties.RenamePublicFields && _isVisibleOutsideAssembly)
					return false;

				if (!_parentProperties.RenameEnumMembers && Node.FieldKind == FieldNodeKind.EnumItem)
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RemoveUnused
		{
			get
			{
				if (!CanRemoveUnused)
					return false;

				if (_projectField.RemoveUnusedChanged)
					return _projectField.RemoveUnused;

				return _parentProperties.RemoveUnusedMembers;
			}
			set
			{
				if (_parentProperties.RemoveUnusedMembers == value)
				{
					_projectField.RemoveUnused = false;
					_projectField.RemoveUnusedChanged = false;
				}
				else
				{
					_projectField.RemoveUnused = value;
					_projectField.RemoveUnusedChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("RemoveUnused");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanRemoveUnused
		{
			get
			{
				if (!_parentProperties.CanRemoveUnusedMembers)
					return false;

				if (!_parentProperties.RemoveUnusedPublicMembers && _isVisibleOutsideAssembly)
					return false;

				return true;
			}
		}

		#endregion
	}
}
