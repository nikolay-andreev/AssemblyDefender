using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Edit-and-continue log descriptors that hold information about what
	/// changes have been made to specific metadata items during in-memory editing. This
	/// table does not exist in optimized metadata (#~ stream).
	/// </summary>
	public class ENCLogTable : MetadataTable<ENCLogRow>
	{
		internal ENCLogTable(MetadataTableStream stream)
			: base(MetadataTableType.ENCLog, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].Token;

				case 1:
					return (int)_rows[index].FuncCode;

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

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = (int)_rows[j].FuncCode;
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

				case 1:
					_rows[index].FuncCode = (uint)value;
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

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].FuncCode = (uint)values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ENCLogRow row)
		{
			row.Token = (uint)values[0];
			row.FuncCode = (uint)values[1];
		}

		protected override void FillValues(int[] values, ref ENCLogRow row)
		{
			values[0] = (int)row.Token;
			values[1] = (int)row.FuncCode;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ENCLogRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ENCLogRow();
				row.Token = accessor.ReadUInt32();
				row.FuncCode = accessor.ReadUInt32();

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
				blob.Write(ref pos, (uint)row.FuncCode);
			}
		}
	}
}
