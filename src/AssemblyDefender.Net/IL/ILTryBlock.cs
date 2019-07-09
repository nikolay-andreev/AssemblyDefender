using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	/// <remarks>
	/// The finally and fault handlers cannot peacefully coexist with other handlers, so if a
	/// guarded block has a finally or fault handler, it cannot have anything else.
	/// </remarks>
	public class ILTryBlock : ILBlock
	{
		#region Fields

		private List<ILExceptionHandlerBlock> _handlers = new List<ILExceptionHandlerBlock>();

		#endregion

		#region Ctors

		public ILTryBlock()
		{
		}

		#endregion

		#region Properties

		public List<ILExceptionHandlerBlock> Handlers
		{
			get { return _handlers; }
		}

		public override ILBlockType BlockType
		{
			get { return ILBlockType.Try; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return "{Try}";
		}

		#endregion
	}
}
