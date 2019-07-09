using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace AssemblyDefender.UI
{
	public class TextImageToggleButton : ToggleButton
	{
		#region Fields

		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image",
			typeof(ImageSource),
			typeof(TextImageToggleButton),
			new PropertyMetadata(default(ImageSource)));

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text",
			typeof(string),
			typeof(TextImageToggleButton),
			new PropertyMetadata(default(ImageSource)));

		#endregion Fields

		#region Ctors

		static TextImageToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TextImageToggleButton),
				new FrameworkPropertyMetadata(typeof(TextImageToggleButton)));
		}

		#endregion Ctors

		#region Properties

		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		#endregion
	}
}
