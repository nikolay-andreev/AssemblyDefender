using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Field layout descriptors that specify the offset or ordinal of individual fields. A row in the FieldLayout
	/// table is created if the .field directive for the parent field has specified a field offset.
	/// </summary>
	public class FieldLayoutTable : MetadataTable<FieldLayoutRow>
	{
		internal FieldLayoutTable(MetadataTableStream stream)
			: base(MetadataTableType.FieldLayout, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].OffSet;

				case 1:
					return _rows[index].Field;

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
						values[i] = _rows[j].OffSet;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Field;
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
					_rows[index].OffSet = value;
					break;

				case 1:
					_rows[index].Field = value;
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
						_rows[j].OffSet = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Field = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref FieldLayoutRow row)
		{
			row.OffSet = values[0];
			row.Field = values[1];
		}

		protected override void FillValues(int[] values, ref FieldLayoutRow row)
		{
			values[0] = row.OffSet;
			values[1] = row.Field;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new FieldLayoutRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new FieldLayoutRow();
				row.OffSet = accessor.ReadInt32();
				row.Field = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.Field]);

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

				blob.Write(ref pos, (int)row.OffSet);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.Field], (int)row.Field);
			}
		}
	}
}
