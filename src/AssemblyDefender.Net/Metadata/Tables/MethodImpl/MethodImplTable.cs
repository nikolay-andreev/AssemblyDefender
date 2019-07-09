using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Method implementation descriptors.
	/// </summary>
	public class MethodImplTable : MetadataTable<MethodImplRow>
	{
		internal MethodImplTable(MetadataTableStream stream)
			: base(MetadataTableType.MethodImpl, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Class;

				case 1:
					return _rows[index].MethodBody;

				case 2:
					return _rows[index].MethodDeclaration;

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
						values[i] = _rows[j].Class;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].MethodBody;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].MethodDeclaration;
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
					_rows[index].Class = value;
					break;

				case 1:
					_rows[index].MethodBody = value;
					break;

				case 2:
					_rows[index].MethodDeclaration = value;
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
						_rows[j].Class = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].MethodBody = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].MethodDeclaration = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref MethodImplRow row)
		{
			row.Class = values[0];
			row.MethodBody = values[1];
			row.MethodDeclaration = values[2];
		}

		protected override void FillValues(int[] values, ref MethodImplRow row)
		{
			values[0] = row.Class;
			values[1] = row.MethodBody;
			values[2] = row.MethodDeclaration;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new MethodImplRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new MethodImplRow();
				row.Class = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef]);
				row.MethodBody = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[7]);
				row.MethodDeclaration = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[7]);

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

				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef], (int)row.Class);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[7], (int)row.MethodBody);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[7], (int)row.MethodDeclaration);
			}
		}
	}
}
