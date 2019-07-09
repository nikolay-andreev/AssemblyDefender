using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AssemblyDefender.Net.Metadata;
using AssemblyDefender.PE;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.Net
{
	public class ExceptionHandler
	{
		#region Fields

		private int _tryOffset;
		private int _tryLength;
		private int _handlerOffset;
		private int _handlerLength;
		private int _filterOffset;
		private TypeSignature _catchType;
		private ExceptionHandlerType _type;

		#endregion

		#region Ctors

		public ExceptionHandler()
		{
		}

		public ExceptionHandler(
			ExceptionHandlerType type, int tryOffset, int tryLength,
			int handlerOffset, int handlerLength, int filterOffset)
		{
			_type = type;
			_tryOffset = tryOffset;
			_tryLength = tryLength;
			_handlerOffset = handlerOffset;
			_handlerLength = handlerLength;
			_filterOffset = filterOffset;
		}

		public ExceptionHandler(
			ExceptionHandlerType type, int tryOffset, int tryLength,
			int handlerOffset, int handlerLength, int filterOffset,
			TypeSignature catchType)
		{
			_type = type;
			_tryOffset = tryOffset;
			_tryLength = tryLength;
			_handlerOffset = handlerOffset;
			_handlerLength = handlerLength;
			_filterOffset = filterOffset;
			_catchType = catchType;
		}

		#endregion

		#region Properties

		public int TryOffset
		{
			get { return _tryOffset; }
			set { _tryOffset = value; }
		}

		public int TryLength
		{
			get { return _tryLength; }
			set { _tryLength = value; }
		}

		public int HandlerOffset
		{
			get { return _handlerOffset; }
			set { _handlerOffset = value; }
		}

		public int HandlerLength
		{
			get { return _handlerLength; }
			set { _handlerLength = value; }
		}

		public int FilterOffset
		{
			get { return _filterOffset; }
			set { _filterOffset = value; }
		}

		public TypeSignature CatchType
		{
			get { return _catchType; }
			set { _catchType = value; }
		}

		public ExceptionHandlerType Type
		{
			get { return _type; }
			set { _type = value; }
		}

		#endregion
	}
}
