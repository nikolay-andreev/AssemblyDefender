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
	public class MethodDetailsViewModel : NodeDetailsViewModel<MethodViewModel>
	{
		#region Fields

		private bool _isVisibleOutsideAssembly;
		private MethodDeclaration _method;
		private TypeDeclaration _ownerType;
		private AD.ProjectMethod _projectMethod;
		private NodeProperties _parentProperties;

		#endregion

		#region Ctors

		public MethodDetailsViewModel(MethodViewModel parent)
			: base(parent)
		{
			_projectMethod = Parent.ProjectMethod;

			_method = Parent.Method;
			_ownerType = _method.GetOwnerType();
			_isVisibleOutsideAssembly = _method.IsVisibleOutsideAssembly();

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
				return _projectMethod.NameChanged ? _projectMethod.Name : _method.Name;
			}
			set
			{
				string name = (value ?? "").Trim();
				if (string.IsNullOrEmpty(name))
				{
					throw new ShellException(SR.EmptyStringNotValid);
				}

				_projectMethod.Name = name;
				OnProjectChanged();
				OnPropertyChanged("Name");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNameChanged
		{
			get { return _projectMethod.NameChanged; }
			set
			{
				if (_projectMethod.NameChanged == value)
					return;

				_projectMethod.NameChanged = value;

				if (value)
				{
					_projectMethod.Name = _method.Name;
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

				if (_projectMethod.RenameChanged)
					return _projectMethod.Rename;

				return _parentProperties.RenameMembers;
			}
			set
			{
				if (_parentProperties.RenameMembers == value)
				{
					_projectMethod.Rename = false;
					_projectMethod.RenameChanged = false;
				}
				else
				{
					_projectMethod.Rename = value;
					_projectMethod.RenameChanged = true;
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

				if (!_parentProperties.RenamePublicMethods && _isVisibleOutsideAssembly)
					return false;

				if (!AD.MemberRenameHelper.CanRename(_method))
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateControlFlow
		{
			get
			{
				if (!CanObfuscateControlFlow)
					return false;

				if (_projectMethod.ObfuscateControlFlowChanged)
					return _projectMethod.ObfuscateControlFlow;

				return _parentProperties.ObfuscateControlFlow;
			}
			set
			{
				if (_parentProperties.ObfuscateControlFlow == value)
				{
					_projectMethod.ObfuscateControlFlow = false;
					_projectMethod.ObfuscateControlFlowChanged = false;
				}
				else
				{
					_projectMethod.ObfuscateControlFlow = value;
					_projectMethod.ObfuscateControlFlowChanged = true;
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

				if (_method.IsAbstract)
					return false;

				if (_method.CodeType != MethodCodeTypeFlags.CIL)
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool EncryptIL
		{
			get
			{
				if (!CanEncryptIL)
					return false;

				if (_projectMethod.EncryptILChanged)
					return _projectMethod.EncryptIL;

				return _parentProperties.EncryptIL;
			}
			set
			{
				if (_parentProperties.EncryptIL == value)
				{
					_projectMethod.EncryptIL = false;
					_projectMethod.EncryptILChanged = false;
				}
				else
				{
					_projectMethod.EncryptIL = value;
					_projectMethod.EncryptILChanged = true;
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

				if (_method.IsAbstract)
					return false;

				if (_method.CodeType != MethodCodeTypeFlags.CIL)
					return false;

				if (_method.CallConv != MethodCallingConvention.Default)
					return false;

				if (_method.IsConstructor())
					return false;

				return true;
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateStrings
		{
			get
			{
				if (!CanObfuscateStrings)
					return false;

				if (_projectMethod.ObfuscateStringsChanged)
					return _projectMethod.ObfuscateStrings;

				return _parentProperties.ObfuscateStrings;
			}
			set
			{
				if (_parentProperties.ObfuscateStrings == value)
				{
					_projectMethod.ObfuscateStrings = false;
					_projectMethod.ObfuscateStringsChanged = false;
				}
				else
				{
					_projectMethod.ObfuscateStrings = value;
					_projectMethod.ObfuscateStringsChanged = true;
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

				if (_ownerType.IsInterface)
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

				if (_projectMethod.RemoveUnusedChanged)
					return _projectMethod.RemoveUnused;

				return _parentProperties.RemoveUnusedMembers;
			}
			set
			{
				if (_parentProperties.RemoveUnusedMembers == value)
				{
					_projectMethod.RemoveUnused = false;
					_projectMethod.RemoveUnusedChanged = false;
				}
				else
				{
					_projectMethod.RemoveUnused = value;
					_projectMethod.RemoveUnusedChanged = true;
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
		public bool Devirtualize
		{
			get
			{
				if (!CanDevirtualize)
					return false;

				if (_projectMethod.DevirtualizeChanged)
					return _projectMethod.Devirtualize;

				return _parentProperties.DevirtualizeMethods;
			}
			set
			{
				if (_parentProperties.DevirtualizeMethods == value)
				{
					_projectMethod.Devirtualize = false;
					_projectMethod.DevirtualizeChanged = false;
				}
				else
				{
					_projectMethod.Devirtualize = value;
					_projectMethod.DevirtualizeChanged = true;
				}

				OnProjectChanged();
				OnPropertyChanged("Devirtualize");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool CanDevirtualize
		{
			get
			{
				if (!_parentProperties.CanDevirtualizeMethods)
					return false;

				if (!_method.IsVirtual)
					return false;

				if (_ownerType.IsInterface)
					return false;

				return true;
			}
		}

		#endregion
	}
}
