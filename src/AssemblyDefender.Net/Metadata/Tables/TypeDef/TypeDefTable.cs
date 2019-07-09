using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Class or interface definition descriptors.
	/// </summary>
	public class TypeDefTable : MetadataTable<TypeDefRow>
	{
		internal TypeDefTable(MetadataTableStream stream)
			: base(MetadataTableType.TypeDef, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Flags;

				case 1:
					return _rows[index].Name;

				case 2:
					return _rows[index].Namespace;

				case 3:
					return _rows[index].Extends;

				case 4:
					return _rows[index].FieldList;

				case 5:
					return _rows[index].MethodList;

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
						values[i] = _rows[j].Flags;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Name;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Namespace;
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Extends;
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].FieldList;
					}
					break;

				case 5:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].MethodList;
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
					_rows[index].Flags = value;
					break;

				case 1:
					_rows[index].Name = value;
					break;

				case 2:
					_rows[index].Namespace = value;
					break;

				case 3:
					_rows[index].Extends = value;
					break;

				case 4:
					_rows[index].FieldList = value;
					break;

				case 5:
					_rows[index].MethodList = value;
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
						_rows[j].Flags = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Name = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Namespace = values[i];
					}
					break;

				case 3:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Extends = values[i];
					}
					break;

				case 4:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].FieldList = values[i];
					}
					break;

				case 5:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].MethodList = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref TypeDefRow row)
		{
			row.Flags = values[0];
			row.Name = values[1];
			row.Namespace = values[2];
			row.Extends = values[3];
			row.FieldList = values[4];
			row.MethodList = values[5];
		}

		protected override void FillValues(int[] values, ref TypeDefRow row)
		{
			values[0] = row.Flags;
			values[1] = row.Name;
			values[2] = row.Namespace;
			values[3] = row.Extends;
			values[4] = row.FieldList;
			values[5] = row.MethodList;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new TypeDefRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new TypeDefRow();
				row.Flags = (int)accessor.ReadUInt32();
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Namespace = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Extends = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[0]);
				row.FieldList = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.Field]);
				row.MethodList = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.MethodDef]);

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

				blob.Write(ref pos, (uint)row.Flags);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Namespace);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[0], (int)row.Extends);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.Field], (int)row.FieldList);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.MethodDef], (int)row.MethodList);
			}
		}
	}
}
