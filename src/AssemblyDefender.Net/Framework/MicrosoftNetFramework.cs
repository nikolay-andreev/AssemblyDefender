using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace AssemblyDefender.Net
{
	public class MicrosoftNetFramework : DotNetFramework
	{
		internal MicrosoftNetFramework(Version version)
		{
			Initialize(version);
		}

		public override FrameworkType Type
		{
			get { return FrameworkType.MicrosoftNet; }
		}

		public override string ToString()
		{
			return ".NETFramework,Version=" + _version.ToString();
		}

		private void Initialize(Version version)
		{
			_version = version;
			_mscorlibAssembly = new AssemblyReference("mscorlib", null, version, CodeModelUtils.EcmaPublicKeyToken);
			_systemAssembly = new AssemblyReference("System", null, version, CodeModelUtils.EcmaPublicKeyToken);

			if (_isFrameworkInstalled)
			{
				string versionKey = version.Major + "." + version.Minor;
				foreach (string installPath in Directory.GetDirectories(_installRootPath))
				{
					if (installPath.Contains(versionKey))
					{
						_isInstalled = true;
						_installPath = installPath;
						break;
					}
				}
			}
		}

		#region Static

		public static readonly string VersionMoniker11 = "v1.1.4322";
		public static readonly string VersionMoniker20 = "v2.0.50727";
		public static readonly string VersionMoniker40 = "v4.0.30319";
		public static readonly string VersionMonikerLatest = VersionMoniker40;

		/// <summary>
		/// Public key used by non-ecma assemblies and implemented by all .NET implementations (Microsoft .NET, Mono).
		/// </summary>
		public static readonly byte[] BclPublicKeyToken = new byte[] { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35, };

		private static bool _isFrameworkInstalled;
		private static string _installRootPath;
		private static DotNetFramework _version20;
		private static DotNetFramework _version40;

		public static bool IsFrameworkInstalled
		{
			get { return _isFrameworkInstalled; }
		}

		public static string InstallRootPath
		{
			get { return _installRootPath; }
		}

		public static DotNetFramework Version20
		{
			get
			{
				if (_version20 == null)
				{
					_version20 = Get(FrameworkType.MicrosoftNet, new Version(2, 0, 0, 0));
				}

				return _version20;
			}
		}

		public static DotNetFramework Version40
		{
			get
			{
				if (_version40 == null)
				{
					_version40 = Get(FrameworkType.MicrosoftNet, new Version(4, 0, 0, 0));
				}

				return _version40;
			}
		}

		public static DotNetFramework VersionLatest
		{
			get { return Version40; }
		}

		static MicrosoftNetFramework()
		{
			Initialize();
		}

		public static Version ParseVersion(string moniker)
		{
			if (moniker.Contains("2.0") || moniker.Contains("3.5") || moniker.Contains("3.0"))
			{
				return new Version(2, 0, 0, 0);
			}
			else // 4.0
			{
				return new Version(4, 0, 0, 0);
			}
		}

		private static void Initialize()
		{
			using (var regKey =
				RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
				.OpenSubKey(@"Software\Microsoft\.NetFramework", false))
			{
				if (regKey != null)
				{
					string installRootPath = regKey.GetValue("InstallRoot").ToString();
					if (Directory.Exists(installRootPath))
					{
						_isFrameworkInstalled = true;
						_installRootPath = installRootPath;
					}
				}
			}
		}

		#endregion
	}
}
