using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Common.Serialization
{
	public class StringSerializer : ObjectSerializer<string>
	{
		private Encoding _encoding = Encoding.UTF8;

		public StringSerializer()
			: base(0, StringComparer.Ordinal)
		{
		}

		public StringSerializer(int capacity)
			: base(capacity, StringComparer.Ordinal)
		{
		}

		public StringSerializer(IEqualityComparer<string> comparer)
			: base(0, comparer)
		{
		}

		public StringSerializer(int capacity, IEqualityComparer<string> comparer)
			: base(capacity, comparer)
		{
		}

		public StringSerializer(IBinaryAccessor accessor)
			: base(accessor, StringComparer.Ordinal)
		{
		}

		public StringSerializer(IBinaryAccessor accessor, IEqualityComparer<string> comparer)
			: base(accessor, comparer)
		{
		}

		public Encoding Encoding
		{
			get { return _encoding; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("Encoding");

				_encoding = value;
			}
		}

		protected override string Read(int pos)
		{
			return _blob.ReadLengthPrefixedString(ref pos, _encoding);
		}

		protected override void Write(string value)
		{
			int pos = _blob.Length;
			_blob.WriteLengthPrefixedString(ref pos, value, _encoding);
		}
	}
}
