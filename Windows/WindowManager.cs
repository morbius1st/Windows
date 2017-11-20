#region Using directives

using System;
using System.Collections.Generic;
using System.Windows.Interop;
using Autodesk.Revit.DB.Events;
using Rectangle = System.Drawing.Rectangle;

using static RevitWindows.RevitWindow;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowListingUtilities;
using static RevitWindows.WindowManagerUtilities;
using static RevitWindows.WindowApiUtilities.DeferWinPos;
using static RevitWindows.WindowApiUtilities.ShowWinCmds;
using static RevitWindows.WindowApiUtilities.SetWindowPosFlags;

#endregion

// itemname:	WindowManager
// username:	jeffs
// created:		10/19/2017 8:34:36 PM


namespace RevitWindows
{
	class WindowManager
	{
		private const int MIN_WIN_IN_CASCADE = 3;
		private const int MIN_WIDTH_PIX = 600; // pixels
		private const int MIN_HEIGHT_PIX = 400; // pixels
		private const double MIN_WIDTH_PCT = 0.40; // percent

		private int _topSelected;
		private int _leftSelected;
		private int _bottomSelected;
		private int _rightSelected;

		private int _topMinimized;
		private int _leftMinimized;
		private int _rightMinMax;

		private int _heightMinimized = GetSystemMetrics(SystemMetric.SM_CYMINIMIZED);
		private int _widthMinimized = GetSystemMetrics(SystemMetric.SM_CXMINIMIZED);

		private int _heightNotSel = GetSystemMetrics(SystemMetric.SM_CYMINTRACK);
		private int _widthNotSel = GetSystemMetrics(SystemMetric.SM_CXMINTRACK);

		private int _indexNormal;
		private int _indexMinimized;
		private int _row;

		private int _winAdjVert = TitleBarHeight;
		private int _winAdjHoriz = TitleBarHeight;


		private IntPtr _active = IntPtr.Zero;

		public WindowManager()
		{
//			_parent = parent;
		}

		internal bool AdjustWindowLayout(int windowLayoutStyle, int whichDoc)
		{
			bool result = false;
			int row = 0;

//			InsureOneChildWindow();

			switch (windowLayoutStyle)
			{
				case 0:
					// for this, sort the windows first
					SortChildWindows();

					result = OrganizeByProperCascade(whichDoc);

					if (!result)
					{
						logMsgln("organize failed");
						return false;
					}

					ListChildWin(ChildWindows, nl + "child windows after sort and organize", 
						1, 2, 3, 4, 5, 6, 7);

//					RepositionWindows();

					break;
				case 1:

//					ListChildWin(ChildWindows, nl + "child windows before sort", 3);

					// for this, sort the windows first
//					SortChildWindows();
//
//					OrganizeByBadCascade();

//					ListChildWin(ChildWindows, nl + "child windows after sort and organize", 3);

//					row = OrganizeSecondarynWindows(ChildWinMinimized, row);
//					OrganizeSecondarynWindows(ChildWinOther, ++row);

//					RepositionWindows();

					break;
			}

			return true;
		}

		bool ProperCascadeFits()
		{
			if (SelectedWinCount == 0) { return false; }

			int minWinHeight = Math.Max((int) (ParentWindow.Height * MIN_WIDTH_PCT), MIN_WIDTH_PIX);
			int minWinWidth = Math.Max((int) (ParentWindow.Width * MIN_WIDTH_PCT), MIN_HEIGHT_PIX);

			int maxWindowsHoriz = (ParentWindow.Width - MarginRight
				- MarginLeft - minWinWidth) / _winAdjHoriz + 1;

			int maxWindowsVert = (ParentWindow.Height - MarginTop
				- MarginBottom - minWinHeight) / _winAdjVert + 1;

			// can all of the windows be cascaded?
			if (maxWindowsHoriz < SelectedWinCount
				|| maxWindowsVert < SelectedWinCount) { return false; }

			return true;
		}


		// proper cascade = cascade from top right to bottom left - 
		// keep right edge at right margin and bottom edge at bottom margin
		// left edge to left margin max
		bool OrganizeByProperCascade(int whichDoc)
		{
			if (SelectedWinCount == 0) return false;

			if (!ProperCascadeFits()) return false;

			bool gotFirstMinimized = false;
			bool gotFirstNotSel = false;

			_topSelected = MarginTop;
			_leftSelected = MarginLeft + SelectedWinCount * _winAdjHoriz;
			_rightSelected = ParentWindow.Width - MarginRight;
			_bottomSelected = ParentWindow.Height - MarginBottom;

			_topMinimized = ParentWindow.Height;
			_leftMinimized = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsMinimized)
				{
					if (!gotFirstMinimized)
					{
						_topMinimized -= _heightMinimized;
						_leftMinimized = 0;
						gotFirstMinimized = true;
					}
					rw.Proposed = RectforMinimized();
				} 
				else if (rw.DocIndex != whichDoc)
				{
					if (!gotFirstNotSel)
					{
						_topMinimized -= _heightNotSel;
						_leftMinimized = 0;
						gotFirstNotSel = true;
					}

					rw.Proposed = RectForNotSelected();
				} 
				else
				{
					rw.Proposed = RectForProperCascade();
				}
			}
			return true;
		}

		Rectangle RectForProperCascade()
		{
			Rectangle r = NewRectangle(_leftSelected, _topSelected,
				_rightSelected, _bottomSelected);

			_leftSelected -= _winAdjHoriz;
			_topSelected += _winAdjVert;

			return r;
		}

		Rectangle RectforMinimized()
		{
			if (_leftMinimized + _widthMinimized > ParentWindow.Width)
			{
				_topMinimized -= _heightMinimized;
				_leftMinimized = 0;
			}

			Rectangle r = new Rectangle(_leftMinimized, _topMinimized, _widthMinimized, _heightMinimized);

			_leftMinimized += _widthMinimized;

			return r;
		}

		Rectangle RectForNotSelected()
		{
			if (_leftMinimized + _widthNotSel > ParentWindow.Width)
			{
				_topMinimized -= _heightNotSel;
				_leftMinimized = 0;
			}

			Rectangle r = new Rectangle(_leftMinimized, _topMinimized, _widthNotSel, _heightNotSel);

			_leftMinimized += _widthNotSel;

			return r;
		}


		void RepositionWindows()
		{
			RevitWindow active = null;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsActive)
				{
					active = rw;
				}
				else if (rw.IsSelected || rw.IsNonSelected)
				{
					SetWindowPos(rw.Handle, HWND.Bottom, rw.Proposed.X, rw.Proposed.Y,
						rw.Proposed.Width, rw.Proposed.Height, SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE);
				}
				else if (rw.IsMinimized)
				{
					SetWindowPos(rw.Handle, HWND.Bottom, rw.Proposed.X, rw.Proposed.Y,
						rw.Proposed.Width, rw.Proposed.Height, SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSIZE);
				}
			}

			SetWindowPos(active.Handle, HWND.Top, active.Proposed.X, active.Proposed.Y,
						active.Proposed.Width, active.Proposed.Height, SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE);
		}


		// organize other windows to be minimized
		// note that the height and width values are for  x / y position
		// only the height / width is based on the minimized window size
		// row is counted from the bottom up
		//		bool OrganizeSecondarynWindows()
		//		{
		//			if (OtherDocWinCount == 0) { return false; }
		//
		//			int left = 0;
		//			int height = GetSystemMetrics(SystemMetric.SM_CYMINIMIZED);
		//			int width = GetSystemMetrics(SystemMetric.SM_CXMINIMIZED);
		//			int top = ParentWindow.Height - height;
		//
		//			for (int i = OtherDocWinCount; i < NormalWinCount + MinimizedWinCount + OtherDocWinCount; i++)
		//			{
		//				RevitWindow rw = ChildWindows[i];
		//
		//				if (left + width > ParentWindow.Width)
		//				{
		//					top -= height;
		//					left = 0;
		//				}
		//
		//				rw.Proposed = new Rectangle(left, top, width, height);
		//
		//				left += width;
		//			}
		//
		//			return true;
		//		}


		//		void RepositionWindowsOther_1()
		//		{
		//			if (OtherDocWinCount == 0) return;
		//
		//			RevitWindow rw;
		//
		//			for (int i = MinimizedWinCount; i < NormalWinCount + MinimizedWinCount + OtherDocWinCount; i++)
		//			{
		//				// these are current document windows
		//				// these are not minimized
		//				// one must be the active window
		//
		//				rw = ChildWindows[i];
		//
		//				SetWindowPos(rw.Handle, HWND.Bottom, rw.Proposed.X, rw.Proposed.Y,
		//						rw.Proposed.Width, rw.Proposed.Height, 
		//						NOSIZE | SHOWWINDOW | NOACTIVATE);
		//			}
		//		}



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
		//

		//		int MinWinSize(int clientSize, double minSizeFactor, int minAllowableSize)
		//		{
		//			int minSize = (int) (clientSize * minSizeFactor);
		//
		//			if (minSize < minAllowableSize) minSize = minAllowableSize;
		//
		//			return minSize;
		//		}
		//

		//
		//		// organize by windows stupid cascade method
		//		void OrganizeByBadCascade()
		//		{
		//			Rectangle rect;
		//
		//			int width = (int) (ParentWindow.Width * 0.6);
		//			int height = (int) (ParentWindow.Height * 0.6);
		//
		//			width = width > MinWindowWidth ? width : MinWindowWidth;
		//			height = height > MinWindowHeight ? height : MinWindowHeight;
		//
		//			int count = ChildWindows.Count;
		//
		//			// make sure there is enough height and width to actually cascade the windows
		//			// allow for 3 times the offset amount
		//			if (MarginTop + height + MarginBottom + Offset * MIN_WIN_IN_CASCADE >= ParentWindow.Height
		//				|| MarginLeft + width + MarginRight + Offset * MIN_WIN_IN_CASCADE >= ParentWindow.Width)
		//			{
		//				return;
		//			}
		//
		//			int idx = 0;
		//			int col = 0;
		//
		//			for (int i = 0; i < count; i++)
		//			{
		//				rect = CalcBadCascadeRect(width, height, ref idx, ref col);
		//
		//				ChildWindows[i].Proposed = rect;
		//
		//				idx++;
		//			}
		//		}

		// based on the adjusted list of windows, reposition all of the windows.
		//		void RepositionWindows()
		//		{
		//			IntPtr insertAfter = HWND.Bottom;
		//
		//			IntPtr hWinPosInfo = 
		//				BeginDeferWindowPos(ChildWindows.Count);// + MinWindows.Count);
		//
		//			foreach (RevitWindow rw in ChildWindows)
		//			{
		//				if (rw.IsActive)
		//				{
		//					insertAfter = HWND.Top;
		//				}
		//
		//				// process a non-minimized window
		//				hWinPosInfo =
		//					DeferWindowPos(hWinPosInfo, rw.Handle, insertAfter,
		//						rw.Proposed.Left, rw.Proposed.Top, rw.Proposed.Width, rw.Proposed.Height,
		//						SWP_SHOWWINDOW);
		//
		//				insertAfter = HWND.Bottom;
		//
		//				if (hWinPosInfo.Equals(null)) { return; }
		//			}
		//
		////			hWinPosInfo = RepositionSecondaryWindows(hWinPosInfo, ChildWinMinimized);
		////
		////			if (hWinPosInfo == IntPtr.Zero) { return; }
		////
		////			hWinPosInfo = RepositionSecondaryWindows(hWinPosInfo, ChildWinOther);
		////
		////			if (hWinPosInfo == IntPtr.Zero) { return; }
		//
		//			EndDeferWindowPos(hWinPosInfo);
		//
		//		}

		//		void RepositionSecondaryWindows(List<RevitWindow> rws)
		//		{
		//			foreach (RevitWindow rw in rws)
		//			{
		//				ShowWindow(rw.Handle, ShowWinCmds.SW_MINIMIZE);
		//			}
		//		}
		//
		//		IntPtr RepositionSecondaryWindows(IntPtr hWinPosInfo, List<RevitWindow> rws)
		//		{
		//			foreach (RevitWindow rw in rws)
		//			{
		//				// process a minimized window
		//				hWinPosInfo =
		//					DeferWindowPos(hWinPosInfo, rw.Handle, HWND.Bottom,
		//						rw.Proposed.Left, rw.Proposed.Top, rw.Proposed.Width, rw.Proposed.Height,
		//						SWP_NOACTIVATE | SWP_SHOWWINDOW | SWP_NOSIZE);
		//
		//				if (hWinPosInfo.Equals(null)) { return IntPtr.Zero; }
		//			}
		//
		//			return hWinPosInfo;
		//		}
		//
		//		void RepositionWindows()
		//		{
		//			RevitWindow rw;
		//			IntPtr hwnd;
		//			SetWindowPosFlags showFlags;
		//
		//			for (int i = 1; i < SelectedWinCount + MinimizedWinCount; i++)
		//			{
		//				// these are current document windows
		//				// these are not minimized
		//				// one must be the active window
		//
		//				rw = ChildWindows[i];
		//
		//				ShowWindow(rw.Handle, SW_RESTORE);
		//				SetWindowPos(rw.Handle, HWND.Bottom, rw.Proposed.X, rw.Proposed.Y,
		//						rw.Proposed.Width, rw.Proposed.Height, SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE);
		//			}
		//
		//			rw = ChildWindows[0];
		//
		//			ShowWindow(rw.Handle, SW_RESTORE);
		//			SetWindowPos(rw.Handle, HWND.Top, rw.Proposed.X, rw.Proposed.Y,
		//					rw.Proposed.Width, rw.Proposed.Height, SetWindowPosFlags.SWP_SHOWWINDOW);
		//		}
		//
		//		void RepositionWindowsMinimized_1()
		//		{
		//			if (MinimizedWinCount == 0) return;
		//
		//			RevitWindow rw;
		//
		//			for (int i = SelectedWinCount; i < SelectedWinCount + MinimizedWinCount; i++)
		//			{
		//				// these are current document windows
		//				// these are not minimized
		//				// one must be the active window
		//
		//				rw = ChildWindows[i];
		//
		//				SetWindowPos(rw.Handle, HWND.Bottom, rw.Proposed.X, rw.Proposed.Y,
		//						rw.Proposed.Width, rw.Proposed.Height, 
		//						SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE);
		//			}
		//		}

	}
}
