using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Common.IO
{
	public static class DirectoryUtils
	{
		public static void CreateDirectoryIfMissing(string dirPath)
		{
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		}

		public static void CreateOrEmptyDirectory(string dirPath)
		{
			if (Directory.Exists(dirPath))
			{
				EmptyDirectory(dirPath);
			}
			else
			{
				Directory.CreateDirectory(dirPath);
			}
		}

		public static void EmptyDirectory(string dirPath)
		{
			DeleteFiles(dirPath);
			DeleteSubDirectories(dirPath);
		}

		public static void CopyDirectory(string srcDirPath, string destDirPath, bool deep = false, bool overwrite = false)
		{
			CopyDirectory(srcDirPath, destDirPath, deep, overwrite, null);
		}

		public static void CopyDirectory(string srcDirPath, string destDirPath, bool deep, bool overwrite, Predicate<string> filter)
		{
			if (!Directory.Exists(srcDirPath))
			{
				throw new IOException(string.Format(SR.DirectoryNotFound, srcDirPath));
			}

			Directory.CreateDirectory(destDirPath);

			foreach (string srcFilePath in Directory.GetFiles(srcDirPath))
			{
				if (filter != null && !filter(srcFilePath))
					continue;

				string fileName = Path.GetFileName(srcFilePath);
				string destFilePath = Path.Combine(destDirPath, fileName);

				if (overwrite || !File.Exists(destFilePath))
				{
					File.Copy(srcFilePath, destFilePath, overwrite);
				}
			}

			if (deep)
			{
				foreach (string srcSubDirPath in Directory.GetDirectories(srcDirPath))
				{
					string dirName = Path.GetFileName(srcSubDirPath);
					string destSubDirPath = Path.Combine(destDirPath, dirName);
					CopyDirectory(srcSubDirPath, destSubDirPath, deep, overwrite, filter);
				}
			}
		}

		public static void DeleteFiles(string dirPath)
		{
			foreach (string filePath in Directory.GetFiles(dirPath))
			{
				File.Delete(filePath);
			}
		}

		public static void DeleteSubDirectories(string dirPath)
		{
			foreach (string subDirPath in Directory.GetDirectories(dirPath))
			{
				Directory.Delete(subDirPath, true);
			}
		}
	}
}
