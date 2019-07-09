using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Method definition descriptors.
	/// </summary>
	public class MethodTable : MetadataTable<MethodRow>
	{
		internal MethodTable(MetadataTableStream stream)
			: base(MetadataTableType.MethodDef, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].RVA;

				case 1:
					return (int)_rows[index].ImplFlags;

				case 2:
					return (int)_rows[index].Flags;

				case 3:
					return _rows[index].Name;

				case 4:
					return _rows[index].Signature;

				case 5:
					return _rows[index].ParamList;

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
						values[i] = (int)_rows[j].RVA;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = (int)_rows[j].ImplFlags;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = (int)_rows[j].Flags;
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Name;
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Signature;
					}
					break;

				case 5:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].ParamList;
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
					_rows[index].RVA = (uint)value;
					break;

				case 1:
					_rows[index].ImplFlags = (ushort)value;
					break;

				case 2:
					_rows[index].Flags = (ushort)value;
					break;

				case 3:
					_rows[index].Name = value;
					break;

				case 4:
					_rows[index].Signature = value;
					break;

				case 5:
					_rows[index].ParamList = value;
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
						_rows[j].RVA = (uint)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].ImplFlags = (ushort)values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Flags = (ushort)values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Signature = values[i];
					}
					break;

				case 5:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].ParamList = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref MethodRow row)
		{
			row.RVA = (uint)values[0];
			row.ImplFlags = (ushort)values[1];
			row.Flags = (ushort)values[2];
			row.Name = values[3];
			row.Signature = values[4];
			row.ParamList = values[5];
		}

		protected override void FillValues(int[] values, ref MethodRow row)
		{
			values[0] = (int)row.RVA;
			values[1] = (int)row.ImplFlags;
			values[2] = (int)row.Flags;
			values[3] = row.Name;
			values[4] = row.Signature;
			values[5] = row.ParamList;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new MethodRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new MethodRow();
				row.RVA = accessor.ReadUInt32();
				row.ImplFlags = accessor.ReadUInt16();
				row.Flags = accessor.ReadUInt16();
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Signature = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);
				row.ParamList = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.Param]);

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

				blob.Write(ref pos, (uint)row.RVA);
				blob.Write(ref pos, (ushort)row.ImplFlags);
				blob.Write(ref pos, (ushort)row.Flags);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.Signature);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.Param], (int)row.ParamList);
			}
		}
	}
}
