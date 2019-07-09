using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.ComponentModel;

namespace AssemblyDefender.UI.Model
{
	public class AboutViewModel : DialogViewModel
	{
		#region Ctors

		internal AboutViewModel(ShellViewModel shell)
			: base(shell)
		{
		}

		#endregion

		#region Properties

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public ImageType LogoHeaderImage
		{
			get { return ImageType.LogoHeader; }
		}

		[System.Reflection.Obfuscation(Feature = "rename,remove", Exclude = true)]
		public Version Version
		{
			get { return Constants.ProductVersion; }
		}

		#endregion
	}
}
