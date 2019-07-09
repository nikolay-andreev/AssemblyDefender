using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Implementation map descriptors used for the platform invocation
	/// (P/Invoke) type of managed/unmanaged code interoperation.
	/// </summary>
	public class ImplMapTable : MetadataTable<ImplMapRow>
	{
		internal ImplMapTable(MetadataTableStream stream)
			: base(MetadataTableType.ImplMap, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].MappingFlags;

				case 1:
					return _rows[index].MemberForwarded;

				case 2:
					return _rows[index].ImportName;

				case 3:
					return _rows[index].ImportScope;

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
						values[i] = (int)_rows[j].MappingFlags;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].MemberForwarded;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].ImportName;
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].ImportScope;
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
					_rows[index].MappingFlags = (ushort)value;
					break;

				case 1:
					_rows[index].MemberForwarded = value;
					break;

				case 2:
					_rows[index].ImportName = value;
					break;

				case 3:
					_rows[index].ImportScope = value;
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
						_rows[j].MappingFlags = (ushort)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].MemberForwarded = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].ImportName = values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].ImportScope = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ImplMapRow row)
		{
			row.MappingFlags = (ushort)values[0];
			row.MemberForwarded = values[1];
			row.ImportName = values[2];
			row.ImportScope = values[3];
		}

		protected override void FillValues(int[] values, ref ImplMapRow row)
		{
			values[0] = (int)row.MappingFlags;
			values[1] = row.MemberForwarded;
			values[2] = row.ImportName;
			values[3] = row.ImportScope;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ImplMapRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ImplMapRow();
				row.MappingFlags = accessor.ReadUInt16();
				row.MemberForwarded = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[8]);
				row.ImportName = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.ImportScope = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.ModuleRef]);

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

				blob.Write(ref pos, (ushort)row.MappingFlags);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[8], (int)row.MemberForwarded);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.ImportName);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.ModuleRef], (int)row.ImportScope);
			}
		}
	}
}
