using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Class reference descriptors.
	/// </summary>
	public class TypeRefTable : MetadataTable<TypeRefRow>
	{
		internal TypeRefTable(MetadataTableStream stream)
			: base(MetadataTableType.TypeRef, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].ResolutionScope;

				case 1:
					return _rows[index].Name;

				case 2:
					return _rows[index].Namespace;

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
						values[i] = _rows[j].ResolutionScope;
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
					_rows[index].ResolutionScope = value;
					break;

				case 1:
					_rows[index].Name = value;
					break;

				case 2:
					_rows[index].Namespace = value;
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
						_rows[j].ResolutionScope = values[i];
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

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref TypeRefRow row)
		{
			row.ResolutionScope = values[0];
			row.Name = values[1];
			row.Namespace = values[2];
		}

		protected override void FillValues(int[] values, ref TypeRefRow row)
		{
			values[0] = row.ResolutionScope;
			values[1] = row.Name;
			values[2] = row.Namespace;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new TypeRefRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new TypeRefRow();
				row.ResolutionScope = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[11]);
				row.Name = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);
				row.Namespace = accessor.ReadCell(compressionInfo.StringHeapOffsetSize4);

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

				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[11], (int)row.ResolutionScope);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Name);
				blob.WriteCell(ref pos, compressionInfo.StringHeapOffsetSize4, (int)row.Namespace);
			}
		}
	}
}
