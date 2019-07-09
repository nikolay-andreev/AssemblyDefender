using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The table is defined by the .assembly extern directiv. Its columns are filled using directives
	/// similar to  those of the Assembly table except for the PublicKeyOrToken column, which is defined
	/// using the .publickeytoken directive.
	/// </summary>
	public class AssemblyRefTable : MetadataTable<AssemblyRefRow>
	{
		internal AssemblyRefTable(MetadataTableStream stream)
			: base(MetadataTableType.AssemblyRef, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].MajorVersion;

				case 1:
					return _rows[index].MinorVersion;

				case 2:
					return _rows[index].BuildNumber;

				case 3:
					return _rows[index].RevisionNumber;

				case 4:
					return _rows[index].Flags;

				case 5:
					return _rows[index].PublicKeyOrToken;

				case 6:
					return _rows[index].Name;

				case 7:
					return _rows[index].Locale;

				case 8:
					return _rows[index].HashValue;

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
						values[i] = _rows[j].MajorVersion;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].MinorVersion;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].BuildNumber;
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].RevisionNumber;
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Flags;
					}
					break;

				case 5:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].PublicKeyOrToken;
					}
					break;

				case 6:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Name;
					}
					break;

				case 7:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Locale;
					}
					break;

				case 8:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].HashValue;
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
					_rows[index].MajorVersion = value;
					break;

				case 1:
					_rows[index].MinorVersion = value;
					break;

				case 2:
					_rows[index].BuildNumber = value;
					break;

				case 3:
					_rows[index].RevisionNumber = value;
					break;

				case 4:
					_rows[index].Flags = value;
					break;

				case 5:
					_rows[index].PublicKeyOrToken = value;
					break;

				case 6:
					_rows[index].Name = value;
					break;

				case 7:
					_rows[index].Locale = value;
					break;

				case 8:
					_rows[index].HashValue = value;
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
						_rows[j].MajorVersion = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].MinorVersion = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].BuildNumber = values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].RevisionNumber = values[i];
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Flags = values[i];
					}
					break;

				case 5:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].PublicKeyOrToken = values[i];
					}
					break;

				case 6:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				case 7:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Locale = values[i];
					}
					break;

				case 8:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].HashValue = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref AssemblyRefRow row)
		{
			row.MajorVersion = values[0];
			row.MinorVersion = values[1];
			row.BuildNumber = values[2];
			row.RevisionNumber = values[3];
			row.Flags = values[4];
			row.PublicKeyOrToken = values[5];
			row.Name = values[6];
			row.Locale = values[7];
			row.HashValue = values[8];
		}

		protected override void FillValues(int[] values, ref AssemblyRefRow row)
		{
			values[0] = row.MajorVersion;
			values[1] = row.MinorVersion;
			values[2] = row.BuildNumber;
			values[3] = row.RevisionNumber;
			values[4] = row.Flags;
			values[5] = row.PublicKeyOrToken;
			values[6] = row.Name;
			values[7] = row.Locale;
			values[8] = row.HashValue;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new AssemblyRefRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new AssemblyRefRow();
				row.MajorVersion = (int)accessor.ReadUInt16();
				row.MinorVersion = (int)accessor.ReadUInt16();
				row.BuildNumber = (int)accessor.ReadUInt16();
				row.RevisionNumber = (int)accessor.ReadUInt16();
				row.Flags = (int)accessor.ReadUInt32();
				row.PublicKeyOrToken = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Locale = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.HashValue = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);

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

				blob.Write(ref pos, (ushort)row.MajorVersion);
				blob.Write(ref pos, (ushort)row.MinorVersion);
				blob.Write(ref pos, (ushort)row.BuildNumber);
				blob.Write(ref pos, (ushort)row.RevisionNumber);
				blob.Write(ref pos, (uint)row.Flags);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.PublicKeyOrToken);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Locale);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.HashValue);
			}
		}
	}
}
