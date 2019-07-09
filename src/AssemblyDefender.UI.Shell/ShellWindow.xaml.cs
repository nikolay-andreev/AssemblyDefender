using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AssemblyDefender.UI.Model;
using Microsoft.Win32;
using AssemblyDefender.UI;

namespace AssemblyDefender.UI.Shell
{
	/// <summary>
	/// Interaction logic for ShellWindow.xaml
	/// </summary>
	public partial class ShellWindow : MvvmWindow
	{
		public ShellWindow()
		{
			InitializeComponent();
		}
	}
}
