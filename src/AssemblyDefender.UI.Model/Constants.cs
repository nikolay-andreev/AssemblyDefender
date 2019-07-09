using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public static class Constants
	{
		public static readonly Version ProductVersion = typeof(AppService).Assembly.GetName().Version;
		public static readonly string ProjectBuilderConsoleName = "AssemblyDefenderBuilder.exe";
		public static readonly string ProjectFileFilter = "Project Files (*.adproj)|*.adproj|All Files|*.*";
		public static readonly string AssemblyFileFilter = "Assembly Files (*.exe;*.dll)|*.exe;*.dll|All Files|*.*";
		public static readonly string AddAssemblyFileFilter = "All Assembly Files (*.exe;*.dll;*.sln;*.csproj;*.vbproj)|*.exe;*.dll;*.sln;*.csproj;*.vbproj|All Files|*.*";
		public static readonly string StrongNameKeyFileFilter = "Key Files (*.snk;*.pfx)|*.snk;*.pfx|All Files|*.*";
		public static readonly string LocalMachineRegistryPath = @"Software\AssemblyDefender";
		public static readonly string CurrentUserRegistryPath = @"Software\AssemblyDefender";

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static readonly string WebSiteName = @"www.assemblydefender.com";

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static readonly string WebSite = @"http://www.assemblydefender.com/";

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public static readonly string WebSiteSupport = @"http://www.assemblydefender.com/support";
	}
}
