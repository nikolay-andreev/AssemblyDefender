using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public delegate string[] GetDirectoriesCallback(string path, string pattern);

	public static class PathUtils
	{
		public static bool ContainsInvalidPathChars(string filePath)
		{
			return -1 != filePath.IndexOfAny(Path.GetInvalidPathChars());
		}

		public static bool ContainsInvalidFileNameChars(string filePath)
		{
			return -1 != filePath.IndexOfAny(Path.GetInvalidFileNameChars());
		}

		public static bool EndsWithDirectorySeparator(string path)
		{
			if (path.Length <= 0)
				return false;

			return IsDirectorySeparator(path[path.Length - 1]);
		}

		public static string TrimLeadingDirectorySeparator(string path)
		{
			int count = 0;
			int length = path.Length;
			while (length > count && IsDirectorySeparator(path[count]))
			{
				count++;
			}

			if (count > 0)
			{
				path = path.Substring(count);
			}

			return path;
		}

		public static string TrimTrailingDirectorySeparator(string path)
		{
			int count = 0;
			int index = path.Length - 1;
			while (index >= 0 && IsDirectorySeparator(path[index]))
			{
				count++;
				index--;
			}

			if (count > 0)
			{
				path = path.Substring(0, path.Length - count);
			}

			return path;
		}

		public static string AppendTrailingDirectorySeparatorIfMissing(string path)
		{
			if (!EndsWithDirectorySeparator(path))
			{
				path = path + Path.DirectorySeparatorChar;
			}

			return path;
		}

		public static string MakeAbsolutePath(string path)
		{
			return MakeAbsolutePath(path, Environment.CurrentDirectory);
		}

		public static string MakeAbsolutePath(string path, string basePath)
		{
			if (Path.IsPathRooted(path))
				return path;

			return Path.GetFullPath(Path.Combine(basePath, path));
		}

		public static string MakeRelativePath(string path, string basePath, bool localPath = false)
		{
			if (!Path.IsPathRooted(path))
				return path;

			var baseUri = new Uri(AppendTrailingDirectorySeparatorIfMissing(basePath));
			var uri = new Uri(path);

			var relativeUri = baseUri.MakeRelativeUri(uri);

			string relativePath = relativeUri.ToString();

			if (localPath)
			{
				relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
			}

			return relativePath;
		}

		public static bool HasExtension(string fileName, string[] allowedExtensions)
		{
			string extension = Path.GetExtension(fileName);
			foreach (string allowedExtension in allowedExtensions)
			{
				if (string.Compare(extension, allowedExtension, true, CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsDirectorySeparator(char c)
		{
			return
				c == Path.DirectorySeparatorChar ||
				c == Path.AltDirectorySeparatorChar;
		}
	}
}
