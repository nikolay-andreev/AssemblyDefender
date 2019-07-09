using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Parameter definition descriptors.
	/// </summary>
	public class ParamTable : MetadataTable<ParamRow>
	{
		internal ParamTable(MetadataTableStream stream)
			: base(MetadataTableType.Param, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].Flags;

				case 1:
					return (int)_rows[index].Sequence;

				case 2:
					return _rows[index].Name;

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
						values[i] = (int)_rows[j].Flags;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = (int)_rows[j].Sequence;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Name;
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
					_rows[index].Flags = (ushort)value;
					break;

				case 1:
					_rows[index].Sequence = (ushort)value;
					break;

				case 2:
					_rows[index].Name = value;
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
						_rows[j].Flags = (ushort)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Sequence = (ushort)values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ParamRow row)
		{
			row.Flags = (ushort)values[0];
			row.Sequence = (ushort)values[1];
			row.Name = values[2];
		}

		protected override void FillValues(int[] values, ref ParamRow row)
		{
			values[0] = (int)row.Flags;
			values[1] = (int)row.Sequence;
			values[2] = row.Name;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ParamRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ParamRow();
				row.Flags = accessor.ReadUInt16();
				row.Sequence = accessor.ReadUInt16();
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);

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

				blob.Write(ref pos, (ushort)row.Flags);
				blob.Write(ref pos, (ushort)row.Sequence);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
			}
		}
	}
}
