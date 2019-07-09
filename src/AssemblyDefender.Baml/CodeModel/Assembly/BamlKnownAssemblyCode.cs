using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyDefender.Baml
{
	public enum BamlKnownAssemblyCode : short
	{
		MsCorLib, // typeof(double)
		System, // typeof(Uri)
		WindowsBase, // typeof(DependencyObject)
		PresentationCore, // typeof(UIElement)
		PresentationFramework, // typeof(FrameworkElement)
	}
}
