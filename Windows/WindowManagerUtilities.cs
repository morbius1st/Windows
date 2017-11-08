#region Using directives

using System;
using System.Drawing;

using static RevitWindows.WindowUtilities;
using static RevitWindows.RevitWindow;
using static RevitWindows.WindowApiUtilities;

#endregion

// itemname:	WindowManagerUtilities
// username:	jeffs
// created:		11/4/2017 7:54:03 AM


namespace RevitWindows
{
	internal class WindowManagerUtilities
	{
		internal static int MarginLeft { get; } = 20;
		internal static int MarginTop { get; } = 20;
		internal static int MarginRight { get; } = 20;
		internal static int MarginBottom { get; } = 20;

		internal static int OffsetVert { get; } = TitleBarHeight;
		internal static int OffsetHoriz { get; } = TitleBarHeight;


		// make sure that there is at least one ChildWindow and 
		// that this is the active window
//		internal static void InsureOneChildWindow()
//		{
//			if (ChildWindows.Count > 0) { return; }
//
//			RevitWindow rw = ChildWinMinimized[0];
//
//			ChildWindows.Add(rw);
//
//			ChildWinMinimized.RemoveAt(0);
//
//			rw.MakeActive();
//
//			ShowWindow(rw.Handle, ShowWinCmds.SW_RESTORE);
//
//		}

		internal static Rectangle CalcBadCascadeRect(int width, int height, ref int idx, ref int col)
		{
			const double leftAdj = 4;
			const double topAdj = 0.5;

			int left = CalcTopLeft(idx, col, MarginLeft, leftAdj);
			int top = CalcTopLeft(idx, col, MarginTop, topAdj);
			//
			//			Rectangle ParentWin = ParentWindow;
			//			Rectangle DisplayWin = DisplayScreenRect;


			if (left + width + MarginRight > ParentWindow.Width || 
				top + height + MarginBottom > ParentWindow.Height)
			{
				idx = 0;
				col++;
				left = CalcTopLeft(idx, col, MarginLeft, leftAdj);
				top = CalcTopLeft(idx, col, MarginTop, topAdj);
			}

			return new Rectangle(left, top, width, height);
		}

		static int CalcTopLeft(int idx, int col, int margin, double adjAmt)
		{
			return (int) ((idx + (adjAmt * col)) * OffsetVert) + margin;
		}



		//		// set up form with revised windows
		//		void SetupFormChildProp()
		//		{
		//			int idx = SetFormChildProposed(ActWindows, 0, false);
		//
		//			//			idx = SetFormChildProposed(MinWindows, idx, true);
		//
		//		}
		//
		//		int SetFormChildProposed(List<RevitWindow> rws, int idx, bool isMin)
		//		{
		//			foreach (RevitWindow rw in rws)
		//			{
		//				if (rw.proposed.Width == 0) { continue; }
		//
		//				MForm.SetChildProp(idx++, rw.proposed, rw.WindowTitle, isMin);
		//			}
		//
		//			return idx;
		//		}


	}
}
