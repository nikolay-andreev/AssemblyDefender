using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Member (field or method) reference descriptors.
	/// </summary>
	public class MemberRefTable : MetadataTable<MemberRefRow>
	{
		internal MemberRefTable(MetadataTableStream stream)
			: base(MetadataTableType.MemberRef, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Class;

				case 1:
					return _rows[index].Name;

				case 2:
					return _rows[index].Signature;

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
						values[i] = _rows[j].Class;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Name;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Signature;
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
					_rows[index].Class = value;
					break;

				case 1:
					_rows[index].Name = value;
					break;

				case 2:
					_rows[index].Signature = value;
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
						_rows[j].Class = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Signature = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref MemberRefRow row)
		{
			row.Class = values[0];
			row.Name = values[1];
			row.Signature = values[2];
		}

		protected override void FillValues(int[] values, ref MemberRefRow row)
		{
			values[0] = row.Class;
			values[1] = row.Name;
			values[2] = row.Signature;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new MemberRefRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new MemberRefRow();
				row.Class = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[5]);
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Signature = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);

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

				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[5], (int)row.Class);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.Signature);
			}
		}
	}
}
