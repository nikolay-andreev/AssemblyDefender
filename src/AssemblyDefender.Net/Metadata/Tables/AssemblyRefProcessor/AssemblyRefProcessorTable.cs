using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// This table is unused.
	/// </summary>
	public class AssemblyRefProcessorTable : MetadataTable<AssemblyRefProcessorRow>
	{
		internal AssemblyRefProcessorTable(MetadataTableStream stream)
			: base(MetadataTableType.AssemblyRefProcessor, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].Processor;

				case 1:
					return _rows[index].AssemblyRef;

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
						values[i] = (int)_rows[j].Processor;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].AssemblyRef;
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
					_rows[index].Processor = (uint)value;
					break;

				case 1:
					_rows[index].AssemblyRef = value;
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
						_rows[j].Processor = (uint)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].AssemblyRef = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref AssemblyRefProcessorRow row)
		{
			row.Processor = (uint)values[0];
			row.AssemblyRef = values[1];
		}

		protected override void FillValues(int[] values, ref AssemblyRefProcessorRow row)
		{
			values[0] = (int)row.Processor;
			values[1] = row.AssemblyRef;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new AssemblyRefProcessorRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new AssemblyRefProcessorRow();
				row.Processor = accessor.ReadUInt32();
				row.AssemblyRef = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.AssemblyRef]);

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

				blob.Write(ref pos, (uint)row.Processor);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.AssemblyRef], (int)row.AssemblyRef);
			}
		}
	}
}
