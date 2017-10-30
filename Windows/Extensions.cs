#region Using directives

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Autodesk.Revit.DB.Structure;
using static Windows.WindowApiUtilities;

#endregion

// itemname:	Extensions
// username:	jeffs
// created:		10/21/2017 5:46:06 AM


namespace Windows
{
	//	class Extensions
	//	{
	//	}

	internal static class RectangleExtensions
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

			int width = r.Right + amount - left;
			int height = r.Bottom + amount - top;

			return new Rectangle(left, top, width, height);
		}

		// adjust the size of the rectangle by the set amounts
		// amountLR is the adjustment to the left / right numbers
		// amountTB is the adjustment to the top / bottom numbers
		public static Rectangle Adjust(this Rectangle r, int amountLR, int amountTB)
		{
			if (amountLR == 0 && amountTB == 0) { return r; }

			int left = r.Left - amountLR;
			int width = r.Right + amountLR - left;

			int top = r.Top - amountTB;
			int height = r.Bottom + amountTB - top;

			return new Rectangle(left, top, width, height);
		}
		
		// adjust the size of the rectangle by the set amounts
		// amountLeft is the adjustment to the left number, etc.
		public static Rectangle Adjust(this Rectangle r, int amountLeft, int amountTop, 
			int amountRight, int amountBottom)
		{
			if (amountLeft == 0 && amountTop == 0 
				&& amountRight == 0 && amountBottom == 0) { return r; }

			int left = r.Left - amountLeft;
			int width = r.Right + amountRight - left;

			int top = r.Top - amountTop;
			int height = r.Bottom + amountRight - top;

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

	internal static class RectExtensions
	{

		public static Rectangle AsRectangle(this RECT r)
		{
			return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
		}
	}

	internal static class IntExtensions
	{
		public static int RoundUp(this int operand, int divisor)
		{
			return operand % divisor == 0 ? operand / divisor : operand / divisor + 1;
		}

	}

	internal static class DictionaryExtensions
	{
		public static Dictionary<T1, int> Sort<T1>(this Dictionary<T1, int> d)
		{
			List<T1> list = d.Keys.ToList();

			int idx = 0;

			return list.ToDictionary(unknown => unknown, unknown => idx++);
		}
	}
	
}
