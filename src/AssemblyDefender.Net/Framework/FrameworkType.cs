using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Net
{
	public enum FrameworkType
	{
		MicrosoftNet,

		Silverlight,

		/// <summary>
		/// An open source, cross-platform, implementation of C# and the CLR that is binary compatible with Microsoft.NET.
		/// </summary>
		Mono,

		/// <summary>
		/// An open source implementation of Microsoft Silverlight for Linux and other Unix/X11 based operating systems
		/// </summary>
		Moonlight,
	}
}
