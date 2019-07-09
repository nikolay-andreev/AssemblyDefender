using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;

namespace AssemblyDefender.Common
{
	public static class CoreUtils
	{
		#region System.Version

		public static bool IsZero(this Version version)
		{
			return IsZero(version, 4);
		}

		public static bool IsZero(this Version version, int fieldCount)
		{
			switch (fieldCount)
			{
				case 1:
					return
						version.Major == 0;

				case 2:
					return
						version.Major == 0 &&
						version.Minor == 0;

				case 3:
					return
						version.Major == 0 &&
						version.Minor == 0 &&
						version.Build == 0;

				case 4:
					return
						version.Major == 0 &&
						version.Minor == 0 &&
						version.Build == 0 &&
						version.Revision == 0;

				default:
					throw new ArgumentException("fieldCount");
			}
		}

		#endregion

		#region System.IServiceProvider

		public static T GetService<T>(this IServiceProvider serviceProvider)
		{
			return (T)serviceProvider.GetService(typeof(T));
		}

		#endregion

		[DllImport("ole32.dll", EntryPoint = "CoCreateInstance")]
		public static extern int CoCreateInstance(
			[In] ref Guid rclsid,
			[In, MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
			[In] uint dwClsContext,
			[In] ref Guid riid,
			[Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

		public static T CoCreateInstance<T>(Guid clsid, Guid iid)
		{
			return CoCreateInstance<T>(clsid, iid, false);
		}

		public static T CoCreateInstance<T>(Guid clsid, Guid iid, bool throwOnFailure)
		{
			object obj;
			HRESULT.ThrowOnFailure(CoCreateInstance(ref clsid, null, 1, ref iid, out obj));
			if (!(obj is T))
			{
				if (throwOnFailure)
				{
					throw new InvalidComObjectException("Unable to create object.");
				}

				return default(T);
			}

			return (T)obj;
		}

		public static bool IsResource(byte[] data)
		{
			if (data.Length < 4)
				return false;

			int magicNumber = ((
				(data[0] |
				(data[1] << 8)) |
				(data[2] << 0x10)) |
				(data[3] << 0x18));

			return magicNumber == ResourceManager.MagicNumber;
		}

		#region Exception

		public static string Print(this Exception e, bool includeEnvironment = false, bool includeStackTrace = false)
		{
			var sb = new StringBuilder();

			if (includeEnvironment)
			{
				sb.AppendFormat("Operating system: {0}", Environment.OSVersion.VersionString).AppendLine();
				sb.AppendFormat("NET framework: {0}", Environment.Version.ToString()).AppendLine();
				sb.AppendFormat("64 bit OS: {0}", Environment.Is64BitOperatingSystem).AppendLine();
				sb.AppendFormat("64 bit process: {0}", Environment.Is64BitProcess).AppendLine();
				sb.AppendFormat("Command line: {0}", Environment.CommandLine).AppendLine();
			}

			if (e != null)
			{
				PrintException(sb, e);

				if (includeStackTrace && !string.IsNullOrEmpty(e.StackTrace))
				{
					sb.Append("--- Stack trace ----------------------------------------------------------").AppendLine();
					sb.Append(e.StackTrace).AppendLine();
				}
			}

			return sb.ToString();
		}

		private static void PrintException(StringBuilder sb, Exception e)
		{
			sb.Append("--- Exception ----------------------------------------------------------").AppendLine();
			sb.AppendFormat("Message: {0}", e.Message).AppendLine();
			sb.AppendFormat("Type: {0}", e.GetType().Name).AppendLine();

			if (!string.IsNullOrEmpty(e.Source))
				sb.AppendFormat("Source: {0}", e.Source).AppendLine();

			if (!string.IsNullOrEmpty(e.HelpLink))
				sb.AppendFormat("Help link: {0}", e.HelpLink).AppendLine();

			if (e.InnerException != null)
			{
				PrintException(sb, e.InnerException);
			}
		}

		#endregion
	}
}
