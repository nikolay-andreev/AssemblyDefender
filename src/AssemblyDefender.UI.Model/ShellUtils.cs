using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Build.BuildEngine;
using Build = Microsoft.Build.BuildEngine;

namespace AssemblyDefender.UI.Model
{
	internal static class ShellUtils
	{
		internal static bool IsProjectFile(string filePath)
		{
			string ext = Path.GetExtension(filePath).ToLower();

			return
				ext == ".sln" ||
				ext == ".csproj" ||
				ext == ".vbproj";
		}
	}
}
