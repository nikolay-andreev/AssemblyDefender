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
	public class NamespaceDetailsViewModel : NodeDetailsViewModel<NamespaceViewModel>
	{
		#region Fields

		private AD.ProjectNamespace _projectNamespace;
		private NodeProperties _parentProperties;

		#endregion

		#region Ctors

		public NamespaceDetailsViewModel(NamespaceViewModel parent)
			: base(parent)
		{
			_projectNamespace = Parent.ProjectNamespace;

			_parentProperties = new NodeProperties();
			_parentProperties.LoadParent(Parent);
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenameMembers
		{
			get
			{
				if (!_parentProperties.CanRenameMembers)
					return false;

				if (_projectNamespace.RenameMembersChanged)
					return _projectNamespace.RenameMembers;

				return _parentProperties.RenameMembers;
			}
			set
			{
				if (_parentProperties.RenameMembers == value)
				{
					_projectNamespace.RenameMembers = false;
					_projectNamespace.RenameMembersChanged = false;
				}
				else
				{
					_projectNamespace.RenameMembers = value;
					_projectNamespace.RenameMembersChanged = true;
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

				if (_projectNamespace.RenamePublicTypesChanged)
					return _projectNamespace.RenamePublicTypes;

				return _parentProperties.RenamePublicTypes;
			}
			set
			{
				if (_parentProperties.RenamePublicTypes == value)
				{
					_projectNamespace.RenamePublicTypes = false;
					_projectNamespace.RenamePublicTypesChanged = false;
				}
				else
				{
					_projectNamespace.RenamePublicTypes = value;
					_projectNamespace.RenamePublicTypesChanged = true;
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

				if (_projectNamespace.RenamePublicMethodsChanged)
					return _projectNamespace.RenamePublicMethods;

				return _parentProperties.RenamePublicMethods;
			}
			set
			{
				if (_parentProperties.RenamePublicMethods == value)
				{
					_projectNamespace.RenamePublicMethods = false;
					_projectNamespace.RenamePublicMethodsChanged = false;
				}
				else
				{
					_projectNamespace.RenamePublicMethods = value;
					_projectNamespace.RenamePublicMethodsChanged = true;
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

				if (_projectNamespace.RenamePublicFieldsChanged)
					return _projectNamespace.RenamePublicFields;

				return _parentProperties.RenamePublicFields;
			}
			set
			{
				if (_parentProperties.RenamePublicFields == value)
				{
					_projectNamespace.RenamePublicFields = false;
					_projectNamespace.RenamePublicFieldsChanged = false;
				}
				else
				{
					_projectNamespace.RenamePublicFields = value;
					_projectNamespace.RenamePublicFieldsChanged = true;
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

				if (_projectNamespace.RenamePublicPropertiesChanged)
					return _projectNamespace.RenamePublicProperties;

				return _parentProperties.RenamePublicProperties;
			}
			set
			{
				if (_parentProperties.RenamePublicProperties == value)
				{
					_projectNamespace.RenamePublicProperties = false;
					_projectNamespace.RenamePublicPropertiesChanged = false;
				}
				else
				{
					_projectNamespace.RenamePublicProperties = value;
					_projectNamespace.RenamePublicPropertiesChanged = true;
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

				if (_projectNamespace.RenamePublicEventsChanged)
					return _projectNamespace.RenamePublicEvents;

				return _parentProperties.RenamePublicEvents;
			}
			set
			{
				if (_parentProperties.RenamePublicEvents == value)
				{
					_projectNamespace.RenamePublicEvents = false;
					_projectNamespace.RenamePublicEventsChanged = false;
				}
				else
				{
					_projectNamespace.RenamePublicEvents = value;
					_projectNamespace.RenamePublicEventsChanged = true;
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

				if (_projectNamespace.ObfuscateControlFlowChanged)
					return _projectNamespace.ObfuscateControlFlow;

				return _parentProperties.ObfuscateControlFlow;
			}
			set
			{
				if (_parentProperties.ObfuscateControlFlow == value)
				{
					_projectNamespace.ObfuscateControlFlow = false;
					_projectNamespace.ObfuscateControlFlowChanged = false;
				}
				else
				{
					_projectNamespace.ObfuscateControlFlow = value;
					_projectNamespace.ObfuscateControlFlowChanged = true;
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

				if (_projectNamespace.EncryptILChanged)
					return _projectNamespace.EncryptIL;

				return _parentProperties.EncryptIL;
			}
			set
			{
				if (_parentProperties.EncryptIL == value)
				{
					_projectNamespace.EncryptIL = false;
					_projectNamespace.EncryptILChanged = false;
				}
				else
				{
					_projectNamespace.EncryptIL = value;
					_projectNamespace.EncryptILChanged = true;
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

				if (_projectNamespace.ObfuscateStringsChanged)
					return _projectNamespace.ObfuscateStrings;

				return _parentProperties.ObfuscateStrings;
			}
			set
			{
				if (_parentProperties.ObfuscateStrings == value)
				{
					_projectNamespace.ObfuscateStrings = false;
					_projectNamespace.ObfuscateStringsChanged = false;
				}
				else
				{
					_projectNamespace.ObfuscateStrings = value;
					_projectNamespace.ObfuscateStringsChanged = true;
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

				if (_projectNamespace.RemoveUnusedMembersChanged)
					return _projectNamespace.RemoveUnusedMembers;

				return _parentProperties.RemoveUnusedMembers;
			}
			set
			{
				if (_parentProperties.RemoveUnusedMembers == value)
				{
					_projectNamespace.RemoveUnusedMembers = false;
					_projectNamespace.RemoveUnusedMembersChanged = false;
				}
				else
				{
					_projectNamespace.RemoveUnusedMembers = value;
					_projectNamespace.RemoveUnusedMembersChanged = true;
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

				if (_projectNamespace.SealTypesChanged)
					return _projectNamespace.SealTypes;

				return _parentProperties.SealTypes;
			}
			set
			{
				if (_parentProperties.SealTypes == value)
				{
					_projectNamespace.SealTypes = false;
					_projectNamespace.SealTypesChanged = false;
				}
				else
				{
					_projectNamespace.SealTypes = value;
					_projectNamespace.SealTypesChanged = true;
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

				if (_projectNamespace.DevirtualizeMethodsChanged)
					return _projectNamespace.DevirtualizeMethods;

				return _parentProperties.DevirtualizeMethods;
			}
			set
			{
				if (_parentProperties.DevirtualizeMethods == value)
				{
					_projectNamespace.DevirtualizeMethods = false;
					_projectNamespace.DevirtualizeMethodsChanged = false;
				}
				else
				{
					_projectNamespace.DevirtualizeMethods = value;
					_projectNamespace.DevirtualizeMethodsChanged = true;
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
