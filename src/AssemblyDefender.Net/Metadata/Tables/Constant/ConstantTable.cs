using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The Constant table is used to store compile-time, constant values for fields, parameters, and properties.
	/// </summary>
	public class ConstantTable : MetadataTable<ConstantRow>
	{
		internal ConstantTable(MetadataTableStream stream)
			: base(MetadataTableType.Constant, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].Type;

				case 1:
					return _rows[index].Parent;

				case 2:
					return _rows[index].Value;

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
						values[i] = (int)_rows[j].Type;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Parent;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Value;
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
					_rows[index].Type = (ConstantTableType)value;
					break;

				case 1:
					_rows[index].Parent = value;
					break;

				case 2:
					_rows[index].Value = value;
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
						_rows[j].Type = (ConstantTableType)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Parent = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Value = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ConstantRow row)
		{
			row.Type = (ConstantTableType)values[0];
			row.Parent = values[1];
			row.Value = values[2];
		}

		protected override void FillValues(int[] values, ref ConstantRow row)
		{
			values[0] = (int)row.Type;
			values[1] = row.Parent;
			values[2] = row.Value;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ConstantRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ConstantRow();
				row.Type = (ConstantTableType)accessor.ReadByte();
				accessor.ReadByte(); // Padding
				row.Parent = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[1]);
				row.Value = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);

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

				blob.Write(ref pos, (byte)row.Type);
				blob.Write(ref pos, (byte)0); // Padding
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[1], (int)row.Parent);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.Value);
			}
		}
	}
}
