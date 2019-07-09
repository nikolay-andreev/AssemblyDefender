using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using AssemblyDefender.Baml;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;
using AD = AssemblyDefender;
using CA = AssemblyDefender.Net.CustomAttributes;
using AssemblyDefender.UI;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model.Project
{
	public class AssemblyDetailsViewModel : NodeDetailsViewModel<AssemblyViewModel>
	{
		#region Fields

		private string _filePath;
		private string _outputFilePath;
		private DateTime _createdDate;
		private DateTime _lastModifiedDate;
		private long _fileSize;
		private string _name;
		private Version _version;
		private string _title;
		private string _description;
		private string _company;
		private string _product;
		private string _copyright;
		private string _trademark;
		private AD.ProjectAssembly _projectAssembly;
		private AD.ProjectAssemblySign _projectSign;
		private Assembly _assembly;
		private ICommand _changeInputCommand;
		private ICommand _changeOutputCommand;
		private ICommand _signBrowseKeyFileCommand;

		#endregion

		#region Ctors

		public AssemblyDetailsViewModel(AssemblyViewModel parent)
			: base(parent)
		{
			_assembly = Node.Assembly;
			_projectAssembly = Node.ProjectAssembly;
			_projectSign = _projectAssembly.Sign;

			_filePath = _projectAssembly.FilePath;
			_outputFilePath = _filePath;

			UpdateOutputFilePath();

			if (File.Exists(_filePath))
			{
				var fileInfo = new FileInfo(_filePath);
				_createdDate = fileInfo.CreationTime;
				_lastModifiedDate = fileInfo.LastWriteTime;
				_fileSize = fileInfo.Length;
			}

			_changeInputCommand = new DelegateCommand(ChangeInput);
			_changeOutputCommand = new DelegateCommand(ChangeOutput);
			_signBrowseKeyFileCommand = new DelegateCommand(SignBrowseForKeyFile);

			Load();
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string FilePath
		{
			get { return _filePath; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string OutputFilePath
		{
			get { return _outputFilePath; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public DateTime CreatedDate
		{
			get { return _createdDate; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public DateTime LastModifiedDate
		{
			get { return _lastModifiedDate; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public long FileSize
		{
			get { return _fileSize; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand ChangeInputCommand
		{
			get { return _changeInputCommand; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand ChangeOutputCommand
		{
			get { return _changeOutputCommand; }
		}

		public bool HasXaml
		{
			get { return Parent.HasXaml; }
		}

		#endregion

		#region Assembly

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Name
		{
			get { return _projectAssembly.NameChanged ? _projectAssembly.Name : _name; }
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

				_projectAssembly.Name = name;
				UpdateOutputFilePath();
				OnProjectChanged();
				OnPropertyChanged("Name");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsNameChanged
		{
			get { return _projectAssembly.NameChanged; }
			set
			{
				if (_projectAssembly.NameChanged == value)
					return;

				_projectAssembly.NameChanged = value;

				if (value)
				{
					_projectAssembly.Name = _name;
				}

				UpdateOutputFilePath();
				OnProjectChanged();
				OnPropertyChanged("Name");
				OnPropertyChanged("IsNameChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Version
		{
			get
			{
				var version = _projectAssembly.VersionChanged ? _projectAssembly.Version : _version;
				return version != null ? version.ToString() : null;
			}
			set
			{
				string versionString = (value ?? "").Trim();
				if (!string.IsNullOrEmpty(versionString))
				{
					_projectAssembly.Version = System.Version.Parse(versionString);
				}
				else
				{
					_projectAssembly.Version = null;
				}

				OnProjectChanged();
				OnPropertyChanged("Version");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsVersionChanged
		{
			get { return _projectAssembly.VersionChanged; }
			set
			{
				if (_projectAssembly.VersionChanged == value)
					return;

				_projectAssembly.VersionChanged = value;

				if (value)
				{
					_projectAssembly.Version = _version;
				}

				OnProjectChanged();
				OnPropertyChanged("Version");
				OnPropertyChanged("IsVersionChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Title
		{
			get { return _projectAssembly.TitleChanged ? _projectAssembly.Title : _title; }
			set
			{
				_projectAssembly.Title = (value ?? "").Trim();
				OnProjectChanged();
				OnPropertyChanged("Title");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsTitleChanged
		{
			get { return _projectAssembly.TitleChanged; }
			set
			{
				if (_projectAssembly.TitleChanged == value)
					return;

				_projectAssembly.TitleChanged = value;

				if (value)
				{
					_projectAssembly.Title = _title;
				}

				OnProjectChanged();
				OnPropertyChanged("Title");
				OnPropertyChanged("IsTitleChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Description
		{
			get { return _projectAssembly.DescriptionChanged ? _projectAssembly.Description : _description; }
			set
			{
				_projectAssembly.Description = (value ?? "").Trim();
				OnProjectChanged();
				OnPropertyChanged("Description");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsDescriptionChanged
		{
			get { return _projectAssembly.DescriptionChanged; }
			set
			{
				if (_projectAssembly.DescriptionChanged == value)
					return;

				_projectAssembly.DescriptionChanged = value;

				if (value)
				{
					_projectAssembly.Description = _description;
				}

				OnProjectChanged();
				OnPropertyChanged("Description");
				OnPropertyChanged("IsDescriptionChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Company
		{
			get { return _projectAssembly.CompanyChanged ? _projectAssembly.Company : _company; }
			set
			{
				_projectAssembly.Company = (value ?? "").Trim();
				OnProjectChanged();
				OnPropertyChanged("Company");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsCompanyChanged
		{
			get { return _projectAssembly.CompanyChanged; }
			set
			{
				if (_projectAssembly.CompanyChanged == value)
					return;

				_projectAssembly.CompanyChanged = value;

				if (value)
				{
					_projectAssembly.Company = _company;
				}

				OnProjectChanged();
				OnPropertyChanged("Company");
				OnPropertyChanged("IsCompanyChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Product
		{
			get { return _projectAssembly.ProductChanged ? _projectAssembly.Product : _product; }
			set
			{
				_projectAssembly.Product = (value ?? "").Trim();
				OnProjectChanged();
				OnPropertyChanged("Product");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsProductChanged
		{
			get { return _projectAssembly.ProductChanged; }
			set
			{
				if (_projectAssembly.ProductChanged == value)
					return;

				_projectAssembly.ProductChanged = value;

				if (value)
				{
					_projectAssembly.Product = _product;
				}

				OnProjectChanged();
				OnPropertyChanged("Product");
				OnPropertyChanged("IsProductChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Copyright
		{
			get { return _projectAssembly.CopyrightChanged ? _projectAssembly.Copyright : _copyright; }
			set
			{
				_projectAssembly.Copyright = (value ?? "").Trim();
				OnProjectChanged();
				OnPropertyChanged("Copyright");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsCopyrightChanged
		{
			get { return _projectAssembly.CopyrightChanged; }
			set
			{
				if (_projectAssembly.CopyrightChanged == value)
					return;

				_projectAssembly.CopyrightChanged = value;

				if (value)
				{
					_projectAssembly.Copyright = _copyright;
				}

				OnProjectChanged();
				OnPropertyChanged("Copyright");
				OnPropertyChanged("IsCopyrightChanged");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string Trademark
		{
			get { return _projectAssembly.TrademarkChanged ? _projectAssembly.Trademark : _trademark; }
			set
			{
				_projectAssembly.Trademark = (value ?? "").Trim();
				OnProjectChanged();
				OnPropertyChanged("Trademark");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsTrademarkChanged
		{
			get { return _projectAssembly.TrademarkChanged; }
			set
			{
				if (_projectAssembly.TrademarkChanged == value)
					return;

				_projectAssembly.TrademarkChanged = value;

				if (value)
				{
					_projectAssembly.Trademark = _trademark;
				}

				OnProjectChanged();
				OnPropertyChanged("Trademark");
				OnPropertyChanged("IsTrademarkChanged");
			}
		}

		#endregion

		#region Obfuscation

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateControlFlow
		{
			get { return _projectAssembly.ObfuscateControlFlow; }
			set
			{
				if (ObfuscateControlFlow == value)
					return;

				_projectAssembly.ObfuscateControlFlow = value;
				OnProjectChanged();
				OnPropertyChanged("ObfuscateControlFlow");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenameMembers
		{
			get { return _projectAssembly.RenameMembers; }
			set
			{
				if (RenameMembers == value)
					return;

				_projectAssembly.RenameMembers = value;
				OnProjectChanged();
				OnPropertyChanged("RenameMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicMembers
		{
			get { return _projectAssembly.RenamePublicMembers; }
			set
			{
				if (RenamePublicMembers == value)
					return;

				_projectAssembly.RenamePublicMembers = value;
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
			get { return _projectAssembly.RenamePublicTypes; }
			set
			{
				if (RenamePublicTypes == value)
					return;

				_projectAssembly.RenamePublicTypes = value;
				OnProjectChanged();
				OnPropertyChanged("RenamePublicTypes");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicMethods
		{
			get { return _projectAssembly.RenamePublicMethods; }
			set
			{
				if (RenamePublicMethods == value)
					return;

				_projectAssembly.RenamePublicMethods = value;
				OnProjectChanged();
				OnPropertyChanged("RenamePublicMethods");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicFields
		{
			get { return _projectAssembly.RenamePublicFields; }
			set
			{
				if (RenamePublicFields == value)
					return;

				_projectAssembly.RenamePublicFields = value;
				OnProjectChanged();
				OnPropertyChanged("RenamePublicFields");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicProperties
		{
			get { return _projectAssembly.RenamePublicProperties; }
			set
			{
				if (RenamePublicProperties == value)
					return;

				_projectAssembly.RenamePublicProperties = value;
				OnProjectChanged();
				OnPropertyChanged("RenamePublicProperties");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenamePublicEvents
		{
			get { return _projectAssembly.RenamePublicEvents; }
			set
			{
				if (RenamePublicEvents == value)
					return;

				_projectAssembly.RenamePublicEvents = value;
				OnProjectChanged();
				OnPropertyChanged("RenamePublic");
				OnPropertyChanged("RenamePublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenameEnumMembers
		{
			get { return _projectAssembly.RenameEnumMembers; }
			set
			{
				if (RenameEnumMembers == value)
					return;

				_projectAssembly.RenameEnumMembers = value;
				OnProjectChanged();
				OnPropertyChanged("RenameEnumMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenameBindableMembers
		{
			get { return _projectAssembly.RenameBindableMembers; }
			set
			{
				if (RenameBindableMembers == value)
					return;

				_projectAssembly.RenameBindableMembers = value;
				OnProjectChanged();
				OnPropertyChanged("RenameBindableMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenameSerializableMembers
		{
			get { return _projectAssembly.RenameSerializableMembers; }
			set
			{
				if (RenameSerializableMembers == value)
					return;

				_projectAssembly.RenameSerializableMembers = value;
				OnProjectChanged();
				OnPropertyChanged("RenameSerializableMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RenameConfigurationMembers
		{
			get { return _projectAssembly.RenameConfigurationMembers; }
			set
			{
				if (RenameConfigurationMembers == value)
					return;

				_projectAssembly.RenameConfigurationMembers = value;
				OnProjectChanged();
				OnPropertyChanged("RenameConfigurationMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateStrings
		{
			get { return _projectAssembly.ObfuscateStrings; }
			set
			{
				if (ObfuscateStrings == value)
					return;

				_projectAssembly.ObfuscateStrings = value;
				OnProjectChanged();
				OnPropertyChanged("ObfuscateStrings");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool ObfuscateResources
		{
			get { return _projectAssembly.ObfuscateResources; }
			set
			{
				if (ObfuscateResources == value)
					return;

				_projectAssembly.ObfuscateResources = value;
				OnProjectChanged();
				OnPropertyChanged("ObfuscateResources");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool SuppressILdasm
		{
			get { return _projectAssembly.SuppressILdasm; }
			set
			{
				if (SuppressILdasm == value)
					return;

				_projectAssembly.SuppressILdasm = value;
				OnProjectChanged();
				OnPropertyChanged("SuppressILdasm");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IgnoreObfuscationAttribute
		{
			get { return _projectAssembly.IgnoreObfuscationAttribute; }
			set
			{
				if (IgnoreObfuscationAttribute == value)
					return;

				_projectAssembly.IgnoreObfuscationAttribute = value;
				OnProjectChanged();
				OnPropertyChanged("IgnoreObfuscationAttribute");
			}
		}

		#endregion

		#region Optimization

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RemoveUnusedMembers
		{
			get { return _projectAssembly.RemoveUnusedMembers; }
			set
			{
				if (RemoveUnusedMembers == value)
					return;

				_projectAssembly.RemoveUnusedMembers = value;
				OnProjectChanged();
				OnPropertyChanged("RemoveUnusedMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool RemoveUnusedPublicMembers
		{
			get { return _projectAssembly.RemoveUnusedPublicMembers; }
			set
			{
				if (RemoveUnusedPublicMembers == value)
					return;

				_projectAssembly.RemoveUnusedPublicMembers = value;
				OnProjectChanged();
				OnPropertyChanged("RemoveUnusedPublicMembers");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool SealTypes
		{
			get { return _projectAssembly.SealTypes; }
			set
			{
				if (SealTypes == value)
					return;

				_projectAssembly.SealTypes = value;
				OnProjectChanged();
				OnPropertyChanged("SealTypes");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool SealPublicTypes
		{
			get { return _projectAssembly.SealPublicTypes; }
			set
			{
				if (SealPublicTypes == value)
					return;

				_projectAssembly.SealPublicTypes = value;
				OnProjectChanged();
				OnPropertyChanged("SealPublicTypes");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool DevirtualizeMethods
		{
			get { return _projectAssembly.DevirtualizeMethods; }
			set
			{
				if (DevirtualizeMethods == value)
					return;

				_projectAssembly.DevirtualizeMethods = value;
				OnProjectChanged();
				OnPropertyChanged("DevirtualizeMethods");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool DevirtualizePublicMethods
		{
			get { return _projectAssembly.DevirtualizePublicMethods; }
			set
			{
				if (DevirtualizePublicMethods == value)
					return;

				_projectAssembly.DevirtualizePublicMethods = value;
				OnProjectChanged();
				OnPropertyChanged("DevirtualizePublicMethods");
			}
		}

		#endregion

		#region Code encryption

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool EncryptIL
		{
			get { return _projectAssembly.EncryptIL; }
			set
			{
				if (EncryptIL == value)
					return;

				_projectAssembly.EncryptIL = value;
				OnProjectChanged();
				OnPropertyChanged("EncryptIL");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool EncryptILAtomic
		{
			get { return _projectAssembly.EncryptILAtomic; }
			set
			{
				if (EncryptILAtomic == value)
					return;

				_projectAssembly.EncryptILAtomic = value;
				OnProjectChanged();
				OnPropertyChanged("EncryptILAtomic");
			}
		}

		#endregion

		#region Signing

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsSigned
		{
			get { return _projectAssembly.IsSigned; }
			set
			{
				if (IsSigned == value)
					return;

				if (value)
					_projectSign = new ProjectAssemblySign();
				else
					_projectSign = null;

				_projectAssembly.Sign = _projectSign;

				OnProjectChanged();
				OnPropertyChanged("IsSigned");
				OnPropertyChanged("IsDelaySign");
				OnPropertyChanged("SignKeyFile");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public bool IsDelaySign
		{
			get { return _projectSign != null ? _projectSign.DelaySign : false; }
			set
			{
				if (IsDelaySign == value)
					return;

				_projectSign.DelaySign = value;
				OnProjectChanged();
				OnPropertyChanged("IsDelaySign");
			}
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public string SignKeyFile
		{
			get { return _projectSign != null ? _projectSign.KeyFile : null; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ICommand SignBrowseKeyFileCommand
		{
			get { return _signBrowseKeyFileCommand; }
		}

		#endregion

		#region Methods

		private void Load()
		{
			var assembly = Node.Assembly;

			_name = assembly.Name;
			_version = assembly.Version;

			// Title
			var titleAttribute = CA.AssemblyTitleAttribute.FindFirst(assembly.CustomAttributes);
			if (titleAttribute != null)
			{
				_title = titleAttribute.Title;
			}

			// Description
			var descriptionAttribute = CA.AssemblyDescriptionAttribute.FindFirst(assembly.CustomAttributes);
			if (descriptionAttribute != null)
			{
				_description = descriptionAttribute.Description;
			}

			// Company
			var companyAttribute = CA.AssemblyCompanyAttribute.FindFirst(assembly.CustomAttributes);
			if (companyAttribute != null)
			{
				_company = companyAttribute.Company;
			}

			// Product
			var productAttribute = CA.AssemblyProductAttribute.FindFirst(assembly.CustomAttributes);
			if (productAttribute != null)
			{
				_product = productAttribute.Product;
			}

			// Copyright
			var copyrightAttribute = CA.AssemblyCopyrightAttribute.FindFirst(assembly.CustomAttributes);
			if (copyrightAttribute != null)
			{
				_copyright = copyrightAttribute.Copyright;
			}

			// Trademark
			var trademarkAttribute = CA.AssemblyTrademarkAttribute.FindFirst(assembly.CustomAttributes);
			if (trademarkAttribute != null)
			{
				_trademark = trademarkAttribute.Trademark;
			}
		}

		private void UpdateOutputFilePath()
		{
			_outputFilePath = ProjectUtils.GetAssemblyOutputFilePath(_projectAssembly);
			OnPropertyChanged("OutputFilePath");
		}

		private void ChangeInput()
		{
			string filePath = AppService.UI.ShowOpenFileDialog(
				Constants.AssemblyFileFilter,
				SR.OpenFileCaption);

			if (string.IsNullOrEmpty(filePath))
				return;

			_projectAssembly.FilePath = filePath;
			OnProjectChanged();
			ProjectShell.Refresh();

			var assemblyViewModel = ProjectShell.Project.FindAssembly(filePath);
			if (assemblyViewModel != null)
			{
				assemblyViewModel.Show();
			}
		}

		private void ChangeOutput()
		{
			string folderPath = AppService.UI.ShowFolderBrowserDialog(SR.OpenFolderCaption);
			if (string.IsNullOrEmpty(folderPath))
				return;

			_projectAssembly.OutputPath = folderPath;
			OnProjectChanged();

			UpdateOutputFilePath();
		}

		private void SignBrowseForKeyFile()
		{
			string filePath = AppService.UI.ShowOpenFileDialog(
				Constants.StrongNameKeyFileFilter,
				SR.OpenFileCaption);

			if (string.IsNullOrEmpty(filePath))
				return;

			if (!File.Exists(filePath))
				return;

			string password = null;
			if (StrongNameUtils.IsPKCS12File(filePath))
			{
				if (!StrongNamePasswordCache.TryGet(filePath, out password))
					return;
			}

			_projectSign.KeyFile = filePath;
			_projectSign.Password = password;
			OnPropertyChanged("SignKeyFile");
			OnProjectChanged();
		}

		#endregion
	}
}
