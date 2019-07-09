using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.UI.Model
{
	// TODO: remove
	public static class PropertyNames
	{
#pragma warning disable 0649

		public static readonly string Free;
		public static readonly string Offline;
		public static readonly string Online;

#pragma warning restore 0649

		static PropertyNames()
		{
			var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public;
			foreach (var field in typeof(PropertyNames).GetFields(flags))
			{
				field.SetValue(null, field.Name);
			}
		}
	}
}
