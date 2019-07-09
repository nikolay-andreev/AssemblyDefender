using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	public abstract class MetadataTable
	{
		#region Fields

		protected const int DefaultCapacity = 4;
		protected int _type;
		protected MetadataTableStream _stream;

		#endregion

		#region Ctors

		protected MetadataTable(int type, MetadataTableStream stream)
		{
			_type = type;
			_stream = stream;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the table type.
		/// </summary>
		public int Type
		{
			get { return _type; }
		}

		public abstract int Capacity
		{
			get;
			set;
		}

		/// <summary>
		/// Gets number of rows in the table.
		/// </summary>
		public abstract int Count
		{
			get;
		}

		public MetadataTableStream Stream
		{
			get { return _stream; }
		}

		#endregion

		#region Methods

		public bool Exists(int rid)
		{
			return rid > 0 && rid <= Count;
		}

		public abstract void Get(int rid, int[] values);

		public abstract int Get(int rid, int column);

		public abstract void Get(int rid, int column, int count, int[] values);

		public abstract int Add(int[] values);

		public abstract void Insert(int rid, int[] values);

		public abstract void Update(int rid, int[] values);

		public abstract void Update(int rid, int column, int value);

		public abstract void Update(int rid, int column, int count, int[] values);

		public abstract void Clear();

		public MetadataTableInfo GetSchema()
		{
			return MetadataSchema.Tables[_type];
		}

		protected internal virtual void Initialize()
		{
		}

		protected internal abstract void Read(IBinaryAccessor accessor, TableCompressionInfo compressionInfo, int count);

		protected internal abstract void Write(Blob blob, ref int pos, TableCompressionInfo compressionInfo);

		#endregion
	}
}
