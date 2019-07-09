using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The CustomAttribute table stores data that can be used to instantiate a Custom Attribute (more precisely,
	/// an object of the specified Custom Attribute class) at runtime.
	/// /// A row in the CustomAttribute table for a parent is created by the .custom attribute, which gives the value
	/// of the Type column and optionally that of the Value column.
	/// </summary>
	public class CustomAttributeTable : MetadataTable<CustomAttributeRow>
	{
		internal CustomAttributeTable(MetadataTableStream stream)
			: base(MetadataTableType.CustomAttribute, stream)
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
					return _rows[index].Type;

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
						values[i] = _rows[j].Parent;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Type;
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
					_rows[index].Parent = value;
					break;

				case 1:
					_rows[index].Type = value;
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
						_rows[j].Parent = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Type = values[i];
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

		protected override void FillRow(int[] values, ref CustomAttributeRow row)
		{
			row.Parent = values[0];
			row.Type = values[1];
			row.Value = values[2];
		}

		protected override void FillValues(int[] values, ref CustomAttributeRow row)
		{
			values[0] = row.Parent;
			values[1] = row.Type;
			values[2] = row.Value;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new CustomAttributeRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new CustomAttributeRow();
				row.Parent = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[2]);
				row.Type = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[10]);
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

				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[2], (int)row.Parent);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[10], (int)row.Type);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.Value);
			}
		}
	}
}
