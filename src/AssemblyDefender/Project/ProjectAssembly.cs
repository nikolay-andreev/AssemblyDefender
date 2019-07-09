using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ProjectAssembly
	{
		#region Fields

		private int _flags;
		private int _flags2;
		private int _flags3;
		private string _filePath;
		private string _outputPath;
		private string _name;
		private string _culture;
		private Version _version;
		private string _title;
		private string _description;
		private string _company;
		private string _product;
		private string _copyright;
		private string _trademark;
		private ProjectAssemblySign _sign;
		private Dictionary<string, ProjectModule> _modules;
		private Dictionary<string, ProjectResource> _resources;

		#endregion

		#region Ctors

		public ProjectAssembly()
		{
		}

		internal ProjectAssembly(IBinaryAccessor accessor, ProjectReadState state)
		{
			Read(accessor, state);
		}

		#endregion

		#region Properties

		public string FilePath
		{
			get { return _filePath; }
			set { _filePath = value; }
		}

		public string OutputPath
		{
			get { return _outputPath; }
			set { _outputPath = value.NullIfEmpty(); }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public bool NameChanged
		{
			get { return _flags.IsBitAtIndexOn(0); }
			set { _flags = _flags.SetBitAtIndex(0, value); }
		}

		public string Culture
		{
			get { return _culture; }
			set { _culture = value; }
		}

		public bool CultureChanged
		{
			get { return _flags.IsBitAtIndexOn(1); }
			set { _flags = _flags.SetBitAtIndex(1, value); }
		}

		public Version Version
		{
			get { return _version; }
			set { _version = value; }
		}

		public bool VersionChanged
		{
			get { return _flags.IsBitAtIndexOn(2); }
			set { _flags = _flags.SetBitAtIndex(2, value); }
		}

		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		public bool TitleChanged
		{
			get { return _flags.IsBitAtIndexOn(3); }
			set { _flags = _flags.SetBitAtIndex(3, value); }
		}

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public bool DescriptionChanged
		{
			get { return _flags.IsBitAtIndexOn(4); }
			set { _flags = _flags.SetBitAtIndex(4, value); }
		}

		public string Company
		{
			get { return _company; }
			set { _company = value; }
		}

		public bool CompanyChanged
		{
			get { return _flags.IsBitAtIndexOn(5); }
			set { _flags = _flags.SetBitAtIndex(5, value); }
		}

		public string Product
		{
			get { return _product; }
			set { _product = value; }
		}

		public bool ProductChanged
		{
			get { return _flags.IsBitAtIndexOn(6); }
			set { _flags = _flags.SetBitAtIndex(6, value); }
		}

		public string Copyright
		{
			get { return _copyright; }
			set { _copyright = value; }
		}

		public bool CopyrightChanged
		{
			get { return _flags.IsBitAtIndexOn(7); }
			set { _flags = _flags.SetBitAtIndex(7, value); }
		}

		public string Trademark
		{
			get { return _trademark; }
			set { _trademark = value; }
		}

		public bool TrademarkChanged
		{
			get { return _flags.IsBitAtIndexOn(8); }
			set { _flags = _flags.SetBitAtIndex(8, value); }
		}

		public bool RenameMembers
		{
			get { return _flags2.IsBitAtIndexOn(0); }
			set { _flags2 = _flags2.SetBitAtIndex(0, value); }
		}

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
			}
		}

		public bool RenamePublicTypes
		{
			get { return _flags2.IsBitAtIndexOn(1); }
			set { _flags2 = _flags2.SetBitAtIndex(1, value); }
		}

		public bool RenamePublicMethods
		{
			get { return _flags2.IsBitAtIndexOn(2); }
			set { _flags2 = _flags2.SetBitAtIndex(2, value); }
		}

		public bool RenamePublicFields
		{
			get { return _flags2.IsBitAtIndexOn(3); }
			set { _flags2 = _flags2.SetBitAtIndex(3, value); }
		}

		public bool RenamePublicProperties
		{
			get { return _flags2.IsBitAtIndexOn(4); }
			set { _flags2 = _flags2.SetBitAtIndex(4, value); }
		}

		public bool RenamePublicEvents
		{
			get { return _flags2.IsBitAtIndexOn(5); }
			set { _flags2 = _flags2.SetBitAtIndex(5, value); }
		}

		public bool RenameEnumMembers
		{
			get { return _flags2.IsBitAtIndexOn(6); }
			set { _flags2 = _flags2.SetBitAtIndex(6, value); }
		}

		public bool RenameSerializableMembers
		{
			get { return _flags2.IsBitAtIndexOn(7); }
			set { _flags2 = _flags2.SetBitAtIndex(7, value); }
		}

		public bool RenameConfigurationMembers
		{
			get { return _flags2.IsBitAtIndexOn(8); }
			set { _flags2 = _flags2.SetBitAtIndex(8, value); }
		}

		public bool RenameBindableMembers
		{
			get { return _flags2.IsBitAtIndexOn(9); }
			set { _flags2 = _flags2.SetBitAtIndex(9, value); }
		}

		public bool ObfuscateControlFlow
		{
			get { return _flags2.IsBitAtIndexOn(11); }
			set { _flags2 = _flags2.SetBitAtIndex(11, value); }
		}

		public bool ObfuscateControlFlowAtomic
		{
			get { return _flags2.IsBitAtIndexOn(12); }
			set { _flags2 = _flags2.SetBitAtIndex(12, value); }
		}

		public bool EncryptIL
		{
			get { return _flags2.IsBitAtIndexOn(15); }
			set { _flags2 = _flags2.SetBitAtIndex(15, value); }
		}

		public bool EncryptILAtomic
		{
			get { return _flags2.IsBitAtIndexOn(16); }
			set { _flags2 = _flags2.SetBitAtIndex(16, value); }
		}

		public bool ObfuscateResources
		{
			get { return _flags2.IsBitAtIndexOn(19); }
			set { _flags2 = _flags2.SetBitAtIndex(19, value); }
		}

		public bool ObfuscateStrings
		{
			get { return _flags2.IsBitAtIndexOn(20); }
			set { _flags2 = _flags2.SetBitAtIndex(20, value); }
		}

		public bool SuppressILdasm
		{
			get { return _flags2.IsBitAtIndexOn(21); }
			set { _flags2 = _flags2.SetBitAtIndex(21, value); }
		}

		public bool IgnoreObfuscationAttribute
		{
			get { return _flags2.IsBitAtIndexOn(22); }
			set { _flags2 = _flags2.SetBitAtIndex(22, value); }
		}

		public bool RemoveUnusedMembers
		{
			get { return _flags3.IsBitAtIndexOn(0); }
			set { _flags3 = _flags3.SetBitAtIndex(0, value); }
		}

		public bool RemoveUnusedPublicMembers
		{
			get { return _flags3.IsBitAtIndexOn(1); }
			set { _flags3 = _flags3.SetBitAtIndex(1, value); }
		}

		public bool SealTypes
		{
			get { return _flags3.IsBitAtIndexOn(10); }
			set { _flags3 = _flags3.SetBitAtIndex(10, value); }
		}

		public bool SealPublicTypes
		{
			get { return _flags3.IsBitAtIndexOn(11); }
			set { _flags3 = _flags3.SetBitAtIndex(11, value); }
		}

		public bool DevirtualizeMethods
		{
			get { return _flags3.IsBitAtIndexOn(14); }
			set { _flags3 = _flags3.SetBitAtIndex(14, value); }
		}

		public bool DevirtualizePublicMethods
		{
			get { return _flags3.IsBitAtIndexOn(15); }
			set { _flags3 = _flags3.SetBitAtIndex(15, value); }
		}

		public bool IsSigned
		{
			get { return _sign != null; }
		}

		public ProjectAssemblySign Sign
		{
			get { return _sign; }
			set { _sign = value; }
		}
		
		public Dictionary<string, ProjectModule> Modules
		{
			get
			{
				if (_modules == null)
				{
					_modules = new Dictionary<string, ProjectModule>(StringComparer.OrdinalIgnoreCase);
				}

				return _modules;
			}
		}

		public Dictionary<string, ProjectResource> Resources
		{
			get
			{
				if (_resources == null)
				{
					_resources = new Dictionary<string, ProjectResource>(StringComparer.OrdinalIgnoreCase);
				}

				return _resources;
			}
		}

		#endregion

		#region Methods

		public void Scavenge()
		{
			var state = new ProjectScavengeState(this);

			if (_modules != null && _modules.Count > 0)
			{
				var modules = _modules.ToArray();
				foreach (var kvp in modules)
				{
					var module = kvp.Value;

					module.Scavenge(state);

					if (module.IsEmpty)
						_modules.Remove(kvp.Key);
				}
			}

			if (_resources != null && _resources.Count > 0)
			{
				var resources = _resources.ToArray();
				foreach (var kvp in resources)
				{
					var resource = kvp.Value;

					resource.Scavenge(state);

					if (resource.IsEmpty)
						_resources.Remove(kvp.Key);
				}
			}
		}

		internal void Read(IBinaryAccessor accessor, ProjectReadState state)
		{
			_filePath = ProjectHelper.MakeAbsolutePath(state.GetString(accessor.Read7BitEncodedInt()), state.BasePath);
			_outputPath = ProjectHelper.MakeAbsolutePath(state.GetString(accessor.Read7BitEncodedInt()), state.BasePath);
			_flags = accessor.ReadInt32();
			_flags2 = accessor.ReadInt32();
			_flags3 = accessor.ReadInt32();

			if (NameChanged)
				_name = state.GetString(accessor.Read7BitEncodedInt());

			if (CultureChanged)
				_culture = state.GetString(accessor.Read7BitEncodedInt());

			if (VersionChanged)
				_version = new Version(accessor.ReadUInt16(), accessor.ReadUInt16(), accessor.ReadUInt16(), accessor.ReadUInt16());

			if (TitleChanged)
				_title = state.GetString(accessor.Read7BitEncodedInt());

			if (DescriptionChanged)
				_description = state.GetString(accessor.Read7BitEncodedInt());

			if (CompanyChanged)
				_company = state.GetString(accessor.Read7BitEncodedInt());

			if (ProductChanged)
				_product = state.GetString(accessor.Read7BitEncodedInt());

			if (CopyrightChanged)
				_copyright = state.GetString(accessor.Read7BitEncodedInt());

			if (TrademarkChanged)
				_trademark = state.GetString(accessor.Read7BitEncodedInt());

			if (accessor.ReadBoolean())
				_sign = new ProjectAssemblySign(accessor, state);

			ReadModules(accessor, state);
			ReadResources(accessor, state);
		}

		private void ReadModules(IBinaryAccessor accessor, ProjectReadState state)
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return;

			_modules = new Dictionary<string, ProjectModule>(count, StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < count; i++)
			{
				string name = state.GetString(accessor.Read7BitEncodedInt());
				_modules.Add(name, new ProjectModule(accessor, state));
			}
		}

		private void ReadResources(IBinaryAccessor accessor, ProjectReadState state)
		{
			int count = accessor.Read7BitEncodedInt();
			if (count == 0)
				return;

			_resources = new Dictionary<string, ProjectResource>(count, StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < count; i++)
			{
				string name = state.GetString(accessor.Read7BitEncodedInt());
				_resources.Add(name, new ProjectResource(accessor, state));
			}
		}

		internal void Write(IBinaryAccessor accessor, ProjectWriteState state)
		{
			accessor.Write7BitEncodedInt(state.SetString(ProjectHelper.MakeRelativePath(_filePath, state.BasePath)));
			accessor.Write7BitEncodedInt(state.SetString(ProjectHelper.MakeRelativePath(_outputPath, state.BasePath)));
			accessor.Write((int)_flags);
			accessor.Write((int)_flags2);
			accessor.Write((int)_flags3);

			if (NameChanged)
				accessor.Write7BitEncodedInt(state.SetString(_name));

			if (CultureChanged)
				accessor.Write7BitEncodedInt(state.SetString(_culture));

			if (VersionChanged)
			{
				accessor.Write((ushort)_version.Major);
				accessor.Write((ushort)_version.Minor);
				accessor.Write((ushort)_version.Build);
				accessor.Write((ushort)_version.Revision);
			}

			if (TitleChanged)
				accessor.Write7BitEncodedInt(state.SetString(_title));

			if (DescriptionChanged)
				accessor.Write7BitEncodedInt(state.SetString(_description));

			if (CompanyChanged)
				accessor.Write7BitEncodedInt(state.SetString(_company));

			if (ProductChanged)
				accessor.Write7BitEncodedInt(state.SetString(_product));

			if (CopyrightChanged)
				accessor.Write7BitEncodedInt(state.SetString(_copyright));

			if (TrademarkChanged)
				accessor.Write7BitEncodedInt(state.SetString(_trademark));

			if (_sign != null)
			{
				accessor.Write(true);
				_sign.Write(accessor, state);
			}
			else
			{
				accessor.Write(false);
			}

			WriteModules(accessor, state);
			WriteResources(accessor, state);
		}

		private void WriteModules(IBinaryAccessor accessor, ProjectWriteState state)
		{
			int count = _modules != null ? _modules.Count : 0;
			accessor.Write7BitEncodedInt(count);

			if (count == 0)
				return;

			foreach (var kvp in _modules)
			{
				accessor.Write7BitEncodedInt(state.SetString(kvp.Key));
				kvp.Value.Write(accessor, state);
			}
		}

		private void WriteResources(IBinaryAccessor accessor, ProjectWriteState state)
		{
			int count = _resources != null ? _resources.Count : 0;
			accessor.Write7BitEncodedInt(count);

			if (count == 0)
				return;

			foreach (var kvp in _resources)
			{
				accessor.Write7BitEncodedInt(state.SetString(kvp.Key));
				kvp.Value.Write(accessor, state);
			}
		}

		#endregion
	}
}
