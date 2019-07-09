using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Ctrl = System.Windows.Controls.PasswordBox;

namespace AssemblyDefender.UI
{
	public static class PasswordBoxExtensions
	{
		public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached(
			"Password",
			typeof(string),
			typeof(PasswordBoxExtensions),
			new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		public static readonly DependencyProperty BindPasswordProperty = DependencyProperty.RegisterAttached(
			"BindPassword",
			typeof(bool),
			typeof(PasswordBoxExtensions),
			new PropertyMetadata(false, OnBindPasswordChanged));

		public static string GetPassword(DependencyObject dp)
		{
			return (string)dp.GetValue(PasswordProperty);
		}

		public static void SetPassword(DependencyObject dp, string value)
		{
			dp.SetValue(PasswordProperty, value);
		}

		public static bool GetBindPassword(DependencyObject dp)
		{
			return (bool)dp.GetValue(BindPasswordProperty);
		}

		public static void SetBindPassword(DependencyObject dp, bool value)
		{
			dp.SetValue(BindPasswordProperty, value);
		}

		private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
		{
			var box = dp as Ctrl;
			if (box == null)
				return;

			bool wasBound = (bool)(e.OldValue);
			bool needToBind = (bool)(e.NewValue);

			if (wasBound)
			{
				box.PasswordChanged -= OnPasswordChanged;
			}

			if (needToBind)
			{
				box.PasswordChanged += OnPasswordChanged;
			}
		}

		private static void OnPasswordChanged(object sender, RoutedEventArgs e)
		{
			var box = sender as Ctrl;
			if (box == null)
				return;

			SetPassword(box, box.Password);
		}
	}
}
