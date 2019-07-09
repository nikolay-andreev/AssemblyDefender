using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using Ctrl = System.Windows.Window;

namespace AssemblyDefender.UI
{
	public static class WindowExtensions
	{
		#region DialogResult

		public static readonly DependencyProperty DialogResultProperty =
			DependencyProperty.RegisterAttached(
				"DialogResult",
				typeof(bool?),
				typeof(WindowExtensions),
				new PropertyMetadata(OnDialogResultChanged));

		public static bool? GetDialogResult(Ctrl target)
		{
			return (bool?)target.GetValue(DialogResultProperty);
		}

		public static void SetDialogResult(Ctrl target, bool? value)
		{
			target.SetValue(DialogResultProperty, value);
		}

		private static void OnDialogResultChanged(
			DependencyObject d,
			DependencyPropertyChangedEventArgs e)
		{
			var window = d as Ctrl;
			if (window == null)
				return;

			window.DialogResult = e.NewValue as bool?;
		}

		#endregion

		#region RemoveIcon

		public static readonly DependencyProperty RemoveIconProperty =
			DependencyProperty.RegisterAttached(
				"RemoveIcon",
				typeof(bool),
				typeof(WindowExtensions),
				new PropertyMetadata(OnRemoveIconChanged));

		public static bool GetRemoveIcon(Ctrl target)
		{
			return (bool)target.GetValue(RemoveIconProperty);
		}

		public static void SetRemoveIcon(Ctrl target, bool value)
		{
			target.SetValue(RemoveIconProperty, value);
		}

		private static void OnRemoveIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var window = d as Ctrl;
			if (window == null)
				return;

			if (window.IsInitialized)
				return;

			if (!(e.NewValue is bool))
				return;

			bool value = (bool)e.NewValue;
			if (value)
			{
				window.SourceInitialized += OnRemoveIconSourceInitialized;
			}
		}

		private static void OnRemoveIconSourceInitialized(object sender, EventArgs e)
		{
			var window = sender as Ctrl;
			if (window == null)
				return;

			WindowsUtils.RemoveIcon(window);
			window.SourceInitialized -= OnRemoveIconSourceInitialized;
		}

		#endregion
	}
}
