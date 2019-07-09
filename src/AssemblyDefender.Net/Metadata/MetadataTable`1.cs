using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.Metadata
{
	public abstract class MetadataTable<T> : MetadataTable
		where T : struct
	{
		#region Fields

		protected int _count;
		protected T[] _rows;

		#endregion

		#region Ctors

		internal MetadataTable(int type, MetadataTableStream stream)
			: base(type, stream)
		{
		}

		#endregion

		#region Properties

		public override int Capacity
		{
			get { return _rows != null ? _rows.Length : 0; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("value");

				if (value > 0)
				{
					var rows = new T[value];
					if (_rows != null)
					{
						Array.Copy(_rows, 0, rows, 0, _count < value ? _count : value);
					}

					_rows = rows;
				}
				else
				{
					_rows = null;
				}
			}
		}

		public override int Count
		{
			get { return _count; }
		}

		#endregion

		#region Methods

		public virtual void Get(int rid, out T row)
		{
			if (rid < 1 || rid > _count)
				throw new ArgumentOutOfRangeException("rid");

			row = _rows[rid - 1];
		}

		public override void Get(int rid, int[] values)
		{
			T row;
			Get(rid, out row);
			FillValues(values, ref row);
		}

		public virtual int Add(ref T row)
		{
			if (_rows == null || _count == _rows.Length)
			{
				EnsureCapacity(_count + 1);
			}

			_rows[_count] = row;
			_count++;

			return _count;
		}

		public override int Add(int[] values)
		{
			var row = new T();
			FillRow(values, ref row);
			return Add(ref row);
		}

		public virtual void Insert(int rid, ref T row)
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

			_rows[index] = row;
			_count++;
		}

		public override void Insert(int rid, int[] values)
		{
			var row = new T();
			FillRow(values, ref row);
			Insert(rid, ref row);
		}

		public virtual void Update(int rid, ref T row)
		{
			if (rid < 1 || rid > _count)
				throw new ArgumentOutOfRangeException("rid");

			_rows[rid - 1] = row;
		}

		public override void Update(int rid, int[] values)
		{
			var row = new T();
			FillRow(values, ref row);
			Update(rid, ref row);
		}

		public override void Clear()
		{
			_count = 0;
			_rows = null;
		}

		protected abstract void FillRow(int[] values, ref T row);

		protected abstract void FillValues(int[] values, ref T row);

		protected void EnsureCapacity(int length)
		{
			int capacity = Capacity;
			if (length <= capacity)
				return;

			if (capacity == 0)
				capacity = 0x100;

			while (capacity < length)
			{
				capacity *= 2;
			}

			Capacity = capacity;
		}

		internal T[] GetRows()
		{
			return _rows ?? new T[0];
		}

		internal void SetRows(T[] rows)
		{
			_rows = rows;
		}

		internal void Map(int[] rids)
		{
			var rows = new T[Capacity];

			for (int i = 0; i < _count; i++)
			{
				rows[i] = _rows[rids[i] - 1];
			}

			_rows = rows;
		}

		#endregion
	}
}
