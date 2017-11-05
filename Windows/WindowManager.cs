#region Using directives

using System;
using System.Collections.Generic;
using Rectangle = System.Drawing.Rectangle;

using static RevitWindows.RevitWindow;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowListingUtilities;
using static RevitWindows.WindowManagerUtilities;
using static RevitWindows.WindowApiUtilities.DeferWinPos;

#endregion

// itemname:	WindowManager
// username:	jeffs
// created:		10/19/2017 8:34:36 PM


namespace RevitWindows
{
	class WindowManager
	{
		private IntPtr _parent;


		public WindowManager(IntPtr parent)
		{
			_parent = parent;
	}

		internal bool AdjustWindowLayout(int windowLayoutStyle)
		{
			bool result = false;
			int row = 0;

			InsureOneChildWindow();

			switch (windowLayoutStyle)
			{
				case 0:
					// for this, sort the windows first
					SortChildWindows();

					OrganizeByProperCascade();

					row = OrganizeSecondarynWindows(ChildWinMinimized, row);
					OrganizeSecondarynWindows(ChildWinOther, ++row);

					RepositionWindows();


					break;
				case 1:

//					ListChildWin(ChildWindows, nl + "child windows before sort", 3);

					// for this, sort the windows first
					SortChildWindows();

					OrganizeByBadCascade();

//					ListChildWin(ChildWindows, nl + "child windows after sort and organize", 3);

					row = OrganizeSecondarynWindows(ChildWinMinimized, row);
					OrganizeSecondarynWindows(ChildWinOther, ++row);

					RepositionWindows();

					break;
			}

			return true;
		}

		// organize the windows that have been minimized
//		void OrganizeMinimized()
//		{
//			if (ChildWinMinimized.Count == 0) { return; }
//
//			int horizIdx = 0;
//
//			int height = ChildWinMinimized[0].current.Height;
//			int top = ParentWindow.Height - height;
//			int left = 0;
//			int width = ChildWinMinimized[0].current.Width;
//
//			// determine the maximum number of minimized windows to place horizontally
//			int maxHorizontal = ParentWindow.Width / width;
//
//			for (int i = 0; i < ChildWinMinimized.Count; i++)
//			{
//				if (horizIdx == maxHorizontal)
//				{
//					top -= height;
//					left = ParentWindow.Left;
//					horizIdx = 0;
//				}
//				ChildWinMinimized[i].proposed = new Rectangle(left, top, width, height);
//
//				left += width;
//				horizIdx++;
//			}
//		}

		// organize other windows to be minimized
		// note that the height and width values are for  x / y position
		// only the height / width is based on the minimized window size
		// row is counted from the bottom up
		int OrganizeSecondarynWindows(List<RevitWindow> rws, int row)
		{
			if (rws.Count == 0) { return -1; }

			int horizIdx = 0;

			int height = GetSystemMetrics(SystemMetric.SM_CYMINIMIZED);
			int left = 0;
			int width = GetSystemMetrics(SystemMetric.SM_CXMINIMIZED);

			// adjust the top value to be the window height adjusted for the row number
			int top = ParentWindow.Height - (height * (row + 1));

			// determine the maximum number of minimized windows to place horizontally
			int maxHorizontal = ParentWindow.Width / width;

			foreach (RevitWindow rw in rws) {
				if (horizIdx == maxHorizontal)
				{
					row++;
					top -= height;
					left = 0;
					horizIdx = 0;
				}
				rw.proposed = new Rectangle(left, top, width, height);

				// set minimized if not already minimized
				if (!rw.IsMinimized)
				{
					ShowWindow(rw.Handle, ShowWinCmds.SW_MINIMIZE);
				}

				left += width;
				horizIdx++;
			}

			return row;
		}

		// organize method 0 - proper cascade
		void OrganizeByProperCascade()
		{
			if (ChildWindows.Count == 0) { return; }

			int top;
			int left;
			int right = ParentWindow.Width - MarginRight;
			int bottom = ParentWindow.Height - MarginBottom;
			int inverseCount = 0;

			int count = ChildWindows.Count;

			int idx = 0;

			for (int i = 0; i < count; i++)
			{
				inverseCount = count - idx;
				top = idx * TitleBarHeight + MarginTop;
				left = Offset * inverseCount + MarginLeft;

				ChildWindows[i].proposed = NewRectangle(left, top, right, bottom);

				idx++;
			}
		}

		// organize by windows stupid cascade method
		void OrganizeByBadCascade()
		{
			Rectangle rect;

			int width = (int) (ParentWindow.Width * 0.6);
			int height = (int) (ParentWindow.Height * 0.6);

			width = width > MinWindowWidth ? width : MinWindowWidth;
			height = height > MinWindowHeight ? height : MinWindowHeight;

			int count = ChildWindows.Count;

			// make sure there is enough height and width to actually cascade the windows
			// allow for 3 times the offset amount
			if (MarginTop + height + MarginBottom + Offset * 3 >= ParentWindow.Height
				|| MarginLeft + width + MarginRight + Offset * 3 >= ParentWindow.Width)
			{
				return;
			}

			int idx = 0;
			int col = 0;

			for (int i = 0; i < count; i++)
			{
				rect = CalcBadCascadeRect(width, height, ref idx, ref col);

				ChildWindows[i].proposed = rect;

				idx++;
			}
		}

		// based on the adjusted list of windows, reposition all of the windows.
		void RepositionWindows()
		{
			IntPtr insertAfter = HWND_BOTTOM;

			IntPtr hWinPosInfo =
				BeginDeferWindowPos(ChildWindows.Count);// + MinWindows.Count);

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.Handle.Equals(ActiveWindow))
				{
					insertAfter = HWND_TOP;
				}

				// process a non-minimized window
				hWinPosInfo =
					DeferWindowPos(hWinPosInfo, rw.Handle, insertAfter,
						rw.proposed.Left, rw.proposed.Top, rw.proposed.Width, rw.proposed.Height,
						SWP_SHOWWINDOW);

				insertAfter = HWND_BOTTOM;

				if (hWinPosInfo.Equals(null)) { return; }
			}

			hWinPosInfo = RepositionSecondaryWindows(hWinPosInfo, ChildWinMinimized);

			if (hWinPosInfo == IntPtr.Zero) { return; }

			hWinPosInfo = RepositionSecondaryWindows(hWinPosInfo, ChildWinOther);

			if (hWinPosInfo == IntPtr.Zero) { return; }

			EndDeferWindowPos(hWinPosInfo);

		}

		void RepositionSecondaryWindows(List<RevitWindow> rws)
		{
			foreach (RevitWindow rw in rws)
			{
				ShowWindow(rw.Handle, ShowWinCmds.SW_MINIMIZE);
			}
		}

		IntPtr RepositionSecondaryWindows(IntPtr hWinPosInfo, List<RevitWindow> rws)
		{
			foreach (RevitWindow rw in rws)
			{
				// process a minimized window
				hWinPosInfo =
					DeferWindowPos(hWinPosInfo, rw.Handle, HWND_BOTTOM,
						rw.proposed.Left, rw.proposed.Top, rw.proposed.Width, rw.proposed.Height,
						SWP_NOACTIVATE | SWP_SHOWWINDOW | SWP_NOSIZE);

				if (hWinPosInfo.Equals(null)) { return IntPtr.Zero; }
			}

			return hWinPosInfo;
		}


		/*
		 * don't need maximized flag in rw
		 
		- create rw - set sequence using
			- current document active = 0+
			- current document normal = 100+
			- current document minimized = 200+
			- other document = 1000+

		- insure have min 1 normal & active window

		- organize
			organize by sorting 
				- current active = 0 to 99
				- current normal = 100 to 199
				- other = 200+

			if window is a current document window
			- set current flag = true
				if not minimized
				- set correct position & size
				if minimized
				- set correct position

			if window is not current document
			- set current flag = false
			- set correct position - each document to its separate row

		- reposition - don't use deferwindowpos
			if current flag == true
				if not minimized
					if active window
					yes
						- show window normal (sw_show)
						- set window position 
							(hwnd_top)
							(swp_showwindow)
					no
						-show window normal (sw_showna)
						- set window position 
							(hwnd_bottom)
							(swp_showwindow | swp_noactivate)
				
				if minimized
					- show window minimized (sw_showminimized)
					- set window position
						(hwnd_bottom)
						(swp_nosize | swp_showwindow | swp+noactivate)
			if current flag == false
				- minimize all windows
				- show window minimized (sw_showminimized)
				- set window position
					(hwnd_bottom)
					(swp_nosize | swp_showwindow | swp+noactivate)

		*/
	}
}
