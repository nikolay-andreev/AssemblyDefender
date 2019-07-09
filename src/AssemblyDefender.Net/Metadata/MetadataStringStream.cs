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
	/// #Strings: A string heap containing the names of metadata items (class names, method
	/// names, field names, and so on). The stream does not contain literal constants defined or
	/// referenced in the methods of the module.
	/// </summary>
	public class MetadataStringStream
	{
		#region Fields

		private Blob _blob;

		#endregion

		#region Ctors

		public MetadataStringStream()
		{
			_blob = new Blob(new byte[] { 0 });
		}

		public MetadataStringStream(byte[] buffer)
		{
			_blob = new Blob(buffer);
		}

		public MetadataStringStream(Blob blob)
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

		public string Get(int position)
		{
			if (position < 0 || position >= _blob.Length)
				throw new ArgumentOutOfRangeException("position");

			if (position == 0)
				return null;

			string s = _blob.ReadNullTerminatedString(ref position, Encoding.UTF8);
			if (s.Length == 0)
				return string.Empty;

			return s;
		}

		public int Add(string value)
		{
			if (string.IsNullOrEmpty(value))
				return 0;

			int position = _blob.Length;

			int addPos = position;
			_blob.WriteNullTerminatedString(ref addPos, value, Encoding.UTF8);

			return position;
		}

		public void Clear()
		{
			_blob = new Blob(new byte[] { 0 });
		}

		#endregion
	}
}
