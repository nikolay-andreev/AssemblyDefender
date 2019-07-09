using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The ClassLayout table is used to define how the fields of a class or value type shall be laid out by the CLI.
	/// (Normally, the CLI is free to reorder and/or insert gaps between the fields defined for a class or value type.)
	/// /// This feature is used to lay out a managed value type in exactly the same way as an unmanaged C struct,
	/// allowing a managed value type to be handed to unmanaged code, which then accesses the fields exactly as if
	/// that block of memory had been laid out by unmanaged code.
	/// </summary>
	public class ClassLayoutTable : MetadataTable<ClassLayoutRow>
	{
		internal ClassLayoutTable(MetadataTableStream stream)
			: base(MetadataTableType.ClassLayout, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].PackingSize;

				case 1:
					return _rows[index].ClassSize;

				case 2:
					return _rows[index].Parent;

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
						values[i] = _rows[j].PackingSize;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].ClassSize;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Parent;
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
					_rows[index].PackingSize = value;
					break;

				case 1:
					_rows[index].ClassSize = value;
					break;

				case 2:
					_rows[index].Parent = value;
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
						_rows[j].PackingSize = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].ClassSize = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Parent = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ClassLayoutRow row)
		{
			row.PackingSize = values[0];
			row.ClassSize = values[1];
			row.Parent = values[2];
		}

		protected override void FillValues(int[] values, ref ClassLayoutRow row)
		{
			values[0] = row.PackingSize;
			values[1] = row.ClassSize;
			values[2] = row.Parent;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ClassLayoutRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ClassLayoutRow();
				row.PackingSize = (int)accessor.ReadUInt16();
				row.ClassSize = accessor.ReadInt32();
				row.Parent = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef]);

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

				blob.Write(ref pos, (ushort)row.PackingSize);
				blob.Write(ref pos, (int)row.ClassSize);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef], (int)row.Parent);
			}
		}
	}
}
