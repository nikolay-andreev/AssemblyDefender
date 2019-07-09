using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public abstract class MemberNode : CodeNode
	{
		#region Fields

		protected int _rid;

		#endregion

		#region Ctors

		protected MemberNode(Module module)
			: base(module)
		{
		}

		#endregion

		#region Properties

		public int RID
		{
			get { return _rid; }
		}

		public abstract MemberType MemberType
		{
			get;
		}

		#endregion

		#region Methods

		protected internal virtual void OnDeleted()
		{
		}

		#endregion
	}
}
