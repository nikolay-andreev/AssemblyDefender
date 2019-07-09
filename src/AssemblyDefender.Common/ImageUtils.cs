using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace AssemblyDefender.Common
{
	public static class ImageUtils
	{
		public static Bitmap CreateTransparent(this Bitmap image)
		{
			return CreateTransparent(image, Color.Transparent);
		}

		public static Bitmap CreateTransparent(this Bitmap image, Color transparentColor)
		{
			var colorToMakeTransparent = Color.LightGray;

			if (image.Height > 0 && image.Width > 0)
			{
				colorToMakeTransparent = image.GetPixel(0, image.Size.Height - 1);
			}

			if (colorToMakeTransparent.A < 0xff)
				return null;

			return CreateTransparent(image, transparentColor, colorToMakeTransparent);
		}

		public static Bitmap CreateTransparent(this Bitmap image, Color transparentColor, Color colorToMakeTransparent)
		{
			if (image.RawFormat.Guid == ImageFormat.Icon.Guid)
			{
				throw new InvalidOperationException("Can't make icon transparent");
			}

			var size = image.Size;
			var newImage = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
			using (var graphics = Graphics.FromImage(newImage))
			{
				graphics.Clear(transparentColor);
				var destRect = new Rectangle(0, 0, size.Width, size.Height);
				using (var attributes = new ImageAttributes())
				{
					attributes.SetColorKey(colorToMakeTransparent, colorToMakeTransparent);
					graphics.DrawImage(image, destRect, 0, 0, size.Width, size.Height, GraphicsUnit.Pixel, attributes, null, IntPtr.Zero);
				}
			}

			return newImage;
		}

		public static Bitmap CreateImageList(IList imageList, Size imageSize)
		{
			var newImage = new Bitmap(imageSize.Width * imageList.Count, imageSize.Height, PixelFormat.Format32bppArgb);
			using (var graphics = Graphics.FromImage(newImage))
			{
				using (var attributes = new ImageAttributes())
				{
					for (int i = 0; i < imageList.Count; i++)
					{
						var image = (Image)imageList[i];
						var destRect = new Rectangle(i * imageSize.Width, 0, imageSize.Width, imageSize.Height);
						graphics.DrawImage(image, destRect, 0, 0, imageSize.Width, imageSize.Height, GraphicsUnit.Pixel, null, null, IntPtr.Zero);
					}
				}
			}

			return newImage;
		}
	}
}
