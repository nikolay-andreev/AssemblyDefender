using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Conceptually, each row in the FieldRVA table is an extension to exactly one row in the Field table, and records
	/// the RVA (Relative Virtual Address) within the image file at which this field's initial value is stored.
	/// /// A row in the FieldRVA table is created for each static parent field that has specified the optional data
	/// The RVA column is the relative virtual address of the data in the PE file.
	/// </summary>
	public class FieldRVATable : MetadataTable<FieldRVARow>
	{
		internal FieldRVATable(MetadataTableStream stream)
			: base(MetadataTableType.FieldRVA, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].RVA;

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
						values[i] = (int)_rows[j].RVA;
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
					_rows[index].RVA = (uint)value;
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
						_rows[j].RVA = (uint)values[i];
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

		protected override void FillRow(int[] values, ref FieldRVARow row)
		{
			row.RVA = (uint)values[0];
			row.Field = values[1];
		}

		protected override void FillValues(int[] values, ref FieldRVARow row)
		{
			values[0] = (int)row.RVA;
			values[1] = row.Field;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new FieldRVARow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new FieldRVARow();
				row.RVA = accessor.ReadUInt32();
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

				blob.Write(ref pos, (uint)row.RVA);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.Field], (int)row.Field);
			}
		}
	}
}
