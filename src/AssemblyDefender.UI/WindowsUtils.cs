using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace AssemblyDefender.UI
{
	public static class WindowsUtils
	{
		internal const int GWL_EXSTYLE = -20;
		internal const int WS_EX_DLGMODALFRAME = 0x0001;
		internal const int SWP_NOSIZE = 0x0001;
		internal const int SWP_NOMOVE = 0x0002;
		internal const int SWP_NOZORDER = 0x0004;
		internal const int SWP_FRAMECHANGED = 0x0020;
		internal const uint WM_SETICON = 0x0080;

		[DllImport("user32.dll")]
		internal static extern int GetWindowLong(IntPtr hwnd, int index);

		[DllImport("user32.dll")]
		internal static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

		[DllImport("user32.dll")]
		internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
				   int x, int y, int width, int height, uint flags);

		[DllImport("user32.dll")]
		internal static extern IntPtr SendMessage(IntPtr hwnd, uint msg,
				   IntPtr wParam, IntPtr lParam);

		public static T FindVisualParent<T>(DependencyObject dependencyObject)
			where T : DependencyObject
		{
			return FindVisualParent(dependencyObject, o => o.GetType() == typeof(T)) as T;
		}

		/// <summary>
		/// Traverses the Visual Tree upwards looking for the ancestor that satisfies the <paramref name="predicate"/>.
		/// </summary>
		/// <param name="dependencyObject">The element for which the ancestor is being looked for.</param>
		/// <param name="predicate">The predicate that evaluates if an element is the ancestor that is being looked for.</param>
		/// <returns>
		/// The ancestor element that matches the <paramref name="predicate"/> or <see langword="null"/>
		/// if the ancestor was not found.
		/// </returns>
		public static DependencyObject FindVisualParent(DependencyObject dependencyObject, Func<DependencyObject, bool> predicate)
		{
			while (dependencyObject != null)
			{
				if (predicate(dependencyObject))
				{
					return dependencyObject;
				}

				dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
			}

			return null;
		}

		public static T FindVisualChild<T>(DependencyObject dependencyObject)
			where T : DependencyObject
		{
			return FindVisualChild(dependencyObject, o => o.GetType() == typeof(T)) as T;
		}

		public static DependencyObject FindVisualChild(DependencyObject dependencyObject, Func<DependencyObject, bool> predicate)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
			{
				var child = VisualTreeHelper.GetChild(dependencyObject, i);
				if (child == null)
					continue;

				if (predicate(child))
					return child;

				var descendent = FindVisualChild(child, predicate);
				if (descendent != null)
					return descendent;
			}

			return null;
		}

		public static T FindLogicalParent<T>(DependencyObject dependencyObject)
			where T : DependencyObject
		{
			return FindLogicalParent(dependencyObject, o => o.GetType() == typeof(T)) as T;
		}

		/// <summary>
		/// Traverses the Logical Tree upwards looking for the ancestor that satisfies the <paramref name="predicate"/>.
		/// </summary>
		/// <param name="dependencyObject">The element for which the ancestor is being looked for.</param>
		/// <param name="predicate">The predicate that evaluates if an element is the ancestor that is being looked for.</param>
		/// <returns>
		/// The ancestor element that matches the <paramref name="predicate"/> or <see langword="null"/>
		/// if the ancestor was not found.
		/// </returns>
		public static DependencyObject FindLogicalParent(DependencyObject dependencyObject, Func<DependencyObject, bool> predicate)
		{
			while (dependencyObject != null)
			{
				if (predicate(dependencyObject))
				{
					return dependencyObject;
				}

				dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
			}

			return null;
		}

		public static T FindLogicalChild<T>(DependencyObject dependencyObject)
					where T : DependencyObject
		{
			return FindLogicalChild(dependencyObject, o => o.GetType() == typeof(T)) as T;
		}

		public static DependencyObject FindLogicalChild(DependencyObject dependencyObject, Func<DependencyObject, bool> predicate)
		{
			foreach (var childObj in LogicalTreeHelper.GetChildren(dependencyObject))
			{
				var child = childObj as DependencyObject;
				if (child == null)
					continue;

				if (predicate(child))
					return child;

				var descendent = FindLogicalChild(child, predicate);
				if (descendent != null)
					return descendent;
			}

			return null;
		}

		public static void RemoveIcon(Window window)
		{
			// Get this window's handle
			IntPtr hwnd = new WindowInteropHelper(window).Handle;

			// Change the extended window style to not show a window icon
			int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

			SetWindowLong(
				hwnd,
				GWL_EXSTYLE,
				extendedStyle |
				WS_EX_DLGMODALFRAME);

			// Update the window's non-client area to reflect the changes
			SetWindowPos(
				hwnd,
				IntPtr.Zero,
				0, 0, 0, 0,
				SWP_NOMOVE |
				SWP_NOSIZE |
				SWP_NOZORDER |
				SWP_FRAMECHANGED);
		}

		public static void WaitForPriority()
		{
			WaitForPriority(DispatcherPriority.ApplicationIdle, TimeSpan.Zero);
		}

		public static void WaitForPriority(DispatcherPriority priority)
		{
			WaitForPriority(priority, TimeSpan.Zero);
		}

		public static void WaitForPriority(DispatcherPriority priority, TimeSpan timeout)
		{
			var dispatcher = Dispatcher.CurrentDispatcher;

			var action = (Action)delegate() { return; };

			if (timeout == TimeSpan.Zero)
				dispatcher.Invoke(priority, action);
			else
				dispatcher.Invoke(priority, timeout, action);
		}
	}
}
