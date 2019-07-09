using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Stand-alone signature descriptors. Signatures per se are used in two
	/// capacities: as composite signatures of local variables of methods and as parameters of the
	/// call indirect (calli) IL instruction.
	/// </summary>
	public class StandAloneSigTable : MetadataTable<StandAloneSigRow>
	{
		internal StandAloneSigTable(MetadataTableStream stream)
			: base(MetadataTableType.StandAloneSig, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
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
						_rows[j].Signature = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref StandAloneSigRow row)
		{
			row.Signature = values[0];
		}

		protected override void FillValues(int[] values, ref StandAloneSigRow row)
		{
			values[0] = row.Signature;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new StandAloneSigRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new StandAloneSigRow();
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

				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.Signature);
			}
		}
	}
}
