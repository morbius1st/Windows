#region Using directives
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#endregion

// itemname:	Extensions
// username:	jeffs
// created:		10/21/2017 5:46:06 AM


namespace Windows
{
//	class Extensions
//	{
//	}

	static class RectangleExtensions
	{
		// adjusts the size of the rectangle by a set amount
		// adjustment is made so that a negative number is a
		// smaller rectangle and a positive number is a larger
		// window
		public static Rectangle Adjust(this Rectangle r, int amount)
		{
			if (amount == 0) { return r; }

			int top = r.Top - amount;
			int left = r.Left - amount;
			int width = r.Right - r.Left + amount * 2;
			int height = r.Bottom - r.Top + amount * 2;

			return new Rectangle(left, top, width, height);
		}

		public static Rectangle SetRight(this Rectangle r, int right)
		{
			r.Width = right - r.Left;

			return r;
		}

		public static Rectangle SetBottom(this Rectangle r, int bottom)
		{
			r.Height = bottom - r.Top;

			return r;
		}
	}
}
