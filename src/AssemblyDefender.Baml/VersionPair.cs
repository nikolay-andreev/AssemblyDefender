using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Baml
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct VersionPair
	{
		#region Fields

		private short _major;
		private short _minor;

		#endregion

		#region Ctors

		public VersionPair(short major, short minor)
		{
			if (major < 0)
				throw new ArgumentOutOfRangeException("major");

			if (minor < 0)
				throw new ArgumentOutOfRangeException("minor");

			_major = major;
			_minor = minor;
		}

		#endregion

		#region Properties

		public short Major
		{
			get { return _major; }
		}

		public short Minor
		{
			get { return _minor; }
		}

		#endregion

		#region Methods

		public override int GetHashCode()
		{
			return (_major << (0x10 + _minor));
		}

		public override bool Equals(object obj)
		{
			if (obj is VersionPair)
			{
				VersionPair pair = (VersionPair)obj;
				return pair._major == _major && pair._minor == _minor;
			}

			return false;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder("(");
			builder.Append(_major);
			builder.Append(",");
			builder.Append(_minor);
			builder.Append(")");

			return builder.ToString();
		}

		#endregion

		#region Static

		public static bool operator ==(VersionPair v1, VersionPair v2)
		{
			return v1._major == v2._major && v1._minor == v2._minor;
		}

		public static bool operator !=(VersionPair v1, VersionPair v2)
		{
			return !(v1 == v2);
		}

		public static bool operator >(VersionPair v1, VersionPair v2)
		{
			return v1._major > v2._major || v1._minor > v2._minor;
		}

		public static bool operator >=(VersionPair v1, VersionPair v2)
		{
			return v1._major >= v2._major || v1._minor >= v2._minor;
		}

		public static bool operator <(VersionPair v1, VersionPair v2)
		{
			return v1._major < v2._major || v1._minor < v2._minor;
		}

		public static bool operator <=(VersionPair v1, VersionPair v2)
		{
			return v1._major <= v2._major || v1._minor <= v2._minor;
		}

		#endregion
	}
}
