using System;
using System.Collections.Generic;
using System.Text;

namespace AssemblyDefender.PE
{
	/// <summary>
	/// Predefined resource types defined by the first level entry's ID.
	/// </summary>
	public enum ResourceType : int
	{
		CURSOR = 1,
		BITMAP = 2,
		ICON = 3,
		MENU = 4,
		DIALOG = 5,
		STRING = 6,
		FONTDIR = 7,
		FONT = 8,
		ACCELERATOR = 9,
		RCDATA = 10,
		MESSAGETABLE = 11,
		GROUP_CURSOR = CURSOR + 11,
		GROUP_ICON = ICON + 11,
		VERSION = 16,
		DLGINCLUDE = 17,
		PLUGPLAY = 19,
		VXD = 20,
		ANICURSOR = 21,
		ANIICON = 22,
		HTML = 23,
		MANIFEST = 24,
	}
}
