using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Type parameter descriptors for generic (parameterized) classes and methods.
	/// </summary>
	public class GenericParamTable : MetadataTable<GenericParamRow>
	{
		internal GenericParamTable(MetadataTableStream stream)
			: base(MetadataTableType.GenericParam, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Number;

				case 1:
					return (int)_rows[index].Flags;

				case 2:
					return _rows[index].Owner;

				case 3:
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
						values[i] = _rows[j].Number;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = (int)_rows[j].Flags;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Owner;
					}
					break;

				case 3:
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
					_rows[index].Number = value;
					break;

				case 1:
					_rows[index].Flags = (ushort)value;
					break;

				case 2:
					_rows[index].Owner = value;
					break;

				case 3:
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
						_rows[j].Number = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Flags = (ushort)values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Owner = values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref GenericParamRow row)
		{
			row.Number = values[0];
			row.Flags = (ushort)values[1];
			row.Owner = values[2];
			row.Name = values[3];
		}

		protected override void FillValues(int[] values, ref GenericParamRow row)
		{
			values[0] = row.Number;
			values[1] = (int)row.Flags;
			values[2] = row.Owner;
			values[3] = row.Name;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new GenericParamRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new GenericParamRow();
				row.Number = (int)accessor.ReadUInt16();
				row.Flags = accessor.ReadUInt16();
				row.Owner = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[12]);
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

				blob.Write(ref pos, (ushort)row.Number);
				blob.Write(ref pos, (ushort)row.Flags);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[12], (int)row.Owner);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
			}
		}
	}
}
