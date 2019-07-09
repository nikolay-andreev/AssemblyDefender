using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Generic method instantiation descriptors.
	/// </summary>
	public class MethodSpecTable : MetadataTable<MethodSpecRow>
	{
		internal MethodSpecTable(MetadataTableStream stream)
			: base(MetadataTableType.MethodSpec, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Method;

				case 1:
					return _rows[index].Instantiation;

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
						values[i] = _rows[j].Method;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Instantiation;
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
					_rows[index].Method = value;
					break;

				case 1:
					_rows[index].Instantiation = value;
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
						_rows[j].Method = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Instantiation = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref MethodSpecRow row)
		{
			row.Method = values[0];
			row.Instantiation = values[1];
		}

		protected override void FillValues(int[] values, ref MethodSpecRow row)
		{
			values[0] = row.Method;
			values[1] = row.Instantiation;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new MethodSpecRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new MethodSpecRow();
				row.Method = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[7]);
				row.Instantiation = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);

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

				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[7], (int)row.Method);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.Instantiation);
			}
		}
	}
}
