using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Managed resource descriptors.
	/// </summary>
	public class ManifestResourceTable : MetadataTable<ManifestResourceRow>
	{
		internal ManifestResourceTable(MetadataTableStream stream)
			: base(MetadataTableType.ManifestResource, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Offset;

				case 1:
					return _rows[index].Flags;

				case 2:
					return _rows[index].Name;

				case 3:
					return _rows[index].Implementation;

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
						values[i] = _rows[j].Offset;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Flags;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Name;
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Implementation;
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
					_rows[index].Offset = value;
					break;

				case 1:
					_rows[index].Flags = value;
					break;

				case 2:
					_rows[index].Name = value;
					break;

				case 3:
					_rows[index].Implementation = value;
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
						_rows[j].Offset = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Flags = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Implementation = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref ManifestResourceRow row)
		{
			row.Offset = values[0];
			row.Flags = values[1];
			row.Name = values[2];
			row.Implementation = values[3];
		}

		protected override void FillValues(int[] values, ref ManifestResourceRow row)
		{
			values[0] = row.Offset;
			values[1] = row.Flags;
			values[2] = row.Name;
			values[3] = row.Implementation;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new ManifestResourceRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new ManifestResourceRow();
				row.Offset = accessor.ReadInt32();
				row.Flags = (int)accessor.ReadUInt32();
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Implementation = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[9]);

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

				blob.Write(ref pos, (int)row.Offset);
				blob.Write(ref pos, (uint)row.Flags);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[9], (int)row.Implementation);
			}
		}
	}
}
