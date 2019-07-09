using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public class ILExceptionFilterBlock : ILBlock
	{
		#region Fields

		private ILTryBlock _try;
		private ILExceptionHandlerBlock _handler;

		#endregion

		#region Ctors

		public ILExceptionFilterBlock()
		{
		}

		#endregion

		#region Properties

		public ILTryBlock Try
		{
			get { return _try; }
			set { _try = value; }
		}

		public ILExceptionHandlerBlock Handler
		{
			get { return _handler; }
			set { _handler = value; }
		}

		public override ILBlockType BlockType
		{
			get { return ILBlockType.ExceptionFilter; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return "{Filter}";
		}

		#endregion
	}
}
