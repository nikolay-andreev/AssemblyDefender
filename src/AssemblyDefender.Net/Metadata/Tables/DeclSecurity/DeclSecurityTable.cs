using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// Security attributes, which derive from System.Security.Permissions.SecurityAttribute can be attached to a
	/// TypeDef, a Method, or an Assembly.
	/// </summary>
	public class DeclSecurityTable : MetadataTable<DeclSecurityRow>
	{
		internal DeclSecurityTable(MetadataTableStream stream)
			: base(MetadataTableType.DeclSecurity, stream)
		{
		}

		public override int Get(int rid, int column)
		{
			int index = rid - 1;

			switch (column)
			{
				case 0:
					return (int)_rows[index].Action;

				case 1:
					return _rows[index].Parent;

				case 2:
					return _rows[index].PermissionSet;

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
						values[i] = (int)_rows[j].Action;
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].Parent;
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						values[i] = _rows[j].PermissionSet;
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
					_rows[index].Action = (SecurityAction)value;
					break;

				case 1:
					_rows[index].Parent = value;
					break;

				case 2:
					_rows[index].PermissionSet = value;
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
						_rows[j].Action = (SecurityAction)values[i];
					}
					break;

				case 1:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].Parent = values[i];
					}
					break;

				case 2:
					for (int i = 0, j = rid - 1; i < count; i++, j++)
					{
						_rows[j].PermissionSet = values[i];
					}
					break;

				default:
					throw new ArgumentOutOfRangeException("column");
			}
		}

		protected override void FillRow(int[] values, ref DeclSecurityRow row)
		{
			row.Action = (SecurityAction)values[0];
			row.Parent = values[1];
			row.PermissionSet = values[2];
		}

		protected override void FillValues(int[] values, ref DeclSecurityRow row)
		{
			values[0] = (int)row.Action;
			values[1] = row.Parent;
			values[2] = row.PermissionSet;
		}

		protected internal override void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count)
		{
			if (count == 0)
				return;

			var rows = new DeclSecurityRow[count];
			for (int i = 0; i < count; i++)
			{
				var row = new DeclSecurityRow();
				row.Action = (SecurityAction)accessor.ReadInt16();
				row.Parent = accessor.ReadCell(compressionInfo.CodedTokenDataSize4[4]);
				row.PermissionSet = accessor.ReadCell(compressionInfo.BlobHeapOffsetSize4);

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

				blob.Write(ref pos, (short)row.Action);
				blob.WriteCell(ref pos, compressionInfo.CodedTokenDataSize4[4], (int)row.Parent);
				blob.WriteCell(ref pos, compressionInfo.BlobHeapOffsetSize4, (int)row.PermissionSet);
			}
		}
	}
}
