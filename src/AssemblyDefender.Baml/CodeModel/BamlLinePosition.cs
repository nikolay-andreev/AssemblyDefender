using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlLinePosition : BamlNode
	{
		private int _lineOffset;

		public BamlLinePosition()
		{
		}

		public BamlLinePosition(int lineOffset)
		{
			_lineOffset = lineOffset;
		}

		public int LineOffset
		{
			get { return _lineOffset; }
			set { _lineOffset = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.LinePosition; }
		}

		public override string ToString()
		{
			return string.Format("LinePosition: LineOffset={0}", _lineOffset);
		}
	}
}
