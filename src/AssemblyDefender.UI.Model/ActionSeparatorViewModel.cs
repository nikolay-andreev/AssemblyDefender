using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace AssemblyDefender.UI.Model
{
	public class ActionSeparatorViewModel : ActionViewModel
	{
		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public override bool IsSeparator
		{
			get { return true; }
		}
	}
}
