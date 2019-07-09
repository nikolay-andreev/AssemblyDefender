using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public class ILExceptionHandlerBlock : ILBlock
	{
		#region Fields

		private ILTryBlock _try;
		private ILExceptionFilterBlock _filter;
		private TypeSignature _catchType;
		private ExceptionHandlerType _handlerType;

		#endregion

		#region Ctors

		public ILExceptionHandlerBlock()
		{
		}

		#endregion

		#region Properties

		public ILTryBlock Try
		{
			get { return _try; }
			set { _try = value; }
		}

		public ILExceptionFilterBlock Filter
		{
			get { return _filter; }
			set { _filter = value; }
		}

		public TypeSignature CatchType
		{
			get { return _catchType; }
			set { _catchType = value; }
		}

		public ExceptionHandlerType HandlerType
		{
			get { return _handlerType; }
			set { _handlerType = value; }
		}

		public override ILBlockType BlockType
		{
			get { return ILBlockType.ExceptionHandle; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return string.Format("{{{0}}} {1}", _handlerType.ToString(), _catchType != null ? _catchType.ToString() : "");
		}

		#endregion
	}
}
