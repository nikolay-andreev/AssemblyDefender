using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Interface implementation descriptors.
	/// </summary>
	public class InterfaceImplTable : MetadataTable<InterfaceImplRow>
	{
		internal InterfaceImplTable(MetadataTableStream stream)
			: base(MetadataTableType.InterfaceImpl, stream)
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
					return _rows[index].Interface;

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
						values[i] = _rows[j].Interface;
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
					_rows[index].Interface = value;
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
						_rows[j].Interface = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref InterfaceImplRow row)
		{
			row.Class = values[0];
			row.Interface = values[1];
		}

		protected override void FillValues(int[] values, ref InterfaceImplRow row)
		{
			values[0] = row.Class;
			values[1] = row.Interface;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new InterfaceImplRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new InterfaceImplRow();
				row.Class = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.TypeDef]);
				row.Interface = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[0]);

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
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[0], (int)row.Interface);
			}
		}
	}
}
