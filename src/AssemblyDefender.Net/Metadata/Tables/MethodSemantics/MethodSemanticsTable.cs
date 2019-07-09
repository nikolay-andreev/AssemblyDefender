using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Method semantics descriptors that hold information about which
	/// method is associated with a specific property or event and in what capacity.
	/// </summary>
	public class MethodSemanticsTable : MetadataTable<MethodSemanticsRow>
	{
		internal MethodSemanticsTable(MetadataTableStream stream)
			: base(MetadataTableType.MethodSemantics, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].Semantic;

				case 1:
					return _rows[index].Method;

				case 2:
					return _rows[index].Association;

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
						values[i] = (int)_rows[j].Semantic;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Method;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Association;
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
					_rows[index].Semantic = (ushort)value;
					break;

				case 1:
					_rows[index].Method = value;
					break;

				case 2:
					_rows[index].Association = value;
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
						_rows[j].Semantic = (ushort)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Method = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Association = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref MethodSemanticsRow row)
		{
			row.Semantic = (ushort)values[0];
			row.Method = values[1];
			row.Association = values[2];
		}

		protected override void FillValues(int[] values, ref MethodSemanticsRow row)
		{
			values[0] = (int)row.Semantic;
			values[1] = row.Method;
			values[2] = row.Association;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new MethodSemanticsRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new MethodSemanticsRow();
				row.Semantic = accessor.ReadUInt16();
				row.Method = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.MethodDef]);
				row.Association = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[6]);

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

				blob.Write(ref pos, (ushort)row.Semantic);
				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.MethodDef], (int)row.Method);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[6], (int)row.Association);
			}
		}
	}
}
