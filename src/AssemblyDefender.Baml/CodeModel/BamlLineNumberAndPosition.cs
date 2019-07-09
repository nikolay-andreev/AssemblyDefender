using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public class BamlLineNumberAndPosition : BamlNode
	{
		private int _lineNumber;
		private int _lineOffset;

		public BamlLineNumberAndPosition()
		{
		}

		public BamlLineNumberAndPosition(int lineNumber, int lineOffset)
		{
			_lineNumber = lineNumber;
			_lineOffset = lineOffset;
		}

		public int LineNumber
		{
			get { return _lineNumber; }
			set { _lineNumber = value; }
		}

		public int LineOffset
		{
			get { return _lineOffset; }
			set { _lineOffset = value; }
		}

		public override BamlNodeType NodeType
		{
			get { return BamlNodeType.LineNumberAndPosition; }
		}

		public override string ToString()
		{
			return string.Format("LineNumberAndPosition: LineNumber={0}; LineOffset={1}", _lineNumber, _lineOffset);
		}
	}
}
