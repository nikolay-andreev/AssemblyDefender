using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;

namespace AssemblyDefender.Common.Resources
{
	public class ResourceReaderEx : IDisposable
	{
		#region Fields

		private string _name;
		private byte[] _data;
		private string _typeName;
		private ResourceTypeCode _typeCode;
		private ResourceReader _reader;
		private IDictionaryEnumerator _enumerator;

		#endregion

		#region Ctors

		public ResourceReaderEx(string filePath)
			: this(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 0x1000, FileOptions.RandomAccess))
		{
		}

		public ResourceReaderEx(byte[] data)
			: this(new MemoryStream(data))
		{
		}

		public ResourceReaderEx(Stream stream)
		{
			_reader = new ResourceReader(stream);
			_enumerator = _reader.GetEnumerator();
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		public byte[] Data
		{
			get { return _data; }
		}

		public object Value
		{
			get { return _enumerator.Value; }
		}

		public string TypeName
		{
			get { return _typeName; }
		}

		public ResourceTypeCode TypeCode
		{
			get { return _typeCode; }
		}

		#endregion

		#region Methods

		public bool Read()
		{
			if (!_enumerator.MoveNext())
				return false;

			_name = _enumerator.Key.ToString();
			_reader.GetResourceData(_name, out _typeName, out _data);
			_typeCode = ResourceUtils.GetTypeCode(_typeName);

			return true;
		}

		public void Close()
		{
			if (_reader != null)
			{
				_reader.Close();
				_reader = null;
				_enumerator = null;
			}
		}

		void IDisposable.Dispose()
		{
			Close();
		}

		#endregion
	}
}
