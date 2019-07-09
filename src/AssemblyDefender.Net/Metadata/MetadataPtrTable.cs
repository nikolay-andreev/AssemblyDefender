using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public abstract class MetadataPtrTable : MetadataTable
	{
		#region Fields

		protected int _count;
		protected int[] _rows;

		#endregion

		#region Ctors

		internal MetadataPtrTable(int type, MetadataTableStream stream)
			: base(type, stream)
		{
			_rows = new int[DefaultCapacity];
		}

		#endregion

		#region Properties

		public override int Capacity
		{
			get { return _rows.Length; }
			set
			{
				if (value < 0)
				{
					throw new InvalidOperationException();
				}

				var rows = new int[value];
				if (_rows.Length > 0)
				{
					Array.Copy(_rows, 0, rows, 0, _count < value ? _count : value);
				}

				_rows = rows;
			}
		}

		public override int Count
		{
			get { return _count; }
		}

		#endregion

		#region Methods

		public int Get(int rid)
		{
			if (rid < 1 || rid > _count)
			{
				throw new ArgumentOutOfRangeException("rid");
			}

			return _rows[rid - 1];
		}

		public override void Get(int rid, int[] values)
		{
			if (rid < 1 || rid > _count)
			{
				throw new ArgumentOutOfRangeException("rid");
			}

			_rows[rid - 1] = values[0];
		}

		public override int Get(int rid, int column)
		{
			if (rid < 1 || rid > _count)
			{
				throw new ArgumentOutOfRangeException("rid");
			}

			if (column != 0)
			{
				throw new ArgumentOutOfRangeException("column");
			}

			return _rows[rid - 1];
		}

		public override void Get(int rid, int column, int count, int[] values)
		{
			for (int i = rid - 1; i < _count; i++)
			{
				values[i] = _rows[i];
			}
		}

		public override int Add(int[] values)
		{
			return Add(values[0]);
		}

		public int Add(int value)
		{
			if (_count == _rows.Length)
			{
				EnsureCapacity(_count + 1);
			}

			_rows[_count] = value;
			_count++;

			return _count;
		}

		public override void Insert(int rid, int[] values)
		{
			Insert(rid, values[0]);
		}

		public void Insert(int rid, int value)
		{
			if (_count == _rows.Length)
			{
				EnsureCapacity(_count + 1);
			}

			int index = rid - 1;
			if (index < _count)
			{
				Array.Copy(_rows, index, _rows, index + 1, _count - index);
			}

			_rows[index] = value;
			_count++;
		}

		public override void Update(int rid, int[] values)
		{
			if (rid < 1 || rid > _count)
			{
				throw new ArgumentOutOfRangeException("rid");
			}

			_rows[rid - 1] = values[0];
		}

		public override void Update(int rid, int column, int value)
		{
			if (rid < 1 || rid > _count)
			{
				throw new ArgumentOutOfRangeException("rid");
			}

			if (column != 0)
			{
				throw new ArgumentOutOfRangeException("column");
			}

			_rows[rid - 1] = value;
		}

		public override void Update(int rid, int column, int count, int[] values)
		{
			for (int i = rid - 1; i < _count; i++)
			{
				_rows[i] = values[i];
			}
		}

		public void Update(int rid, int value)
		{
			if (rid < 1 || rid > _count)
			{
				throw new ArgumentOutOfRangeException("rid");
			}

			_rows[rid - 1] = value;
		}

		public override void Clear()
		{
			_count = 0;
			_rows = new int[DefaultCapacity];
		}

		private void EnsureCapacity(int length)
		{
			int capacity = Capacity;
			if (length <= capacity)
				return;

			while (capacity < length)
			{
				capacity += 0x100;
			}

			Capacity = capacity;
		}

		#endregion
	}
}
