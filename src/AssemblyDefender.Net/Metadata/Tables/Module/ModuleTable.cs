using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The current module descriptor.
	/// </summary>
	public class ModuleTable : MetadataTable<ModuleRow>
	{
		internal ModuleTable(MetadataTableStream stream)
			: base(MetadataTableType.Module, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].Generation;

				case 1:
					return _rows[index].Name;

				case 2:
					return _rows[index].Mvid;

				case 3:
					return _rows[index].EncId;

				case 4:
					return _rows[index].EncBaseId;

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
						values[i] = (int)_rows[j].Generation;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Name;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Mvid;
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].EncId;
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].EncBaseId;
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
					_rows[index].Generation = (ushort)value;
					break;

				case 1:
					_rows[index].Name = value;
					break;

				case 2:
					_rows[index].Mvid = value;
					break;

				case 3:
					_rows[index].EncId = value;
					break;

				case 4:
					_rows[index].EncBaseId = value;
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
						_rows[j].Generation = (ushort)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Mvid = values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].EncId = values[i];
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].EncBaseId = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ModuleRow row)
		{
			row.Generation = (ushort)values[0];
			row.Name = values[1];
			row.Mvid = values[2];
			row.EncId = values[3];
			row.EncBaseId = values[4];
		}

		protected override void FillValues(int[] values, ref ModuleRow row)
		{
			values[0] = (int)row.Generation;
			values[1] = row.Name;
			values[2] = row.Mvid;
			values[3] = row.EncId;
			values[4] = row.EncBaseId;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ModuleRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ModuleRow();
				row.Generation = accessor.ReadUInt16();
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Mvid = accessor.ReadCell(compressionInfo.GuidHeapOffsetSize4);
				row.EncId = accessor.ReadCell(compressionInfo.GuidHeapOffsetSize4);
				row.EncBaseId = accessor.ReadCell(compressionInfo.GuidHeapOffsetSize4);

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

				blob.Write(ref pos, (ushort)row.Generation);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
				blob.WriteCell(ref pos, compressionInfo.GuidHeapOffsetSize4, (int)row.Mvid);
				blob.WriteCell(ref pos, compressionInfo.GuidHeapOffsetSize4, (int)row.EncId);
				blob.WriteCell(ref pos, compressionInfo.GuidHeapOffsetSize4, (int)row.EncBaseId);
			}
		}
	}
}
