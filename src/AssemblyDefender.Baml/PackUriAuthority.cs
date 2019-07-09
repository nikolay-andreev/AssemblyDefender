using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	/// <summary>
	/// The authority specifies the type of package that a part is contained by, while the path specifies the location of a part within a package.
	/// </summary>
	public enum PackUriAuthority
	{
		/// <summary>
		/// /ReferencedAssembly;component/Subfolder/ResourceFile.xaml
		/// </summary>
		None,

		/// <summary>
		/// pack://application:,,,/ReferencedAssembly;component/Subfolder/ResourceFile.xaml
		/// </summary>
		Application,

		/// <summary>
		/// pack://siteoforigin:,,,/SomeAssembly;component/ResourceFile.xaml
		/// </summary>
		SiteOfOrigin,
	}
}
