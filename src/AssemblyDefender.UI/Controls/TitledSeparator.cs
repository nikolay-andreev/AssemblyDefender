using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AssemblyDefender.UI
{
	public class TitledSeparator : Control
	{
		#region Fields

		/// <summary>
		/// Title Dependency Property.
		/// </summary>
		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title",
			typeof(string),
			typeof(TitledSeparator),
			new PropertyMetadata(default(string)));

		/// <summary>
		/// Image Dependency Property.
		/// </summary>
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
			"Image",
			typeof(ImageSource),
			typeof(TitledSeparator),
			new PropertyMetadata(default(ImageSource)));

		#endregion

		#region Ctors

		static TitledSeparator()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TitledSeparator),
				new FrameworkPropertyMetadata(typeof(TitledSeparator)));
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the title to display.
		/// </summary>
		[Browsable(true)]
		[Bindable(true)]
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <summary>
		/// Gets or sets the title to display.
		/// </summary>
		[Browsable(true)]
		[Bindable(true)]
		public ImageSource Image
		{
			get { return (ImageSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		[Bindable(true)]
		public Visibility ImageVisibility
		{
			get { return GetValue(ImageProperty) != null ? Visibility.Visible : Visibility.Collapsed; }
		}

		#endregion
	}
}
