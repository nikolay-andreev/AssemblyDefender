using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net.Metadata
{
	/// <summary>
	/// External metadata stream.
	/// </summary>
	public class MetadataExternalStream
	{
		#region Fields

		private string _name;
		private Blob _blob;

		#endregion

		#region Ctors

		public MetadataExternalStream(string name)
			: this(name, new Blob(new byte[] { 0 }))
		{
		}

		public MetadataExternalStream(string name, Blob blob)
		{
			if (blob == null)
				throw new ArgumentNullException("blob");

			_name = name;
			_blob = blob;
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public Blob Blob
		{
			get { return _blob; }
		}

		#endregion
	}
}
