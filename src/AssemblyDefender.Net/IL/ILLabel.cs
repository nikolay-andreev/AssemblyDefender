using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net.IL
{
	public class ILLabel : ILNode
	{
		#region Fields

		private ILInstruction _branch;

		#endregion

		#region Ctors

		public ILLabel()
		{
		}

		public ILLabel(ILInstruction branch)
		{
			_branch = branch;
		}

		#endregion

		#region Properties

		public ILInstruction Branch
		{
			get { return _branch; }
			set { _branch = value; }
		}

		public override ILNodeType NodeType
		{
			get { return ILNodeType.Label; }
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return string.Format("Label: {0}", _branch != null ? _branch.ToString() : "{NULL}");
		}

		#endregion
	}
}
