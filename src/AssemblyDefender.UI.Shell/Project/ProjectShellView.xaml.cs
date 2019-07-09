using System;
using System.Collections.Generic;
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

namespace AssemblyDefender.UI.Shell.Project
{
	/// <summary>
	/// Interaction logic for ProjectShellView.xaml
	/// </summary>
	public partial class ProjectShellView : UserControl
	{
		private double _toolRowHeight = 250;

		public ProjectShellView()
		{
			InitializeComponent();
		}

		private void toolSplitter_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			bool isEnabled = (bool)e.NewValue;
			if (isEnabled)
			{
				toolRow.MinHeight = 50;
				toolRow.Height = new GridLength(_toolRowHeight);
			}
			else
			{
				_toolRowHeight = toolRow.ActualHeight;
				toolRow.MinHeight = 0;
				toolRow.Height = GridLength.Auto;
			}
		}
	}
}
