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
	public class AssemblyOSTable : MetadataTable<AssemblyOSRow>
	{
		internal AssemblyOSTable(MetadataTableStream stream)
			: base(MetadataTableType.AssemblyOS, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].OSPlatformId;

				case 1:
					return (int)_rows[index].OSMajorVersion;

				case 2:
					return (int)_rows[index].OSMinorVersion;

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
						values[i] = (int)_rows[j].OSPlatformId;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = (int)_rows[j].OSMajorVersion;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = (int)_rows[j].OSMinorVersion;
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
					_rows[index].OSPlatformId = (uint)value;
					break;

				case 1:
					_rows[index].OSMajorVersion = (uint)value;
					break;

				case 2:
					_rows[index].OSMinorVersion = (uint)value;
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
						_rows[j].OSPlatformId = (uint)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].OSMajorVersion = (uint)values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].OSMinorVersion = (uint)values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref AssemblyOSRow row)
		{
			row.OSPlatformId = (uint)values[0];
			row.OSMajorVersion = (uint)values[1];
			row.OSMinorVersion = (uint)values[2];
		}

		protected override void FillValues(int[] values, ref AssemblyOSRow row)
		{
			values[0] = (int)row.OSPlatformId;
			values[1] = (int)row.OSMajorVersion;
			values[2] = (int)row.OSMinorVersion;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new AssemblyOSRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new AssemblyOSRow();
				row.OSPlatformId = accessor.ReadUInt32();
				row.OSMajorVersion = accessor.ReadUInt32();
				row.OSMinorVersion = accessor.ReadUInt32();

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

				blob.Write(ref pos, (uint)row.OSPlatformId);
				blob.Write(ref pos, (uint)row.OSMajorVersion);
				blob.Write(ref pos, (uint)row.OSMinorVersion);
			}
		}
	}
}
