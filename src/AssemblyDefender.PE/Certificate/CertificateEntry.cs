using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class CertificateEntry : ICloneable
	{
		#region Fields

		private CertificateRevision _revision;
		private CertificateType _type;
		private byte[] _data;
		internal CertificateTable _parent;

		#endregion

		#region Ctors

		public CertificateEntry()
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the certificate version number.
		/// </summary>
		public CertificateRevision Revision
		{
			get { return _revision; }
			set { _revision = value; }
		}

		/// <summary>
		/// Gets the type of content in data blob.
		/// </summary>
		public CertificateType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		/// <summary>
		/// Gets the certificate blob, such as an Authenticode signature.
		/// </summary>
		public byte[] Data
		{
			get { return _data; }
			set { _data = value; }
		}

		public CertificateTable Parent
		{
			get { return _parent; }
		}

		#endregion

		#region Methods

		public CertificateEntry Clone()
		{
			var copy = new CertificateEntry();
			CopyTo(copy);

			return copy;
		}

		protected void CopyTo(CertificateEntry copy)
		{
			copy._revision = _revision;
			copy._type = _type;
			copy._data = _data;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region Static

		internal static CertificateEntry Load(IBinaryAccessor accessor)
		{
			var entry = new CertificateEntry();

			int length = accessor.ReadInt32();
			entry._revision = (CertificateRevision)accessor.ReadUInt16();
			entry._type = (CertificateType)accessor.ReadUInt16();

			// Subtract header length (length, revision, type).
			int dataLength = length - 8;
			entry._data = accessor.ReadBytes(dataLength);

			return entry;
		}

		#endregion
	}
}
