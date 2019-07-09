using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;
using AD = AssemblyDefender;

namespace AssemblyDefender.UI.Model.Project
{
	internal struct NodeProperties
	{
		private int _flags;

		public bool ObfuscateControlFlow
		{
			get { return _flags.IsBitAtIndexOn(0); }
			set { _flags = _flags.SetBitAtIndex(0, value); }
		}

		public bool CanObfuscateControlFlow
		{
			get { return _flags.IsBitAtIndexOn(1); }
			set { _flags = _flags.SetBitAtIndex(1, value); }
		}

		public bool RenameMembers
		{
			get { return _flags.IsBitAtIndexOn(2); }
			set { _flags = _flags.SetBitAtIndex(2, value); }
		}

		public bool RenamePublicMembers
		{
			get { return RenamePublicTypes || RenamePublicMethods || RenamePublicFields || RenamePublicProperties || RenamePublicEvents; }
		}

		public bool RenamePublicTypes
		{
			get { return _flags.IsBitAtIndexOn(3); }
			set { _flags = _flags.SetBitAtIndex(3, value); }
		}

		public bool RenamePublicMethods
		{
			get { return _flags.IsBitAtIndexOn(4); }
			set { _flags = _flags.SetBitAtIndex(4, value); }
		}

		public bool RenamePublicFields
		{
			get { return _flags.IsBitAtIndexOn(5); }
			set { _flags = _flags.SetBitAtIndex(5, value); }
		}

		public bool RenamePublicProperties
		{
			get { return _flags.IsBitAtIndexOn(6); }
			set { _flags = _flags.SetBitAtIndex(6, value); }
		}

		public bool RenamePublicEvents
		{
			get { return _flags.IsBitAtIndexOn(7); }
			set { _flags = _flags.SetBitAtIndex(7, value); }
		}

		public bool RenameEnumMembers
		{
			get { return _flags.IsBitAtIndexOn(8); }
			set { _flags = _flags.SetBitAtIndex(8, value); }
		}

		public bool CanRenameMembers
		{
			get { return _flags.IsBitAtIndexOn(9); }
			set { _flags = _flags.SetBitAtIndex(9, value); }
		}

		public bool EncryptIL
		{
			get { return _flags.IsBitAtIndexOn(10); }
			set { _flags = _flags.SetBitAtIndex(10, value); }
		}

		public bool CanEncryptIL
		{
			get { return _flags.IsBitAtIndexOn(11); }
			set { _flags = _flags.SetBitAtIndex(11, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _flags.IsBitAtIndexOn(12); }
			set { _flags = _flags.SetBitAtIndex(12, value); }
		}

		public bool CanObfuscateStrings
		{
			get { return _flags.IsBitAtIndexOn(13); }
			set { _flags = _flags.SetBitAtIndex(13, value); }
		}

		public bool RemoveUnusedMembers
		{
			get { return _flags.IsBitAtIndexOn(14); }
			set { _flags = _flags.SetBitAtIndex(14, value); }
		}

		public bool RemoveUnusedPublicMembers
		{
			get { return _flags.IsBitAtIndexOn(15); }
			set { _flags = _flags.SetBitAtIndex(15, value); }
		}

		public bool CanRemoveUnusedMembers
		{
			get { return _flags.IsBitAtIndexOn(16); }
			set { _flags = _flags.SetBitAtIndex(16, value); }
		}

		public bool SealTypes
		{
			get { return _flags.IsBitAtIndexOn(17); }
			set { _flags = _flags.SetBitAtIndex(17, value); }
		}

		public bool SealPublicTypes
		{
			get { return _flags.IsBitAtIndexOn(18); }
			set { _flags = _flags.SetBitAtIndex(18, value); }
		}

		public bool CanSealTypes
		{
			get { return _flags.IsBitAtIndexOn(19); }
			set { _flags = _flags.SetBitAtIndex(19, value); }
		}

		public bool DevirtualizeMethods
		{
			get { return _flags.IsBitAtIndexOn(20); }
			set { _flags = _flags.SetBitAtIndex(20, value); }
		}

		public bool DevirtualizePublicMethods
		{
			get { return _flags.IsBitAtIndexOn(21); }
			set { _flags = _flags.SetBitAtIndex(21, value); }
		}

		public bool CanDevirtualizeMethods
		{
			get { return _flags.IsBitAtIndexOn(22); }
			set { _flags = _flags.SetBitAtIndex(22, value); }
		}

		internal void Load(AssemblyViewModel viewModel)
		{
			var projectAssembly = viewModel.ProjectAssembly;

			if (projectAssembly.RenameMembers)
			{
				RenameMembers = true;
				CanRenameMembers = true;
				RenamePublicTypes = projectAssembly.RenamePublicTypes;
				RenamePublicMethods = projectAssembly.RenamePublicMethods;
				RenamePublicFields = projectAssembly.RenamePublicFields;
				RenamePublicProperties = projectAssembly.RenamePublicProperties;
				RenamePublicEvents = projectAssembly.RenamePublicEvents;
				RenameEnumMembers = projectAssembly.RenameEnumMembers;
			}

			if (projectAssembly.ObfuscateControlFlow)
			{
				ObfuscateControlFlow = true;
				CanObfuscateControlFlow = true;
			}

			if (projectAssembly.EncryptIL)
			{
				EncryptIL = true;
				CanEncryptIL = true;
			}

			if (projectAssembly.ObfuscateStrings)
			{
				ObfuscateStrings = true;
				CanObfuscateStrings = true;
			}

			if (projectAssembly.RemoveUnusedMembers)
			{
				RemoveUnusedMembers = true;
				CanRemoveUnusedMembers = true;
				RemoveUnusedPublicMembers = projectAssembly.RemoveUnusedPublicMembers;
			}

			if (projectAssembly.SealTypes)
			{
				SealTypes = true;
				CanSealTypes = true;
				SealPublicTypes = projectAssembly.SealPublicTypes;
			}

			if (projectAssembly.DevirtualizeMethods)
			{
				DevirtualizeMethods = true;
				CanDevirtualizeMethods = true;
				DevirtualizePublicMethods = projectAssembly.DevirtualizePublicMethods;
			}
		}

		internal void Load(ModuleViewModel viewModel)
		{
			LoadParent(viewModel);
			
			var projectModule = viewModel.ProjectModule;

			if (CanRenameMembers)
			{
				if (projectModule.RenameMembersChanged)
					RenameMembers = projectModule.RenameMembers;

				if (projectModule.RenamePublicTypesChanged)
					RenamePublicTypes = projectModule.RenamePublicTypes;

				if (projectModule.RenamePublicMethodsChanged)
					RenamePublicMethods = projectModule.RenamePublicMethods;

				if (projectModule.RenamePublicFieldsChanged)
					RenamePublicFields = projectModule.RenamePublicFields;

				if (projectModule.RenamePublicPropertiesChanged)
					RenamePublicProperties = projectModule.RenamePublicProperties;

				if (projectModule.RenamePublicEventsChanged)
					RenamePublicEvents = projectModule.RenamePublicEvents;
			}

			if (CanObfuscateControlFlow && projectModule.ObfuscateControlFlowChanged)
				ObfuscateControlFlow = projectModule.ObfuscateControlFlow;

			if (CanEncryptIL && projectModule.EncryptILChanged)
				EncryptIL = projectModule.EncryptIL;

			if (CanObfuscateStrings && projectModule.ObfuscateStringsChanged)
				ObfuscateStrings = projectModule.ObfuscateStrings;

			if (CanRemoveUnusedMembers && projectModule.RemoveUnusedMembersChanged)
				RemoveUnusedMembers = projectModule.RemoveUnusedMembers;

			if (CanSealTypes && projectModule.SealTypesChanged)
				SealTypes = projectModule.SealTypes;

			if (CanDevirtualizeMethods && projectModule.DevirtualizeMethodsChanged)
				DevirtualizeMethods = projectModule.DevirtualizeMethods;
		}

		internal void Load(NamespaceViewModel viewModel)
		{
			LoadParent(viewModel);

			var projectNamespace = viewModel.ProjectNamespace;

			if (CanRenameMembers)
			{
				if (projectNamespace.RenameMembersChanged)
					RenameMembers = projectNamespace.RenameMembers;

				if (projectNamespace.RenamePublicTypesChanged)
					RenamePublicTypes = projectNamespace.RenamePublicTypes;

				if (projectNamespace.RenamePublicMethodsChanged)
					RenamePublicMethods = projectNamespace.RenamePublicMethods;

				if (projectNamespace.RenamePublicFieldsChanged)
					RenamePublicFields = projectNamespace.RenamePublicFields;

				if (projectNamespace.RenamePublicPropertiesChanged)
					RenamePublicProperties = projectNamespace.RenamePublicProperties;

				if (projectNamespace.RenamePublicEventsChanged)
					RenamePublicEvents = projectNamespace.RenamePublicEvents;
			}

			if (CanObfuscateControlFlow && projectNamespace.ObfuscateControlFlowChanged)
				ObfuscateControlFlow = projectNamespace.ObfuscateControlFlow;

			if (CanEncryptIL && projectNamespace.EncryptILChanged)
				EncryptIL = projectNamespace.EncryptIL;

			if (CanObfuscateStrings && projectNamespace.ObfuscateStringsChanged)
				ObfuscateStrings = projectNamespace.ObfuscateStrings;

			if (CanRemoveUnusedMembers && projectNamespace.RemoveUnusedMembersChanged)
				RemoveUnusedMembers = projectNamespace.RemoveUnusedMembers;

			if (CanSealTypes && projectNamespace.SealTypesChanged)
				SealTypes = projectNamespace.SealTypes;

			if (CanDevirtualizeMethods && projectNamespace.DevirtualizeMethodsChanged)
				DevirtualizeMethods = projectNamespace.DevirtualizeMethods;
		}

		internal void Load(TypeViewModel viewModel)
		{
			LoadParent(viewModel);

			var projectType = viewModel.ProjectType;

			if (CanRenameMembers)
			{
				if (projectType.RenameMembersChanged)
					RenameMembers = projectType.RenameMembers;

				if (projectType.RenamePublicTypesChanged)
					RenamePublicTypes = projectType.RenamePublicTypes;

				if (projectType.RenamePublicMethodsChanged)
					RenamePublicMethods = projectType.RenamePublicMethods;

				if (projectType.RenamePublicFieldsChanged)
					RenamePublicFields = projectType.RenamePublicFields;

				if (projectType.RenamePublicPropertiesChanged)
					RenamePublicProperties = projectType.RenamePublicProperties;

				if (projectType.RenamePublicEventsChanged)
					RenamePublicEvents = projectType.RenamePublicEvents;
			}

			if (CanObfuscateControlFlow && projectType.ObfuscateControlFlowChanged)
				ObfuscateControlFlow = projectType.ObfuscateControlFlow;

			if (CanEncryptIL && projectType.EncryptILChanged)
				EncryptIL = projectType.EncryptIL;

			if (CanObfuscateStrings && projectType.ObfuscateStringsChanged)
				ObfuscateStrings = projectType.ObfuscateStrings;

			if (CanRemoveUnusedMembers && projectType.RemoveUnusedMembersChanged)
				RemoveUnusedMembers = projectType.RemoveUnusedMembers;

			if (CanSealTypes && projectType.SealTypesChanged)
				SealTypes = projectType.SealTypes;

			if (CanDevirtualizeMethods && projectType.DevirtualizeMethodsChanged)
				DevirtualizeMethods = projectType.DevirtualizeMethods;
		}

		internal void LoadParent(ModuleViewModel viewModel)
		{
			Load((AssemblyViewModel)viewModel.Parent);
		}

		internal void LoadParent(NamespaceViewModel viewModel)
		{
			Load((ModuleViewModel)viewModel.Parent);
		}

		internal void LoadParent(TypeViewModel viewModel)
		{
			if (viewModel.Parent is NamespaceViewModel)
				Load((NamespaceViewModel)viewModel.Parent);
			else if (viewModel.Parent is TypeViewModel)
				Load((TypeViewModel)viewModel.Parent);
			else
				throw new InvalidOperationException();
		}

		internal void LoadParent(MethodViewModel viewModel)
		{
			Load(viewModel.FindParent<TypeViewModel>(true));
		}

		internal void LoadParent(FieldViewModel viewModel)
		{
			Load(viewModel.FindParent<TypeViewModel>(true));
		}

		internal void LoadParent(PropertyViewModel viewModel)
		{
			Load(viewModel.FindParent<TypeViewModel>(true));
		}

		internal void LoadParent(EventViewModel viewModel)
		{
			Load(viewModel.FindParent<TypeViewModel>(true));
		}
	}
}
