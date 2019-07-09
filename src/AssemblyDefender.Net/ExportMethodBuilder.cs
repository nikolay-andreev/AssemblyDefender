using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class ExportMethodBuilder : BuildTask
	{
		#region Fields

		private BuildTable _table;
		private BuildBlob _codeBlob;
		private string _sectionName = PESectionNames.Text;
		private int _blobPriority = 1000;
		private State _state;

		#endregion

		#region Properties

		public BuildTable Table
		{
			get { return _table; }
			set { _table = value; }
		}

		public BuildBlob CodeBlob
		{
			get { return _codeBlob; }
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
			if (_table == null)
				return;

			_state = new State();
			_codeBlob = new BuildBlob();

			Initialize();

			switch (PE.Machine)
			{
				case MachineType.I386:
					BuildX86();
					break;

				case MachineType.IA64:
					BuildIA64();
					break;

				case MachineType.AMD64:
					BuildAMD64();
					break;

				default:
					throw new NotImplementedException();
			}

			// Add blobs
			var section = PE.GetSection(_sectionName);
			section.Blobs.Add(_codeBlob, _blobPriority);

			// Change CLI flags
			var corHeaderBuilder = PE.Tasks.Get<CorHeaderBuilder>(true);
			if (PE.Is32Bits)
			{
				corHeaderBuilder.Flags &= ~CorFlags.ILOnly;
				corHeaderBuilder.Flags |= CorFlags.F32BitsRequired;
			}
			else
			{
				corHeaderBuilder.Flags &= ~CorFlags.ILOnly;
			}

			_state = null;
		}

		private void Initialize()
		{
			// Method signature/rid mapping
			var moduleBuilder = PE.Tasks.Get<ModuleBuilder>(true);
			_state.MethodDefHash = moduleBuilder.MethodDefHash;

			// VTableFixup
			var vtableFixupBuilder = PE.Tasks.Get<VTableFixupBuilder>(true);
			_state.VTableFixupTable = vtableFixupBuilder.GetOrCreateTable();

			// BaseRelocation
			var relocBuilder = PE.Tasks.Get<BaseRelocationBuilder>(true);
			_state.RelocTable = relocBuilder.GetOrCreateTable();

			// Export
			var exportBuilder = PE.Tasks.Get<ExportBuilder>(true);
			_state.ExportTable = exportBuilder.Table;
			if (_state.ExportTable == null)
			{
				_state.ExportTable = new ExportBuilder.BuildTable();
				_state.ExportTable.DllName = moduleBuilder.Module.Name;
				exportBuilder.Table = _state.ExportTable;
			}
		}

		private void BuildX86()
		{
			foreach (var entry in _table)
			{
				BuildX86(entry);
			}
		}

		private void BuildX86(BuildEntry entry)
		{
			int methodRID = _state.MethodDefHash.IndexOf(entry.MethodSignature) + 1;

			// Add fixup entry
			var fixup = new VTableFixupEntry();
			fixup.Type = VTableFixupType.SlotSize32Bit | VTableFixupType.FromUnmanaged;
			fixup.Add(MetadataToken.Get(MetadataTokenType.Method, methodRID));
			_state.VTableFixupTable.Add(fixup);

			int pos = _codeBlob.Length;
			if (pos > 0)
				pos = pos.Align(0x10);

			PE.Fixups.Add(new X86Fixup(_codeBlob, pos, fixup));

			// jmp
			_state.ExportTable.AddRVAEntry(entry.MethodName, _codeBlob, pos);
			_codeBlob.Write(ref pos, new byte[] { 0xFF, 0x25 });

			// VA of method (through VTable)
			_state.RelocTable.Add(_codeBlob, pos, BaseRelocationType.HIGHLOW);
			_codeBlob.Write(ref pos, (uint)0);
		}

		private void BuildIA64()
		{
			foreach (var entry in _table)
			{
				BuildIA64(entry);
			}
		}

		private void BuildIA64(BuildEntry entry)
		{
			int methodRID = _state.MethodDefHash.IndexOf(entry.MethodSignature) + 1;

			// Add fixup entry
			var fixup = new VTableFixupEntry();
			fixup.Type = VTableFixupType.SlotSize64Bit | VTableFixupType.FromUnmanaged;
			fixup.Add(MetadataToken.Get(MetadataTokenType.Method, methodRID));
			_state.VTableFixupTable.Add(fixup);

			int pos = _codeBlob.Length;
			if (pos > 0)
				pos = pos.Align(0x10);

			PE.Fixups.Add(new IA64Fixup(_codeBlob, pos, fixup));

			// IA64 stub
			// ld8    r9  = [gp]    ;;
			// ld8    r10 = [r9],8
			// nop.i                ;;
			// ld8    gp  = [r9]
			// mov    b6  = r10
			// br.cond.sptk.few  b6
			_codeBlob.Write(ref pos,
				new byte[]
				{
					0x0B, 0x48, 0x00, 0x02, 0x18, 0x10, 0xA0, 0x40,
					0x24, 0x30, 0x28, 0x00, 0x00, 0x00, 0x04, 0x00,
					0x10, 0x08, 0x00, 0x12, 0x18, 0x10, 0x60, 0x50,
					0x04, 0x80, 0x03, 0x00, 0x60, 0x00, 0x80, 0x00,
				});

			// VA of IA64 stub
			_state.ExportTable.AddRVAEntry(entry.MethodName, _codeBlob, pos);
			_state.RelocTable.Add(_codeBlob, pos, BaseRelocationType.DIR64);
			_codeBlob.Write(ref pos, (ulong)0);

			// VA of method (through VTable)
			_state.RelocTable.Add(_codeBlob, pos, BaseRelocationType.DIR64);
			_codeBlob.Write(ref pos, (ulong)0);
		}

		private void BuildAMD64()
		{
			foreach (var entry in _table)
			{
				BuildAMD64(entry);
			}
		}

		private void BuildAMD64(BuildEntry entry)
		{
			int methodRID = _state.MethodDefHash.IndexOf(entry.MethodSignature) + 1;

			// Add fixup entry
			var fixup = new VTableFixupEntry();
			fixup.Type = VTableFixupType.SlotSize64Bit | VTableFixupType.FromUnmanaged;
			fixup.Add(MetadataToken.Get(MetadataTokenType.Method, methodRID));
			_state.VTableFixupTable.Add(fixup);

			int pos = _codeBlob.Length;
			if (pos > 0)
				pos = pos.Align(0x10);

			PE.Fixups.Add(new AMD64Fixup(_codeBlob, pos, fixup));

			// rex.w rex.b mov rax,[following address]
			_state.ExportTable.AddRVAEntry(entry.MethodName, _codeBlob, pos);
			_codeBlob.Write(ref pos, new byte[] { 0x48, 0xA1 });

			// VA of method (through VTable)
			_state.RelocTable.Add(_codeBlob, pos, BaseRelocationType.DIR64);
			_codeBlob.Write(ref pos, (ulong)0);

			// jmp [rax]
			_codeBlob.Write(ref pos, new byte[] { 0xFF, 0xE0 });
		}

		#endregion

		#region Nested types

		public class BuildTable : IEnumerable<BuildEntry>
		{
			#region Fields

			private List<BuildEntry> _list = new List<BuildEntry>();

			#endregion

			#region Ctors

			public BuildTable()
			{
			}

			#endregion

			#region Properties

			public BuildEntry this[int index]
			{
				get { return _list[index]; }
			}

			public int Count
			{
				get { return _list.Count; }
			}

			#endregion

			#region Methods

			public void Add(BuildEntry item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				_list.Add(item);
			}

			public BuildEntry Add(MethodDeclaration method)
			{
				var entry = new BuildEntry()
				{
					MethodName = method.Name,
					MethodSignature = method.ToReference(method.Module),
				};

				_list.Add(entry);

				return entry;
			}

			public BuildEntry Add(MethodReference methodRef)
			{
				var entry = new BuildEntry()
				{
					MethodName = methodRef.Name,
					MethodSignature = methodRef,
				};

				_list.Add(entry);

				return entry;
			}

			public IEnumerator<BuildEntry> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			#endregion
		}

		public class BuildEntry
		{
			public string MethodName
			{
				get;
				internal set;
			}

			public MethodReference MethodSignature
			{
				get;
				internal set;
			}
		}

		private class X86Fixup : BuildFixup
		{
			private BuildBlob _codeBlob;
			private int _codeBlobPos;
			private VTableFixupEntry _fixup;

			public X86Fixup(BuildBlob codeBlob, int codeBlobPos, VTableFixupEntry fixup)
			{
				_codeBlob = codeBlob;
				_codeBlobPos = codeBlobPos;
				_fixup = fixup;
			}

			public override void ApplyFixup()
			{
				var vtableFixupBuilder = PE.Tasks.Get<VTableFixupBuilder>(true);

				// Fix VA of method
				var fixupBlob = vtableFixupBuilder.DataBlob;

				uint VA =
					(uint)fixupBlob.RVA +
					(uint)PE.ImageBase +
					(uint)vtableFixupBuilder.GetFixupOffset(_fixup);

				int pos = _codeBlobPos + 2;
				_codeBlob.Write(ref pos, (uint)VA);
			}
		}

		private class IA64Fixup : BuildFixup
		{
			private BuildBlob _codeBlob;
			private int _codeBlobPos;
			private VTableFixupEntry _fixup;

			public IA64Fixup(BuildBlob codeBlob, int codeBlobPos, VTableFixupEntry fixup)
			{
				_codeBlob = codeBlob;
				_codeBlobPos = codeBlobPos;
				_fixup = fixup;
			}

			public override void ApplyFixup()
			{
				var vtableFixupBuilder = PE.Tasks.Get<VTableFixupBuilder>(true);

				// Fix IA64 stub
				{
					ulong VA = _codeBlob.RVA + PE.ImageBase;

					int pos = _codeBlobPos + 32;
					_codeBlob.Write(ref pos, (ulong)VA);
				}

				// Fix VA of method
				{
					var fixupBlob = vtableFixupBuilder.DataBlob;

					ulong VA =
						(ulong)fixupBlob.RVA +
						(ulong)PE.ImageBase +
						(ulong)vtableFixupBuilder.GetFixupOffset(_fixup);

					int pos = _codeBlobPos + 40;
					_codeBlob.Write(ref pos, (ulong)VA);
				}
			}
		}

		private class AMD64Fixup : BuildFixup
		{
			private BuildBlob _codeBlob;
			private int _codeBlobPos;
			private VTableFixupEntry _fixup;

			public AMD64Fixup(BuildBlob codeBlob, int codeBlobPos, VTableFixupEntry fixup)
			{
				_codeBlob = codeBlob;
				_codeBlobPos = codeBlobPos;
				_fixup = fixup;
			}

			public override void ApplyFixup()
			{
				var vtableFixupBuilder = PE.Tasks.Get<VTableFixupBuilder>(true);

				// Fix VA of method
				var fixupBlob = vtableFixupBuilder.DataBlob;

				ulong VA =
					(ulong)fixupBlob.RVA +
					(ulong)PE.ImageBase +
					(ulong)vtableFixupBuilder.GetFixupOffset(_fixup);

				int pos = _codeBlobPos + 2;
				_codeBlob.Write(ref pos, (ulong)VA);
			}
		}

		private class State
		{
			public VTableFixupTable VTableFixupTable;
			public BaseRelocationBuilder.BuildTable RelocTable;
			public ExportBuilder.BuildTable ExportTable;
			public HashList<IMethodSignature> MethodDefHash;
		}

		#endregion
	}
}
