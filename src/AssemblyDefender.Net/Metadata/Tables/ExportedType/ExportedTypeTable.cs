using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Exported type descriptors that contain information about public
	/// classes exported by the current assembly, which are declared in other modules of the
	/// assembly. Only the prime module of the assembly should carry this table.
	/// </summary>
	public class ExportedTypeTable : MetadataTable<ExportedTypeRow>
	{
		internal ExportedTypeTable(MetadataTableStream stream)
			: base(MetadataTableType.ExportedType, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Flags;

				case 1:
					return _rows[index].TypeDefId;

				case 2:
					return _rows[index].TypeName;

				case 3:
					return _rows[index].TypeNamespace;

				case 4:
					return _rows[index].Implementation;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		public override void Get(int rid, int column, int count, int[] values)
		{
			switch (column)
			{
				case 0:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Flags;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].TypeDefId;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].TypeName;
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].TypeNamespace;
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Implementation;
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		public override void Update(int rid, int column, int value)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					_rows[index].Flags = value;
					break;

				case 1:
					_rows[index].TypeDefId = value;
					break;

				case 2:
					_rows[index].TypeName = value;
					break;

				case 3:
					_rows[index].TypeNamespace = value;
					break;

				case 4:
					_rows[index].Implementation = value;
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		public override void Update(int rid, int column, int count, int[] values)
		{
			switch (column)
			{
				case 0:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Flags = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].TypeDefId = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].TypeName = values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].TypeNamespace = values[i];
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Implementation = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ExportedTypeRow row)
		{
			row.Flags = values[0];
			row.TypeDefId = values[1];
			row.TypeName = values[2];
			row.TypeNamespace = values[3];
			row.Implementation = values[4];
		}

		protected override void FillValues(int[] values, ref ExportedTypeRow row)
		{
			values[0] = row.Flags;
			values[1] = row.TypeDefId;
			values[2] = row.TypeName;
			values[3] = row.TypeNamespace;
			values[4] = row.Implementation;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ExportedTypeRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ExportedTypeRow();
				row.Flags = (int)accessor.ReadUInt32();
				row.TypeDefId = accessor.ReadInt32();
				row.TypeName = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.TypeNamespace = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Implementation = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[9]);

				rows[i] = row;
			}

			_count = count;
			_rows = rows;
		}

		protected internal override void Write(Blob blob, ref int pos, TableCompressionInfo compressionInfo)
		{
			for (int i = 0; i < _count; i++)
			{
				var row = _rows[i];

				blob.Write(ref pos, (uint)row.Flags);
				blob.Write(ref pos, row.TypeDefId);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.TypeName);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.TypeNamespace);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[9], (int)row.Implementation);
			}
		}
	}
}
