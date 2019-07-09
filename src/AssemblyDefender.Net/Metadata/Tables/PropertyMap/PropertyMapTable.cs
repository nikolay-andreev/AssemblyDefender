using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// A class-to-properties mapping table. This is not an intermediate
	/// lookup table, and it does exist in optimized metadata.
	/// </summary>
	public class PropertyMapTable : MetadataTable<PropertyMapRow>
	{
		internal PropertyMapTable(MetadataTableStream stream)
			: base(MetadataTableType.PropertyMap, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Parent;

				case 1:
					return _rows[index].PropertyList;

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
						values[i] = _rows[j].Parent;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].PropertyList;
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
					_rows[index].Parent = value;
					break;

				case 1:
					_rows[index].PropertyList = value;
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
						_rows[j].Parent = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].PropertyList = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref PropertyMapRow row)
		{
			row.Parent = values[0];
			row.PropertyList = values[1];
		}

		protected override void FillValues(int[] values, ref PropertyMapRow row)
		{
			values[0] = row.Parent;
			values[1] = row.PropertyList;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new PropertyMapRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new PropertyMapRow();
				row.Parent = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef]);
				row.PropertyList = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.Property]);

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

				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef], (int)row.Parent);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.Property], (int)row.PropertyList);
			}
		}
	}
}
