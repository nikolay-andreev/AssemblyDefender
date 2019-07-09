using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Field or parameter marshaling descriptors for managed/unmanaged interoperations.
	/// </summary>
	public class FieldMarshalTable : MetadataTable<FieldMarshalRow>
	{
		internal FieldMarshalTable(MetadataTableStream stream)
			: base(MetadataTableType.FieldMarshal, stream)
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
					return _rows[index].NativeType;

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
						values[i] = _rows[j].NativeType;
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
					_rows[index].NativeType = value;
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
						_rows[j].NativeType = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref FieldMarshalRow row)
		{
			row.Parent = values[0];
			row.NativeType = values[1];
		}

		protected override void FillValues(int[] values, ref FieldMarshalRow row)
		{
			values[0] = row.Parent;
			values[1] = row.NativeType;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new FieldMarshalRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new FieldMarshalRow();
				row.Parent = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[3]);
				row.NativeType = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);

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

				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[3], (int)row.Parent);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.NativeType);
			}
		}
	}
}
