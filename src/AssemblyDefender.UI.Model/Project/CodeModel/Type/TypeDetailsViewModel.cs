using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;
using AD = AssemblyDefender;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class TypeDetailsViewModel : NodeDetailsViewModel<TypeViewModel>
	{
		#region Fields

		private bool _isVisibleOutsideAssembly;
		private TypeDeclaration _type;
		private AD.ProjectType _projectType;
		private NodeProperties _parentProperties;

		#endregion

		#region Ctors

		public TypeDetailsViewModel(TypeViewModel parent)
			: base(parent)
		{
			_projectType = Parent.ProjectType;

			_type = Parent.Type;
			_isVisibleOutsideAssembly = _type.IsVisibleOutsideAssembly();

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
				return _projectType.NameChanged ? _projectType.Name : _type.Name;
			}
			set
			{
				string name = (value ?? "").Trim();
				if (string.IsNullOrEmpty(name))
				{
					throw new ShellException(SR.EmptyStringNotValid);
				}

				_projectType.Name = name;
				OnProjectChanged();
				OnPropertyChanged("Name");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNameChanged
		{
			get { return _projectType.NameChanged; }
			set
			{
				if (_projectType.NameChanged == value)
					return;

				_projectType.NameChanged = value;

				if (value)
				{
					_projectType.Name = _type.Name;
				}

				OnProjectChanged();
				OnPropertyChanged("Name");
				OnPropertyChanged("IsNameChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Namespace
		{
			get
			{
				return _projectType.NamespaceChanged ? _projectType.Namespace : _type.Namespace;
			}
			set
			{
				_projectType.Namespace = (value ?? "").Trim().NullIfEmpty();
				OnProjectChanged();
				OnPropertyChanged("Namespace");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNamespaceChanged
		{
			get { return _projectType.NamespaceChanged; }
			set
			{
				if (_projectType.NamespaceChanged == value)
					return;

				_projectType.NamespaceChanged = value;

				if (value)
				{
					_projectType.Namespace = _type.Namespace;
				}

				OnProjectChanged();
				OnPropertyChanged("Namespace");
				OnPropertyChanged("IsNamespaceChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool Rename
		{
			get
			{
				if (!CanRename)
					return false;

				if (_projectType.RenameChanged)
					return _projectType.Rename;

				return _parentProperties.RenameMembers;
			}
			set
			{
				if (_parentProperties.RenameMembers == value)
				{
					_projectType.Rename = false;
					_projectType.RenameChanged = false;
				}
				else
				{
					_projectType.Rename = value;
					_projectType.RenameChanged = true;
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

				if (!_parentProperties.RenamePublicTypes && _isVisibleOutsideAssembly)
					return false;

				if (!AD.MemberRenameHelper.CanRename(_type))
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenameMembers
		{
			get
			{
				if (!_parentProperties.CanRenameMembers)
					return false;

				if (_projectType.RenameMembersChanged)
					return _projectType.RenameMembers;

				return _parentProperties.RenameMembers;
			}
			set
			{
				if (_parentProperties.RenameMembers == value)
				{
					_projectType.RenameMembers = false;
					_projectType.RenameMembersChanged = false;
				}
				else
				{
					_projectType.RenameMembers = value;
					_projectType.RenameMembersChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("RenameMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanRenameMembers
		{
			get { return _parentProperties.CanRenameMembers; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicMembers
		{
			get { return RenamePublicTypes || RenamePublicMethods || RenamePublicFields || RenamePublicProperties || RenamePublicEvents; }
			set
			{
				RenamePublicTypes = value;
				RenamePublicMethods = value;
				RenamePublicFields = value;
				RenamePublicProperties = value;
				RenamePublicEvents = value;

				OnProjectChanged();
				OnPropertyChanged("RenamePublicMembers");
				OnPropertyChanged("RenamePublicTypes");
				OnPropertyChanged("RenamePublicMethods");
				OnPropertyChanged("RenamePublicFields");
				OnPropertyChanged("RenamePublicProperties");
				OnPropertyChanged("RenamePublicEvents");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicTypes
		{
			get
			{
				if (!_parentProperties.CanRenameMembers)
					return false;

				if (_projectType.RenamePublicTypesChanged)
					return _projectType.RenamePublicTypes;

				return _parentProperties.RenamePublicTypes;
			}
			set
			{
				if (_parentProperties.RenamePublicTypes == value)
				{
					_projectType.RenamePublicTypes = false;
					_projectType.RenamePublicTypesChanged = false;
				}
				else
				{
					_projectType.RenamePublicTypes = value;
					_projectType.RenamePublicTypesChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("RenamePublicTypes");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicMethods
		{
			get
			{
				if (!_parentProperties.CanRenameMembers)
					return false;

				if (_projectType.RenamePublicMethodsChanged)
					return _projectType.RenamePublicMethods;

				return _parentProperties.RenamePublicMethods;
			}
			set
			{
				if (_parentProperties.RenamePublicMethods == value)
				{
					_projectType.RenamePublicMethods = false;
					_projectType.RenamePublicMethodsChanged = false;
				}
				else
				{
					_projectType.RenamePublicMethods = value;
					_projectType.RenamePublicMethodsChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("RenamePublicMethods");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicFields
		{
			get
			{
				if (!_parentProperties.CanRenameMembers)
					return false;

				if (_projectType.RenamePublicFieldsChanged)
					return _projectType.RenamePublicFields;

				return _parentProperties.RenamePublicFields;
			}
			set
			{
				if (_parentProperties.RenamePublicFields == value)
				{
					_projectType.RenamePublicFields = false;
					_projectType.RenamePublicFieldsChanged = false;
				}
				else
				{
					_projectType.RenamePublicFields = value;
					_projectType.RenamePublicFieldsChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("RenamePublicFields");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicProperties
		{
			get
			{
				if (!_parentProperties.CanRenameMembers)
					return false;

				if (_projectType.RenamePublicPropertiesChanged)
					return _projectType.RenamePublicProperties;

				return _parentProperties.RenamePublicProperties;
			}
			set
			{
				if (_parentProperties.RenamePublicProperties == value)
				{
					_projectType.RenamePublicProperties = false;
					_projectType.RenamePublicPropertiesChanged = false;
				}
				else
				{
					_projectType.RenamePublicProperties = value;
					_projectType.RenamePublicPropertiesChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("RenamePublicProperties");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicEvents
		{
			get
			{
				if (!_parentProperties.CanRenameMembers)
					return false;

				if (_projectType.RenamePublicEventsChanged)
					return _projectType.RenamePublicEvents;

				return _parentProperties.RenamePublicEvents;
			}
			set
			{
				if (_parentProperties.RenamePublicEvents == value)
				{
					_projectType.RenamePublicEvents = false;
					_projectType.RenamePublicEventsChanged = false;
				}
				else
				{
					_projectType.RenamePublicEvents = value;
					_projectType.RenamePublicEventsChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("RenamePublicEvents");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateControlFlow
		{
			get
			{
				if (!_parentProperties.CanObfuscateControlFlow)
					return false;

				if (_projectType.ObfuscateControlFlowChanged)
					return _projectType.ObfuscateControlFlow;

				return _parentProperties.ObfuscateControlFlow;
			}
			set
			{
				if (_parentProperties.ObfuscateControlFlow == value)
				{
					_projectType.ObfuscateControlFlow = false;
					_projectType.ObfuscateControlFlowChanged = false;
				}
				else
				{
					_projectType.ObfuscateControlFlow = value;
					_projectType.ObfuscateControlFlowChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("ObfuscateControlFlow");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanObfuscateControlFlow
		{
			get
			{
				if (!_parentProperties.CanObfuscateControlFlow)
					return false;

				if (_type.IsInterface)
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool EncryptIL
		{
			get
			{
				if (!_parentProperties.CanEncryptIL)
					return false;

				if (_projectType.EncryptILChanged)
					return _projectType.EncryptIL;

				return _parentProperties.EncryptIL;
			}
			set
			{
				if (_parentProperties.EncryptIL == value)
				{
					_projectType.EncryptIL = false;
					_projectType.EncryptILChanged = false;
				}
				else
				{
					_projectType.EncryptIL = value;
					_projectType.EncryptILChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("EncryptIL");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanEncryptIL
		{
			get
			{
				if (!_parentProperties.CanEncryptIL)
					return false;

				if (_type.IsInterface)
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateStrings
		{
			get
			{
				if (!_parentProperties.CanObfuscateStrings)
					return false;

				if (_projectType.ObfuscateStringsChanged)
					return _projectType.ObfuscateStrings;

				return _parentProperties.ObfuscateStrings;
			}
			set
			{
				if (_parentProperties.ObfuscateStrings == value)
				{
					_projectType.ObfuscateStrings = false;
					_projectType.ObfuscateStringsChanged = false;
				}
				else
				{
					_projectType.ObfuscateStrings = value;
					_projectType.ObfuscateStringsChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("ObfuscateStrings");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanObfuscateStrings
		{
			get
			{
				if (!_parentProperties.CanObfuscateStrings)
					return false;

				if (_type.IsInterface)
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

				if (_projectType.RemoveUnusedChanged)
					return _projectType.RemoveUnused;

				return _parentProperties.RemoveUnusedMembers;
			}
			set
			{
				if (_parentProperties.RemoveUnusedMembers == value)
				{
					_projectType.RemoveUnused = false;
					_projectType.RemoveUnusedChanged = false;
				}
				else
				{
					_projectType.RemoveUnused = value;
					_projectType.RemoveUnusedChanged = true;
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

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RemoveUnusedMembers
		{
			get
			{
				if (!_parentProperties.CanRemoveUnusedMembers)
					return false;

				if (_projectType.RemoveUnusedMembersChanged)
					return _projectType.RemoveUnusedMembers;

				return _parentProperties.RemoveUnusedMembers;
			}
			set
			{
				if (_parentProperties.RemoveUnusedMembers == value)
				{
					_projectType.RemoveUnusedMembers = false;
					_projectType.RemoveUnusedMembersChanged = false;
				}
				else
				{
					_projectType.RemoveUnusedMembers = value;
					_projectType.RemoveUnusedMembersChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("RemoveUnusedMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanRemoveUnusedMembers
		{
			get { return _parentProperties.CanRemoveUnusedMembers; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool Seal
		{
			get
			{
				if (!CanSeal)
					return false;

				if (_projectType.SealChanged)
					return _projectType.Seal;

				return _parentProperties.SealTypes;
			}
			set
			{
				if (_parentProperties.SealTypes == value)
				{
					_projectType.Seal = false;
					_projectType.SealChanged = false;
				}
				else
				{
					_projectType.Seal = value;
					_projectType.SealChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("Seal");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanSeal
		{
			get
			{
				if (!_parentProperties.CanSealTypes)
					return false;

				if (!_parentProperties.SealPublicTypes && _isVisibleOutsideAssembly)
					return false;

				if (_type.IsSealed)
					return false;

				if (_type.IsInterface)
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool SealTypes
		{
			get
			{
				if (!_parentProperties.CanSealTypes)
					return false;

				if (_projectType.SealTypesChanged)
					return _projectType.SealTypes;

				return _parentProperties.SealTypes;
			}
			set
			{
				if (_parentProperties.SealTypes == value)
				{
					_projectType.SealTypes = false;
					_projectType.SealTypesChanged = false;
				}
				else
				{
					_projectType.SealTypes = value;
					_projectType.SealTypesChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("SealTypes");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanSealTypes
		{
			get
			{
				if (!_parentProperties.CanSealTypes)
					return false;

				if (_type.IsInterface)
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool DevirtualizeMethods
		{
			get
			{
				if (!_parentProperties.CanDevirtualizeMethods)
					return false;

				if (_projectType.DevirtualizeMethodsChanged)
					return _projectType.DevirtualizeMethods;

				return _parentProperties.DevirtualizeMethods;
			}
			set
			{
				if (_parentProperties.DevirtualizeMethods == value)
				{
					_projectType.DevirtualizeMethods = false;
					_projectType.DevirtualizeMethodsChanged = false;
				}
				else
				{
					_projectType.DevirtualizeMethods = value;
					_projectType.DevirtualizeMethodsChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("DevirtualizeMethods");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanDevirtualizeMethods
		{
			get
			{
				if (!_parentProperties.CanDevirtualizeMethods)
					return false;

				if (_type.IsInterface)
					return false;

				return true;
			}
		}

		#endregion
	}
}
