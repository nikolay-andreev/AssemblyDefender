using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.CommandLine
{
	public struct ArgumentUsage
	{
		private string _name;
		private string _description;

		public ArgumentUsage(string name, string description)
		{
			_name = name;
			_description = description;
		}

		public string Name
		{
			get { return _name; }
		}

		public string Description
		{
			get { return _description; }
		}
	}
}
