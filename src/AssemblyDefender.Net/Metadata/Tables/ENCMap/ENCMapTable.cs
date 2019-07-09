using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Edit-and-continue mapping descriptors. This table does not exist in
	/// optimized metadata (#~ stream).
	/// </summary>
	public class ENCMapTable : MetadataTable<ENCMapRow>
	{
		internal ENCMapTable(MetadataTableStream stream)
			: base(MetadataTableType.ENCMap, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].Token;

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
						values[i] = (int)_rows[j].Token;
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
					_rows[index].Token = (uint)value;
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
						_rows[j].Token = (uint)values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ENCMapRow row)
		{
			row.Token = (uint)values[0];
		}

		protected override void FillValues(int[] values, ref ENCMapRow row)
		{
			values[0] = (int)row.Token;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ENCMapRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ENCMapRow();
				row.Token = accessor.ReadUInt32();

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

				blob.Write(ref pos, (uint)row.Token);
			}
		}
	}
}
