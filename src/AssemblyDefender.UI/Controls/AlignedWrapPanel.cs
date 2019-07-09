using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AssemblyDefender.UI
{
	public class AlignedWrapPanel : WrapPanel
	{
		protected override Size MeasureOverride(Size constraint)
		{
			double itemWidth = 0;
			double itemHeight = 0;

			var internalChildren = base.InternalChildren;

			for (int i = 0; i < internalChildren.Count; i++)
			{
				var element = internalChildren[i];
				if (element == null)
					continue;

				element.Measure(constraint);

				var desiredSize = element.DesiredSize;

				if (itemWidth < desiredSize.Width)
					itemWidth = desiredSize.Width;

				if (itemHeight < desiredSize.Height)
					itemHeight = desiredSize.Height;
			}

			ItemWidth = itemWidth;
			ItemHeight = itemHeight;

			return base.MeasureOverride(constraint);
		}
	}
}
