using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	public class ProjectAssemblyMerge
	{
		#region Fields

		private MergeDuplicateBehavior _duplicateBehavior;

		#endregion

		#region Ctors

		public ProjectAssemblyMerge()
		{
		}

		internal ProjectAssemblyMerge(IBinaryAccessor accessor, ProjectReadState state)
		{
			Read(accessor, state);
		}

		#endregion

		#region Properties

		public MergeDuplicateBehavior DuplicateBehavior
		{
			get { return _duplicateBehavior; }
			set { _duplicateBehavior = value; }
		}

		#endregion

		#region Methods

		internal void Read(IBinaryAccessor accessor, ProjectReadState state)
		{
			_duplicateBehavior = (MergeDuplicateBehavior)accessor.ReadByte();
		}

		internal void Write(IBinaryAccessor accessor, ProjectWriteState state)
		{
			accessor.Write((byte)_duplicateBehavior);
		}

		#endregion
	}
}
