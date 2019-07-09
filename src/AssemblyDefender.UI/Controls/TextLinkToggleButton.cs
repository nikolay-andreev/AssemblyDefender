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
	public class TextLinkToggleButton : ToggleButton
	{
		#region Fields

		/// <summary>
		/// Text Dependency Property.
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text",
			typeof(string),
			typeof(TextLinkToggleButton),
			new UIPropertyMetadata(default(string)));

		/// <summary>
		/// CheckedText Dependency Property.
		/// </summary>
		public static readonly DependencyProperty CheckedTextProperty = DependencyProperty.Register(
			"CheckedText",
			typeof(string),
			typeof(TextLinkToggleButton),
			new UIPropertyMetadata(default(string)));

		#endregion

		#region Ctors

		static TextLinkToggleButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TextLinkToggleButton),
				new FrameworkPropertyMetadata(typeof(TextLinkToggleButton)));
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the text to display when unchecked.
		/// </summary>
		[Browsable(true)]
		[Bindable(true)]
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		/// <summary>
		/// Gets or sets the text to display when checked.
		/// </summary>
		[Browsable(true)]
		[Bindable(true)]
		public string CheckedText
		{
			get { return (string)GetValue(CheckedTextProperty); }
			set { SetValue(CheckedTextProperty, value); }
		}

		#endregion
	}
}
