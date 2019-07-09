using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AssemblyDefender.UI
{
	public class TextImageLinkButton : Button
	{
		#region Fields

		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image",
			typeof(ImageSource),
			typeof(TextImageLinkButton),
			new PropertyMetadata(default(ImageSource)));

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text",
			typeof(string),
			typeof(TextImageLinkButton),
			new PropertyMetadata(default(ImageSource)));

		#endregion Fields

		#region Ctors

		static TextImageLinkButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TextImageLinkButton),
				new FrameworkPropertyMetadata(typeof(TextImageLinkButton)));
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
