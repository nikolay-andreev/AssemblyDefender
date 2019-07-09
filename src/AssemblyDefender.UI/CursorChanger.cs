using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AssemblyDefender.UI
{
	public class CursorChanger : IDisposable
	{
		private Cursor _cursor;
		
		public CursorChanger(Cursor cursor)
		{
			_cursor = Mouse.OverrideCursor;
			Mouse.OverrideCursor = cursor;
		}

		public void Dispose()
		{
			Mouse.OverrideCursor = _cursor;
		}

		public static void ChangeAsync(Cursor cursor, DispatcherPriority priority)
		{
			var currentCursor = Mouse.OverrideCursor;

			Mouse.OverrideCursor = cursor;

			Dispatcher.CurrentDispatcher.BeginInvoke(
				priority,
				(DispatcherOperationCallback)delegate(object o)
				{
					Mouse.OverrideCursor = (Cursor)o;
					return null;
				},
				currentCursor);
		}
	}
}
