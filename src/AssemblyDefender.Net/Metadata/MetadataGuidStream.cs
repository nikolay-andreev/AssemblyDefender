using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// #GUID: A GUID heap containing all sorts of globally unique identifiers.
	/// </summary>
	public class MetadataGuidStream
	{
		#region Fields

		private Blob _blob;

		#endregion

		#region Ctors

		public MetadataGuidStream()
		{
			_blob = new Blob();
		}

		public MetadataGuidStream(byte[] buffer)
		{
			_blob = new Blob(buffer);
		}

		public MetadataGuidStream(Blob blob)
		{
			_blob = blob;
		}

		#endregion

		#region Properties

		public int Length
		{
			get { return _blob.Length; }
		}

		internal Blob Blob
		{
			get { return _blob; }
			set { _blob = value; }
		}

		#endregion

		#region Methods

		public Guid Get(int position)
		{
			if (position == 0)
				return Guid.Empty;

			int startPos = position - 1;
			if (startPos < 0 || startPos + 16 > _blob.Length)
			{
				throw new ArgumentOutOfRangeException("position");
			}

			byte[] buffer = _blob.ReadBytes(ref startPos, 16);
			position += 16;

			Guid value = new Guid(buffer);

			return value;
		}

		public int Add(Guid value)
		{
			int position = _blob.Length;

			int addPos = position;
			_blob.Write(ref addPos, value.ToByteArray(), 0, 16);

			return position + 1;
		}

		public void Clear()
		{
			_blob = new Blob();
		}

		#endregion
	}
}
