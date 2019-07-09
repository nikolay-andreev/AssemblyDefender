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
	public class ModuleDetailsViewModel : NodeDetailsViewModel<ModuleViewModel>
	{
		#region Fields

		private Module _module;
		private AD.ProjectModule _projectModule;
		private NodeProperties _parentProperties;

		#endregion

		#region Ctors

		public ModuleDetailsViewModel(ModuleViewModel parent)
			: base(parent)
		{
			_projectModule = Parent.ProjectModule;

			_module = Parent.Module;

			_parentProperties = new NodeProperties();
			_parentProperties.LoadParent(Parent);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsPrimeModule
		{
			get { return Parent.Module.IsPrimeModule; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Name
		{
			get { return _projectModule.NameChanged ? _projectModule.Name : _module.Name; }
			set
			{
				string name = (value ?? "").Trim();
				if (string.IsNullOrEmpty(name))
				{
					throw new ShellException(SR.EmptyStringNotValid);
				}

				if (PathUtils.ContainsInvalidFileNameChars(name))
				{
					throw new ShellException(SR.AssemblyNameNotValid);
				}

				_projectModule.Name = name;
				OnProjectChanged();
				OnPropertyChanged("Name");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNameChanged
		{
			get
			{
				if (IsPrimeModule)
					return false;

				return _projectModule.NameChanged;
			}
			set
			{
				if (_projectModule.NameChanged == value)
					return;

				_projectModule.NameChanged = value;

				if (value)
				{
					_projectModule.Name = _module.Name;
				}

				OnProjectChanged();
				OnPropertyChanged("Name");
				OnPropertyChanged("IsNameChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanChangeName
		{
			get
			{
				if (IsPrimeModule)
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

				if (_projectModule.RenameMembersChanged)
					return _projectModule.RenameMembers;

				return _parentProperties.RenameMembers;
			}
			set
			{
				if (_parentProperties.RenameMembers == value)
				{
					_projectModule.RenameMembers = false;
					_projectModule.RenameMembersChanged = false;
				}
				else
				{
					_projectModule.RenameMembers = value;
					_projectModule.RenameMembersChanged = true;
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

				if (_projectModule.RenamePublicTypesChanged)
					return _projectModule.RenamePublicTypes;

				return _parentProperties.RenamePublicTypes;
			}
			set
			{
				if (_parentProperties.RenamePublicTypes == value)
				{
					_projectModule.RenamePublicTypes = false;
					_projectModule.RenamePublicTypesChanged = false;
				}
				else
				{
					_projectModule.RenamePublicTypes = value;
					_projectModule.RenamePublicTypesChanged = true;
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

				if (_projectModule.RenamePublicMethodsChanged)
					return _projectModule.RenamePublicMethods;

				return _parentProperties.RenamePublicMethods;
			}
			set
			{
				if (_parentProperties.RenamePublicMethods == value)
				{
					_projectModule.RenamePublicMethods = false;
					_projectModule.RenamePublicMethodsChanged = false;
				}
				else
				{
					_projectModule.RenamePublicMethods = value;
					_projectModule.RenamePublicMethodsChanged = true;
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

				if (_projectModule.RenamePublicFieldsChanged)
					return _projectModule.RenamePublicFields;

				return _parentProperties.RenamePublicFields;
			}
			set
			{
				if (_parentProperties.RenamePublicFields == value)
				{
					_projectModule.RenamePublicFields = false;
					_projectModule.RenamePublicFieldsChanged = false;
				}
				else
				{
					_projectModule.RenamePublicFields = value;
					_projectModule.RenamePublicFieldsChanged = true;
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

				if (_projectModule.RenamePublicPropertiesChanged)
					return _projectModule.RenamePublicProperties;

				return _parentProperties.RenamePublicProperties;
			}
			set
			{
				if (_parentProperties.RenamePublicProperties == value)
				{
					_projectModule.RenamePublicProperties = false;
					_projectModule.RenamePublicPropertiesChanged = false;
				}
				else
				{
					_projectModule.RenamePublicProperties = value;
					_projectModule.RenamePublicPropertiesChanged = true;
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

				if (_projectModule.RenamePublicEventsChanged)
					return _projectModule.RenamePublicEvents;

				return _parentProperties.RenamePublicEvents;
			}
			set
			{
				if (_parentProperties.RenamePublicEvents == value)
				{
					_projectModule.RenamePublicEvents = false;
					_projectModule.RenamePublicEventsChanged = false;
				}
				else
				{
					_projectModule.RenamePublicEvents = value;
					_projectModule.RenamePublicEventsChanged = true;
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

				if (_projectModule.ObfuscateControlFlowChanged)
					return _projectModule.ObfuscateControlFlow;

				return _parentProperties.ObfuscateControlFlow;
			}
			set
			{
				if (_parentProperties.ObfuscateControlFlow == value)
				{
					_projectModule.ObfuscateControlFlow = false;
					_projectModule.ObfuscateControlFlowChanged = false;
				}
				else
				{
					_projectModule.ObfuscateControlFlow = value;
					_projectModule.ObfuscateControlFlowChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("ObfuscateControlFlow");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanObfuscateControlFlow
		{
			get { return _parentProperties.CanObfuscateControlFlow; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool EncryptIL
		{
			get
			{
				if (!_parentProperties.CanEncryptIL)
					return false;

				if (_projectModule.EncryptILChanged)
					return _projectModule.EncryptIL;

				return _parentProperties.EncryptIL;
			}
			set
			{
				if (_parentProperties.EncryptIL == value)
				{
					_projectModule.EncryptIL = false;
					_projectModule.EncryptILChanged = false;
				}
				else
				{
					_projectModule.EncryptIL = value;
					_projectModule.EncryptILChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("EncryptIL");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanEncryptIL
		{
			get { return _parentProperties.CanEncryptIL; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateStrings
		{
			get
			{
				if (!_parentProperties.CanObfuscateStrings)
					return false;

				if (_projectModule.ObfuscateStringsChanged)
					return _projectModule.ObfuscateStrings;

				return _parentProperties.ObfuscateStrings;
			}
			set
			{
				if (_parentProperties.ObfuscateStrings == value)
				{
					_projectModule.ObfuscateStrings = false;
					_projectModule.ObfuscateStringsChanged = false;
				}
				else
				{
					_projectModule.ObfuscateStrings = value;
					_projectModule.ObfuscateStringsChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("ObfuscateStrings");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanObfuscateStrings
		{
			get { return _parentProperties.CanObfuscateStrings; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RemoveUnusedMembers
		{
			get
			{
				if (!_parentProperties.CanRemoveUnusedMembers)
					return false;

				if (_projectModule.RemoveUnusedMembersChanged)
					return _projectModule.RemoveUnusedMembers;

				return _parentProperties.RemoveUnusedMembers;
			}
			set
			{
				if (_parentProperties.RemoveUnusedMembers == value)
				{
					_projectModule.RemoveUnusedMembers = false;
					_projectModule.RemoveUnusedMembersChanged = false;
				}
				else
				{
					_projectModule.RemoveUnusedMembers = value;
					_projectModule.RemoveUnusedMembersChanged = true;
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
		public bool SealTypes
		{
			get
			{
				if (!_parentProperties.CanSealTypes)
					return false;

				if (_projectModule.SealTypesChanged)
					return _projectModule.SealTypes;

				return _parentProperties.SealTypes;
			}
			set
			{
				if (_parentProperties.SealTypes == value)
				{
					_projectModule.SealTypes = false;
					_projectModule.SealTypesChanged = false;
				}
				else
				{
					_projectModule.SealTypes = value;
					_projectModule.SealTypesChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("SealTypes");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanSealTypes
		{
			get { return _parentProperties.CanSealTypes; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool DevirtualizeMethods
		{
			get
			{
				if (!_parentProperties.CanDevirtualizeMethods)
					return false;

				if (_projectModule.DevirtualizeMethodsChanged)
					return _projectModule.DevirtualizeMethods;

				return _parentProperties.DevirtualizeMethods;
			}
			set
			{
				if (_parentProperties.DevirtualizeMethods == value)
				{
					_projectModule.DevirtualizeMethods = false;
					_projectModule.DevirtualizeMethodsChanged = false;
				}
				else
				{
					_projectModule.DevirtualizeMethods = value;
					_projectModule.DevirtualizeMethodsChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("DevirtualizeMethods");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanDevirtualizeMethods
		{
			get { return _parentProperties.CanDevirtualizeMethods; }
		}

		#endregion
	}
}
