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
	public class PropertyDetailsViewModel : NodeDetailsViewModel<PropertyViewModel>
	{
		#region Fields

		private bool _isVisibleOutsideAssembly;
		private PropertyDeclaration _property;
		private AD.ProjectProperty _projectProperty;
		private NodeProperties _parentProperties;

		#endregion

		#region Ctors

		public PropertyDetailsViewModel(PropertyViewModel parent)
			: base(parent)
		{
			_projectProperty = Parent.ProjectProperty;

			_property = Parent.Property;
			_isVisibleOutsideAssembly = _property.IsVisibleOutsideAssembly();

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
				return _projectProperty.NameChanged ? _projectProperty.Name : _property.Name;
			}
			set
			{
				string name = (value ?? "").Trim();
				if (string.IsNullOrEmpty(name))
				{
					throw new ShellException(SR.EmptyStringNotValid);
				}

				_projectProperty.Name = name;
				OnProjectChanged();
				OnPropertyChanged("Name");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNameChanged
		{
			get { return _projectProperty.NameChanged; }
			set
			{
				if (_projectProperty.NameChanged == value)
					return;

				_projectProperty.NameChanged = value;

				if (value)
				{
					_projectProperty.Name = _property.Name;
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

				if (_projectProperty.RenameChanged)
					return _projectProperty.Rename;

				return _parentProperties.RenameMembers;
			}
			set
			{
				if (_parentProperties.RenameMembers == value)
				{
					_projectProperty.Rename = false;
					_projectProperty.RenameChanged = false;
				}
				else
				{
					_projectProperty.Rename = value;
					_projectProperty.RenameChanged = true;
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

				if (!_parentProperties.RenamePublicProperties && _isVisibleOutsideAssembly)
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

				if (_projectProperty.RemoveUnusedChanged)
					return _projectProperty.RemoveUnused;

				return _parentProperties.RemoveUnusedMembers;
			}
			set
			{
				if (_parentProperties.RemoveUnusedMembers == value)
				{
					_projectProperty.RemoveUnused = false;
					_projectProperty.RemoveUnusedChanged = false;
				}
				else
				{
					_projectProperty.RemoveUnused = value;
					_projectProperty.RemoveUnusedChanged = true;
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
