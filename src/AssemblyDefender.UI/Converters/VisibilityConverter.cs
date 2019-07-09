using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AssemblyDefender.UI
{
	public class VisibilityConverter : IValueConverter
	{
		private Visibility _trueValue = Visibility.Visible;
		private Visibility _falseValue = Visibility.Collapsed;

		public Visibility TrueValue
		{
			get { return _trueValue; }
			set { _trueValue = value; }
		}

		public Visibility FalseValue
		{
			get { return _falseValue; }
			set { _falseValue = value; }
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
				return (bool)value ? _trueValue : _falseValue;

			if (value is int)
				return (int)value != 0 ? _trueValue : _falseValue;

			if (value is ICollection)
				return ((ICollection)value).Count > 0 ? _trueValue : _falseValue;

			return value != null ? _trueValue : _falseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == _trueValue ? true : false;
		}
	}
}
