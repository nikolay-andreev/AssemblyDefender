using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
	internal static class ProjectHelper
	{
		internal static string MakeAbsolutePath(string path, string basePath)
		{
			if (!string.IsNullOrEmpty(path) && basePath != null)
			{
				path = PathUtils.MakeAbsolutePath(path, basePath);
			}

			return path;
		}

		internal static string MakeRelativePath(string path, string basePath)
		{
			if (!string.IsNullOrEmpty(path) && basePath != null)
			{
				path = PathUtils.MakeRelativePath(path, basePath, true);
			}

			return path;
		}
	}
}
