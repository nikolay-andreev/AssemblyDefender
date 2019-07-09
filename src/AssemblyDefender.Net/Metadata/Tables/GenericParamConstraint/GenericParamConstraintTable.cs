using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Descriptors of constraints specified for type parameters of generic classes and methods.
	/// </summary>
	public class GenericParamConstraintTable : MetadataTable<GenericParamConstraintRow>
	{
		internal GenericParamConstraintTable(MetadataTableStream stream)
			: base(MetadataTableType.GenericParamConstraint, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return _rows[index].Owner;

				case 1:
					return _rows[index].Constraint;

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
						values[i] = _rows[j].Owner;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Constraint;
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
					_rows[index].Owner = value;
					break;

				case 1:
					_rows[index].Constraint = value;
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
						_rows[j].Owner = values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Constraint = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref GenericParamConstraintRow row)
		{
			row.Owner = values[0];
			row.Constraint = values[1];
		}

		protected override void FillValues(int[] values, ref GenericParamConstraintRow row)
		{
			values[0] = row.Owner;
			values[1] = row.Constraint;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new GenericParamConstraintRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new GenericParamConstraintRow();
				row.Owner = accessor.ReadCell(compressionInfo.TableRowIndexSize4[MetadataTableType.GenericParam]);
				row.Constraint = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[0]);

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

				blob.WriteCell(ref pos, compressionInfo.TableRowIndexSize4[MetadataTableType.GenericParam], (int)row.Owner);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[0], (int)row.Constraint);
			}
		}
	}
}
