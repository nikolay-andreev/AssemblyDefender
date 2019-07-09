using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.PE;

namespace AssemblyDefender.Net
{
	/// <summary>
	/// Import _CorExeMain or _CorDllMain from mscoree.dll and create native blob that jumps to
	/// imported method.
	/// </summary>
	public class CorMainStubBuilder : BuildTask
	{
		#region Fields

		private bool _noCorStub;
		private BuildBlob _blob;
		private BaseRelocationBuilder.BuildTable _relocTable;
		private string _sectionName = PESectionNames.Text;
		private int _blobPriority = 57000;

		#endregion

		#region Properties

		public bool NoCorStub
		{
			get { return _noCorStub; }
			set { _noCorStub = value; }
		}

		public BuildBlob Blob
		{
			get { return _blob; }
		}

		public string SectionName
		{
			get { return _sectionName; }
			set { _sectionName = value; }
		}

		public int BlobPriority
		{
			get { return _blobPriority; }
			set { _blobPriority = value; }
		}

		#endregion

		#region Methods

		public override void Build()
		{
			if (_noCorStub)
				return;

			_blob = new BuildBlob();

			var relocBuilder = PE.Tasks.Get<BaseRelocationBuilder>(true);
			_relocTable = relocBuilder.GetOrCreateTable();

			DoBuild();

			// Add blob
			var section = PE.GetSection(_sectionName);
			section.Blobs.Add(_blob, _blobPriority);
		}

		private void DoBuild()
		{
			var importBuilder = PE.Tasks.Get<ImportBuilder>(true);
			var importTable = importBuilder.GetOrCreateTable();

			var module = importTable.FirstOrDefault(m => m.DllName == "mscoree.dll");
			if (module == null)
			{
				module = new ImportModuleTable();
				module.DllName = "mscoree.dll";
				importTable.Add(module);
			}

			string name;
			if ((PE.Characteristics & ImageCharacteristics.DLL) == ImageCharacteristics.DLL)
				name = "_CorDllMain";
			else
				name = "_CorExeMain";

			var entry = module.FirstOrDefault(e => e.Name == name);
			if (entry == null)
			{
				entry = new ImportEntry();
				entry.Name = name;
				module.Add(entry);
			}

			switch (PE.Machine)
			{
				case MachineType.I386:
					BuildX86CorStub(entry);
					break;

				case MachineType.IA64:
					BuildIA64CorStub(entry);
					break;

				case MachineType.AMD64:
					BuildAMD64CorStub(entry);
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private void BuildX86CorStub(ImportEntry entry)
		{
			int pos = 0;

			// jmp
			_blob.Write(ref pos, new byte[] { 0xFF, 0x25 });

			// VA of IAT
			_relocTable.Add(_blob, pos, BaseRelocationType.HIGHLOW);
			_blob.Write(ref pos, (uint)0);

			PE.Fixups.Add(new X86CorStubFixup(_blob, entry));
		}

		private void BuildIA64CorStub(ImportEntry entry)
		{
			int pos = 0;

			_blob.OffsetAlignment = 0x10;

			// IA64 stub
			// ld8    r9  = [gp]    ;;
			// ld8    r10 = [r9],8
			// nop.i                ;;
			// ld8    gp  = [r9]
			// mov    b6  = r10
			// br.cond.sptk.few  b6
			_blob.Write(ref pos,
				new byte[]
				{
					0x0B, 0x48, 0x00, 0x02, 0x18, 0x10, 0xA0, 0x40,
					0x24, 0x30, 0x28, 0x00, 0x00, 0x00, 0x04, 0x00,
					0x10, 0x08, 0x00, 0x12, 0x18, 0x10, 0x60, 0x50,
					0x04, 0x80, 0x03, 0x00, 0x60, 0x00, 0x80, 0x00,
				});

			// VA of IA64 stub
			_relocTable.Add(_blob, pos, BaseRelocationType.DIR64);
			_blob.Write(ref pos, (ulong)0);

			// VA of IAT
			_relocTable.Add(_blob, pos, BaseRelocationType.DIR64);
			_blob.Write(ref pos, (ulong)0);

			PE.Fixups.Add(new IA64CorStubFixup(_blob, entry));
		}

		private void BuildAMD64CorStub(ImportEntry entry)
		{
			int pos = 0;

			// rex.w rex.b mov rax,[following address]
			_blob.Write(ref pos, new byte[] { 0x48, 0xA1 });

			// VA of IAT
			_relocTable.Add(_blob, pos, BaseRelocationType.DIR64);
			_blob.Write(ref pos, (ulong)0);

			// jmp [rax]
			_blob.Write(ref pos, new byte[] { 0xFF, 0xE0 });

			PE.Fixups.Add(new AMD64CorStubFixup(_blob, entry));
		}

		#endregion

		#region Nested types

		private class X86CorStubFixup : BuildFixup
		{
			private BuildBlob _blob;
			private ImportEntry _entry;

			public X86CorStubFixup(BuildBlob blob, ImportEntry entry)
			{
				_blob = blob;
				_entry = entry;
			}

			public override void ApplyFixup()
			{
				var importBuilder = PE.Tasks.Get<ImportBuilder>(true);

				// Fix VA of IAT
				var iatBlob = importBuilder.IATBlob;
				if (iatBlob == null)
					return;

				uint VA =
					(uint)iatBlob.RVA +
					(uint)PE.ImageBase +
					(uint)importBuilder.GetIATOffset(_entry);

				int pos = 2;
				_blob.Write(ref pos, (uint)VA);

				// Set entry point
				PE.AddressOfEntryPoint = _blob.RVA;
			}
		}

		private class IA64CorStubFixup : BuildFixup
		{
			private BuildBlob _blob;
			private ImportEntry _entry;

			public IA64CorStubFixup(BuildBlob blob, ImportEntry entry)
			{
				_blob = blob;
				_entry = entry;
			}

			public override void ApplyFixup()
			{
				var importBuilder = PE.Tasks.Get<ImportBuilder>(true);

				// Fix IA64 stub
				{
					ulong VA = _blob.RVA + PE.ImageBase;

					int pos = 32;
					_blob.Write(ref pos, (ulong)VA);
				}

				// Fix VA of IAT
				{
					var iatBlob = importBuilder.IATBlob;
					if (iatBlob == null)
						return;

					ulong VA =
						(ulong)iatBlob.RVA +
						(ulong)PE.ImageBase +
						(ulong)importBuilder.GetIATOffset(_entry);

					int pos = 40;
					_blob.Write(ref pos, (ulong)VA);
				}

				// Set entry point
				PE.AddressOfEntryPoint = _blob.RVA + 32;
			}
		}

		private class AMD64CorStubFixup : BuildFixup
		{
			private BuildBlob _blob;
			private ImportEntry _entry;

			public AMD64CorStubFixup(BuildBlob blob, ImportEntry entry)
			{
				_blob = blob;
				_entry = entry;
			}

			public override void ApplyFixup()
			{
				var importBuilder = PE.Tasks.Get<ImportBuilder>(true);

				// Fix VA of IAT
				var iatBlob = importBuilder.IATBlob;
				if (iatBlob == null)
					return;

				ulong VA =
					(ulong)iatBlob.RVA +
					(ulong)PE.ImageBase +
					(ulong)importBuilder.GetIATOffset(_entry);

				int pos = 2;
				_blob.Write(ref pos, (ulong)VA);

				// Set entry point
				PE.AddressOfEntryPoint = _blob.RVA;
			}
		}

		#endregion
	}
}
