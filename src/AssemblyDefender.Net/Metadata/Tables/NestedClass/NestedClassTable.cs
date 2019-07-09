using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Nested class descriptors that provide mapping of nested classes to their
	/// respective enclosing classes.
	/// </summary>
	public class NestedClassTable : MetadataTable<NestedClassRow>
	{
		internal NestedClassTable(MetadataTableStream stream)
			: base(MetadataTableType.NestedClass, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].NestedClass;

				case 1:
					return _rows[index].EnclosingClass;

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
						values[i] = _rows[j].NestedClass;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].EnclosingClass;
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
					_rows[index].NestedClass = value;
					break;

				case 1:
					_rows[index].EnclosingClass = value;
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
						_rows[j].NestedClass = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].EnclosingClass = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref NestedClassRow row)
		{
			row.NestedClass = values[0];
			row.EnclosingClass = values[1];
		}

		protected override void FillValues(int[] values, ref NestedClassRow row)
		{
			values[0] = row.NestedClass;
			values[1] = row.EnclosingClass;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new NestedClassRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new NestedClassRow();
				row.NestedClass = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef]);
				row.EnclosingClass = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef]);

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

				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef], (int)row.NestedClass);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef], (int)row.EnclosingClass);
			}
		}
	}
}
