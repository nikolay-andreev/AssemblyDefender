using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssemblyDefender.Common;

namespace AssemblyDefender.Net
{
	public abstract class DotNetFramework
	{
		#region Fields

		protected Version _version;
		protected bool _isInstalled;
		protected string _installPath;
		protected AssemblyReference _mscorlibAssembly;
		protected AssemblyReference _systemAssembly;

		#endregion

		#region Ctors

		protected DotNetFramework()
		{
		}

		#endregion

		#region Properties

		public bool IsInstalled
		{
			get { return _isInstalled; }
		}

		public Version Version
		{
			get { return _version; }
		}

		public string InstallPath
		{
			get { return _installPath; }
		}

		public AssemblyReference MscorlibAssembly
		{
			get { return _mscorlibAssembly; }
		}

		public AssemblyReference SystemAssembly
		{
			get { return _systemAssembly; }
		}

		public abstract FrameworkType Type
		{
			get;
		}

		#endregion

		#region Static

		private static Dictionary<FrameworkKey, DotNetFramework> _frameworks = new Dictionary<FrameworkKey, DotNetFramework>();

		public static DotNetFramework GetByMscorlib(IAssemblySignature mscorlib)
		{
			return Get(GetFrameworkTypeByPublicKeyToken(mscorlib.PublicKeyToken), mscorlib.Version);
		}

		public static DotNetFramework Get(FrameworkType type, string versionMoniker)
		{
			Version version;
			switch (type)
			{
				case FrameworkType.MicrosoftNet:
					version = MicrosoftNetFramework.ParseVersion(versionMoniker);
					break;

				case FrameworkType.Silverlight:
				case FrameworkType.Mono:
				case FrameworkType.Moonlight:
				default:
					throw new NotImplementedException();
			}

			return Get(type, version);
		}

		public static DotNetFramework Get(FrameworkType type, Version version)
		{
			var key = new FrameworkKey(type, version);

			DotNetFramework framework;
			if (!_frameworks.TryGetValue(key, out framework))
			{
				framework = Create(type, version);
				_frameworks.Add(key, framework);
			}

			return framework;
		}

		private static DotNetFramework Create(FrameworkType type, Version version)
		{
			switch (type)
			{
				case FrameworkType.MicrosoftNet:
					return new MicrosoftNetFramework(version);

				case FrameworkType.Silverlight:
				case FrameworkType.Mono:
				case FrameworkType.Moonlight:
				default:
					throw new NotImplementedException();
			}
		}

		private static FrameworkType GetFrameworkTypeByPublicKeyToken(byte[] publicKeyToken)
		{
			if (CompareUtils.Equals(publicKeyToken, CodeModelUtils.EcmaPublicKeyToken, false))
			{
				return FrameworkType.MicrosoftNet;
			}
			else if (CompareUtils.Equals(publicKeyToken, SilverlightFramework.PublicKeyToken, false))
			{
				return FrameworkType.Silverlight;
			}
			else
			{
				return FrameworkType.MicrosoftNet;
			}
		}

		#endregion

		#region Nested types

		private struct FrameworkKey
		{
			private FrameworkType Type;
			private Version Version;

			internal FrameworkKey(FrameworkType type, Version version)
			{
				this.Type = type;
				this.Version = version;
			}

			public override int GetHashCode()
			{
				return (int)Type ^ Version.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var other = (FrameworkKey)obj;
				if (Type != other.Type)
					return false;

				if (Version.Equals(other.Version))
					return false;

				return true;
			}
		}

		#endregion
	}
}
