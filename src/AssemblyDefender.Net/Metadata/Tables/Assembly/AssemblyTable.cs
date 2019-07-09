using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// The Assembly table is defined using the .assembly directive; its columns are obtained from the
	/// respective .hash algorithm, .ver, .publickey, and .culture.
	/// </summary>
	public class AssemblyTable : MetadataTable<AssemblyRow>
	{
		internal AssemblyTable(MetadataTableStream stream)
			: base(MetadataTableType.Assembly, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].HashAlgId;

				case 1:
					return _rows[index].MajorVersion;

				case 2:
					return _rows[index].MinorVersion;

				case 3:
					return _rows[index].BuildNumber;

				case 4:
					return _rows[index].RevisionNumber;

				case 5:
					return _rows[index].Flags;

				case 6:
					return _rows[index].PublicKey;

				case 7:
					return _rows[index].Name;

				case 8:
					return _rows[index].Locale;

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
						values[i] = (int)_rows[j].HashAlgId;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].MajorVersion;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].MinorVersion;
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].BuildNumber;
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].RevisionNumber;
					}
					break;

				case 5:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Flags;
					}
					break;

				case 6:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].PublicKey;
					}
					break;

				case 7:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Name;
					}
					break;

				case 8:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Locale;
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
					_rows[index].HashAlgId = (HashAlgorithm)value;
					break;

				case 1:
					_rows[index].MajorVersion = value;
					break;

				case 2:
					_rows[index].MinorVersion = value;
					break;

				case 3:
					_rows[index].BuildNumber = value;
					break;

				case 4:
					_rows[index].RevisionNumber = value;
					break;

				case 5:
					_rows[index].Flags = value;
					break;

				case 6:
					_rows[index].PublicKey = value;
					break;

				case 7:
					_rows[index].Name = value;
					break;

				case 8:
					_rows[index].Locale = value;
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
						_rows[j].HashAlgId = (HashAlgorithm)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].MajorVersion = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].MinorVersion = values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].BuildNumber = values[i];
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].RevisionNumber = values[i];
					}
					break;

				case 5:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Flags = values[i];
					}
					break;

				case 6:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].PublicKey = values[i];
					}
					break;

				case 7:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				case 8:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Locale = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref AssemblyRow row)
		{
			row.HashAlgId = (HashAlgorithm)values[0];
			row.MajorVersion = values[1];
			row.MinorVersion = values[2];
			row.BuildNumber = values[3];
			row.RevisionNumber = values[4];
			row.Flags = values[5];
			row.PublicKey = values[6];
			row.Name = values[7];
			row.Locale = values[8];
		}

		protected override void FillValues(int[] values, ref AssemblyRow row)
		{
			values[0] = (int)row.HashAlgId;
			values[1] = row.MajorVersion;
			values[2] = row.MinorVersion;
			values[3] = row.BuildNumber;
			values[4] = row.RevisionNumber;
			values[5] = row.Flags;
			values[6] = row.PublicKey;
			values[7] = row.Name;
			values[8] = row.Locale;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new AssemblyRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new AssemblyRow();
				row.HashAlgId = (HashAlgorithm)accessor.ReadUInt32();
				row.MajorVersion = (int)accessor.ReadUInt16();
				row.MinorVersion = (int)accessor.ReadUInt16();
				row.BuildNumber = (int)accessor.ReadUInt16();
				row.RevisionNumber = (int)accessor.ReadUInt16();
				row.Flags = (int)accessor.ReadUInt32();
				row.PublicKey = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Locale = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);

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

				blob.Write(ref pos, (uint)row.HashAlgId);
				blob.Write(ref pos, (ushort)row.MajorVersion);
				blob.Write(ref pos, (ushort)row.MinorVersion);
				blob.Write(ref pos, (ushort)row.BuildNumber);
				blob.Write(ref pos, (ushort)row.RevisionNumber);
				blob.Write(ref pos, (uint)row.Flags);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.PublicKey);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Locale);
			}
		}
	}
}
