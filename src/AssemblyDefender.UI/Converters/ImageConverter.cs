using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AssemblyDefender.UI
{
	public sealed class ImageConverter : IValueConverter
	{
		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ImageSource source;
			if (value is ImageSource)
			{
				source = (ImageSource)value;
			}
			else if (value is Uri)
			{
				source = new BitmapImage((Uri)value);
			}
			else if (value is string)
			{
				source = new BitmapImage(new Uri((string)value, UriKind.Relative));
			}
			else
			{
				return null;
			}

			var image = new Image();
			image.Source = source;
			image.Width = Width > 0 ? Width : source.Width;
			image.Height = Height > 0 ? Height : source.Height;

			return image;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}
}
