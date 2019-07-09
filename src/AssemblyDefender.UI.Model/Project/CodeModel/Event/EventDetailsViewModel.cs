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
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
	public class EventDetailsViewModel : NodeDetailsViewModel<EventViewModel>
	{
		#region Fields

		private bool _isVisibleOutsideAssembly;
		private EventDeclaration _eventDecl;
		private AD.ProjectEvent _projectEvent;
		private NodeProperties _parentProperties;

		#endregion

		#region Ctors

		public EventDetailsViewModel(EventViewModel parent)
			: base(parent)
		{
			_projectEvent = Parent.ProjectEvent;

			_eventDecl = Parent.Event;
			_isVisibleOutsideAssembly = _eventDecl.IsVisibleOutsideAssembly();

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
				return _projectEvent.NameChanged ? _projectEvent.Name : _eventDecl.Name;
			}
			set
			{
				string name = (value ?? "").Trim();
				if (string.IsNullOrEmpty(name))
				{
					throw new ShellException(SR.EmptyStringNotValid);
				}

				_projectEvent.Name = name;
				OnProjectChanged();
				OnPropertyChanged("Name");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNameChanged
		{
			get { return _projectEvent.NameChanged; }
			set
			{
				if (_projectEvent.NameChanged == value)
					return;

				_projectEvent.NameChanged = value;

				if (value)
				{
					_projectEvent.Name = _eventDecl.Name;
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

				if (_projectEvent.RenameChanged)
					return _projectEvent.Rename;

				return _parentProperties.RenameMembers;
			}
			set
			{
				if (_parentProperties.RenameMembers == value)
				{
					_projectEvent.Rename = false;
					_projectEvent.RenameChanged = false;
				}
				else
				{
					_projectEvent.Rename = value;
					_projectEvent.RenameChanged = true;
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

				if (!_parentProperties.RenamePublicEvents && _isVisibleOutsideAssembly)
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

				if (_projectEvent.RemoveUnusedChanged)
					return _projectEvent.RemoveUnused;

				return _parentProperties.RemoveUnusedMembers;
			}
			set
			{
				if (_parentProperties.RemoveUnusedMembers == value)
				{
					_projectEvent.RemoveUnused = false;
					_projectEvent.RemoveUnusedChanged = false;
				}
				else
				{
					_projectEvent.RemoveUnused = value;
					_projectEvent.RemoveUnusedChanged = true;
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
