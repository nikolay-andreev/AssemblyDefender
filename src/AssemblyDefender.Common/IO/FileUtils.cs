using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace AssemblyDefender.Common.IO
{
    public static class FileUtils
    {
        public static void AppendAllBytes(string filePath, byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public static long GetFileLength(string filePath)
        {
            return (new FileInfo(filePath)).Length;
        }

        /// <summary>
        /// Accepts two strings that represents two files to compare.
        /// </summary>
        /// <param name="filePath1">The file1.</param>
        /// <param name="filePath2">The file2.</param>
        /// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
        public static bool CompareFiles(string filePath1, string filePath2)
        {
            return CompareFiles(filePath1, filePath2, 0x2000);
        }

        /// <summary>
        /// Accepts two strings that represents two files to compare.
        /// </summary>
        /// <param name="filePath1">The file1.</param>
        /// <param name="filePath2">The file2.</param>
        /// <param name="posOfFirstDiffByte">Position of the first diff byte or -1 if files are equal.</param>
        /// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
        public static bool CompareFiles(string filePath1, string filePath2, int bufferSize)
        {
            long posOfFirstDiffByte;
            return CompareFiles(filePath1, filePath2, bufferSize, out posOfFirstDiffByte);
        }

        /// <summary>
        /// Accepts two strings that represents two files to compare.
        /// </summary>
        /// <param name="filePath1">The file1.</param>
        /// <param name="filePath2">The file2.</param>
        /// <param name="bufferSize">The size of buffer.</param>
        /// <param name="posOfFirstDiffByte">Position of the first diff byte or -1 if files are equal.</param>
        /// <returns>Return true indicates that the contents of the files are the same. Return false if the files are different.</returns>
        public static bool CompareFiles(string filePath1, string filePath2, int bufferSize, out long posOfFirstDiffByte)
        {
            posOfFirstDiffByte = -1;

            // Determine if the same file was referenced two times.
            if (0 == string.Compare(filePath1, filePath2, true))
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // If some of the files does not exists then assume they are different.
            if (!(File.Exists(filePath1) && File.Exists(filePath2)))
            {
                return false;
            }

            using (var stream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var stream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return stream1.CompareTo(stream2, bufferSize, out posOfFirstDiffByte);
                }
            }
        }

        public static void MoveFile(string sourceFilePath, string destFilePath, bool overwrite)
        {
            if (overwrite)
            {
                if (File.Exists(destFilePath))
                {
                    File.Delete(destFilePath);
                }
            }

            File.Move(sourceFilePath, destFilePath);
        }

        #region Attributes

        /// <summary>
        /// Append attributes to file.
        /// </summary>
        /// <param name="filePath">The name of the file.</param>
        /// <param name="attributes">The attributes.</param>
        public static void AppendAttributes(string filePath, FileAttributes attributes)
        {
            var existingAttributes = File.GetAttributes(filePath);
            existingAttributes |= attributes;
            File.SetAttributes(filePath, existingAttributes);
        }

        /// <summary>
        /// Append file attributes in depth to all files in directory.
        /// </summary>
        /// <param name="dirPath">Directory to crawl for files.</param>
        /// <param name="attributes">File attributes.</param>
        public static void AppendAllAttributes(string dirPath, FileAttributes attributes)
        {
            AppendAllAttributes(dirPath, "*.*", attributes, true);
        }

        /// <summary>
        /// Append file attributes in depth using file pattern.
        /// </summary>
        /// <param name="dirPath">Directory to crawl for files.</param>
        /// <param name="pattern">Search pattern.</param>
        /// <param name="attributes">File attributes.</param>
        /// <param name="deep">True to recursively append attributes in all subdirs.</param>
        public static void AppendAllAttributes(string dirPath, string pattern, FileAttributes attributes, bool deep)
        {
            if (!Directory.Exists(dirPath))
            {
                throw new IOException(string.Format(SR.DirectoryNotFound, dirPath));
            }

            foreach (string filePath in Directory.GetFiles(dirPath, pattern))
            {
                AppendAttributes(filePath, attributes);
            }

            if (deep)
            {
                foreach (string subDirPath in Directory.GetDirectories(dirPath))
                {
                    AppendAllAttributes(subDirPath, pattern, attributes, deep);
                }
            }
        }

        /// <summary>
        /// Set file attributes in depth to all files in directory.
        /// </summary>
        /// <param name="dirPath">Directory to crawl for files.</param>
        /// <param name="attributes">File attributes.</param>
        public static void SetAllAttributes(string dirPath, FileAttributes attributes)
        {
            SetAllAttributes(dirPath, "*.*", attributes, true);
        }

        /// <summary>
        /// Set file attributes in depth using file pattern.
        /// </summary>
        /// <param name="dirPath">Directory to crawl for files.</param>
        /// <param name="pattern">Search pattern.</param>
        /// <param name="attributes">File attributes.</param>
        /// <param name="deep">True to recursively set attributes in all subdirs.</param>
        public static void SetAllAttributes(string dirPath, string pattern, FileAttributes attributes, bool deep)
        {
            if (!Directory.Exists(dirPath))
            {
                throw new Exception(string.Format(SR.DirectoryNotFound, dirPath));
            }

            foreach (string filePath in Directory.GetFiles(dirPath, pattern))
            {
                File.SetAttributes(filePath, attributes);
            }

            if (deep)
            {
                foreach (string subDirPath in Directory.GetDirectories(dirPath))
                {
                    SetAllAttributes(subDirPath, pattern, attributes, deep);
                }
            }
        }

        /// <summary>
        /// Removes the file attributes.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="attributes">The attributes.</param>
        public static void RemoveAttributes(string filePath, FileAttributes attributes)
        {
            var existingAttributes = File.GetAttributes(filePath);
            existingAttributes &= ~attributes;
            File.SetAttributes(filePath, existingAttributes);
        }

        /// <summary>
        /// Set file attributes in depth to all files in directory.
        /// </summary>
        /// <param name="dirPath">Directory to crawl for files.</param>
        /// <param name="attributes">File attributes.</param>
        public static void RemoveAllAttributes(string dirPath, FileAttributes attributes)
        {
            RemoveAllAttributes(dirPath, "*.*", attributes, true);
        }

        /// <summary>
        /// Set file attributes in depth using file pattern.
        /// </summary>
        /// <param name="dirPath">Directory to crawl for files.</param>
        /// <param name="pattern">Search pattern.</param>
        /// <param name="attributes">File attributes.</param>
        /// <param name="deep">True to recursively set attributes in all subdirs.</param>
        public static void RemoveAllAttributes(string dirPath, string pattern, FileAttributes attributes, bool deep)
        {
            if (!Directory.Exists(dirPath))
            {
                throw new Exception(string.Format(SR.DirectoryNotFound, dirPath));
            }

            foreach (string filePath in Directory.GetFiles(dirPath, pattern))
            {
                RemoveAttributes(filePath, attributes);
            }

            if (deep)
            {
                foreach (string subDirPath in Directory.GetDirectories(dirPath))
                {
                    RemoveAllAttributes(subDirPath, pattern, attributes, deep);
                }
            }
        }

        #endregion
    }
}
