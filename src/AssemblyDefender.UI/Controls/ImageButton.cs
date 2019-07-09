using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;

namespace AssemblyDefender.UI
{
	public class ImageButton : Button
	{
		#region Fields

		public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(
			"ImageWidth",
			typeof(double),
			typeof(ImageButton),
			new PropertyMetadata(16.0));

		public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(
			"ImageHeight",
			typeof(double),
			typeof(ImageButton),
			new PropertyMetadata(16.0));

		public static readonly DependencyProperty NormalImageProperty = DependencyProperty.Register(
			"NormalImage",
			typeof(ImageSource),
			typeof(ImageButton),
			new PropertyMetadata(default(ImageSource)));

		public static readonly DependencyProperty HoverImageProperty = DependencyProperty.Register(
			"HoverImage",
			typeof(ImageSource),
			typeof(ImageButton),
			new PropertyMetadata(default(ImageSource)));

		public static readonly DependencyProperty PressedImageProperty = DependencyProperty.Register(
			"PressedImage",
			typeof(ImageSource),
			typeof(ImageButton),
			new PropertyMetadata(default(ImageSource)));

		public static readonly DependencyProperty DisabledImageProperty = DependencyProperty.Register(
			"DisabledImage",
			typeof(ImageSource),
			typeof(ImageButton),
			new PropertyMetadata(default(ImageSource)));

		#endregion Fields

		#region Ctors

		static ImageButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(ImageButton),
				new FrameworkPropertyMetadata(typeof(ImageButton)));
		}

		#endregion Ctors

		#region Properties

		public double ImageWidth
		{
			get { return (double)GetValue(ImageWidthProperty); }
			set { SetValue(ImageWidthProperty, value); }
		}

		public double ImageHeight
		{
			get { return (double)GetValue(ImageHeightProperty); }
			set { SetValue(ImageHeightProperty, value); }
		}

		public ImageSource NormalImage
		{
			get { return (ImageSource)GetValue(NormalImageProperty); }
			set { SetValue(NormalImageProperty, value); }
		}

		public ImageSource HoverImage
		{
			get { return (ImageSource)GetValue(HoverImageProperty); }
			set { SetValue(HoverImageProperty, value); }
		}

		public ImageSource PressedImage
		{
			get { return (ImageSource)GetValue(PressedImageProperty); }
			set { SetValue(PressedImageProperty, value); }
		}

		public ImageSource DisabledImage
		{
			get { return (ImageSource)GetValue(DisabledImageProperty); }
			set { SetValue(DisabledImageProperty, value); }
		}

		#endregion
	}
}
