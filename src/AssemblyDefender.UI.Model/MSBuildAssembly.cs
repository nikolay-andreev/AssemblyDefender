using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.UI.Model
{
	public class MSBuildAssembly
	{
		public string FilePath
		{
			get;
			set;
		}

		public bool Sign
		{
			get;
			set;
		}

		public bool DelaySign
		{
			get;
			set;
		}

		public string KeyFilePath
		{
			get;
			set;
		}

		public string KeyPassword
		{
			get;
			set;
		}
	}
}
