using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender.PE
{
	public class BuildBlob : Blob
	{
		#region Fields

		internal int Priority;
		private int _offsetAlignment;
		internal uint _rva;
		internal uint _pointerToRawData;
		internal BuildSection _section;

		#endregion

		#region Ctors

		public BuildBlob()
		{
		}

		public BuildBlob(byte[] buffer)
			: base(buffer)
		{
		}

		#endregion

		#region Properties

		public int OffsetAlignment
		{
			get { return _offsetAlignment; }
			set { _offsetAlignment = value; }
		}

		public uint RVA
		{
			get { return _rva; }
		}

		public uint PointerToRawData
		{
			get { return _pointerToRawData; }
		}

		public BuildSection Section
		{
			get { return _section; }
		}

		#endregion

		#region Methods

		internal void Attach(BuildSection section)
		{
			if (_section != null)
			{
				throw new BuildException(SR.BuildBlobAlreadyAttached);
			}

			_section = section;
		}

		#endregion
	}
}
