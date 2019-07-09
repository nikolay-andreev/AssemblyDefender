using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace AssemblyDefender.Net
{
	internal class SilverlightFramework : DotNetFramework
	{
		internal SilverlightFramework(Version version)
		{
			Initialize(version);
		}

		public override FrameworkType Type
		{
			get { return FrameworkType.Silverlight; }
		}

		public override string ToString()
		{
			return "Silverlight,Version=" + _version.ToString();
		}

		private void Initialize(Version version)
		{
			_version = version;
			_mscorlibAssembly = new AssemblyReference("mscorlib", null, version, CodeModelUtils.EcmaPublicKeyToken);
			_systemAssembly = new AssemblyReference("System", null, version, CodeModelUtils.EcmaPublicKeyToken);

			if (_isFrameworkInstalled)
			{
				string versionString = GetCurrentVersionString();
				if (!string.IsNullOrEmpty(versionString))
				{
					string installPath = Path.Combine(_installRootPath, versionString);
					if (Directory.Exists(installPath))
					{
						_isInstalled = true;
						_installPath = installPath;
					}
				}
			}
		}

		private string GetCurrentVersionString()
		{
			string versionString = GetCurrentVersionString(RegistryView.Registry32);
			if (!string.IsNullOrEmpty(versionString))
				return versionString;

			if (RegistryView.Default != RegistryView.Registry32)
			{
				versionString = GetCurrentVersionString(RegistryView.Default);
				if (!string.IsNullOrEmpty(versionString))
					return versionString;
			}

			return null;
		}

		private string GetCurrentVersionString(RegistryView registryView)
		{
			using (var regKey =
				RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView)
				.OpenSubKey(@"Software\Microsoft\Silverlight", false))
			{
				if (regKey == null)
					return null;

				return regKey.GetValue("Version").ToString();
			}
		}

		#region Static

		public static readonly string VersionMoniker40 = "v2.0.50727";
		public static readonly string VersionMonikerLatest = VersionMoniker40;
		public static readonly byte[] PublicKeyToken = new byte[] { 0x7c, 0xec, 0x85, 0xd7, 0xbe, 0xa7, 0x79, 0x8e, };
		private static bool _isFrameworkInstalled;
		private static string _installRootPath;
		private static DotNetFramework _version40;

		public static bool IsFrameworkInstalled
		{
			get { return _isFrameworkInstalled; }
		}

		public static string InstallRootPath
		{
			get { return _installRootPath; }
		}

		public static DotNetFramework Version40
		{
			get
			{
				if (_version40 == null)
				{
					_version40 = Get(FrameworkType.Silverlight, VersionMoniker40);
				}

				return _version40;
			}
		}

		public static DotNetFramework VersionLatest
		{
			get { return Version40; }
		}

		static SilverlightFramework()
		{
			Initialize();
		}

		private static void Initialize()
		{
			string installRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Silverlight");
			if (!Directory.Exists(installRootPath))
			{
				installRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft Silverlight");
				if (!Directory.Exists(installRootPath))
					return;
			}

			_isFrameworkInstalled = true;
			_installRootPath = installRootPath;
		}

		#endregion
	}
}
