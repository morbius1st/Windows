#region Using directives
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using static Windows.Command;
using static Windows.WindowUtilities;
using static Windows.WindowApiUtilities.DeferWinPos;
using Rectangle = System.Drawing.Rectangle;

using static Windows.WindowApiUtilities;
using static Windows.RevitWindow;

#endregion

// itemname:	WindowManager
// username:	jeffs
// created:		10/19/2017 8:34:36 PM


namespace Windows
{
	class WindowManager
	{
		private IntPtr _parent;

		// given tenporary values
		private int _marginLeft;
		private int _marginTop;
		private int _marginRight;
		private int _marginBottom;

		private int _offset;

		public WindowManager(IntPtr parent)
		{
			_parent = parent;

			_offset = (int) (1.5 * TitleBarHeight);

			_marginLeft = 20;
			_marginTop = 20;
			_marginRight = 20;
			_marginBottom = 20;
	}

		internal bool AdjustWindowLayout(int windowLayoutStyle)
		{
			bool result = false;

			switch (windowLayoutStyle)
			{
				case 0:
					// for this, sort the windows first
					SortChildWindows();

					OrganizeByProperCascade();
					OrganizeMinimized();

					RepositionWindows();

					break;
				case 1:
//					SortChildWindows();
//
//					OrganizeByBadCascade();
//
//					RepositionWindows();

					break;
			}

			return true;
		}

		// organize the windows that have been minimized
		void OrganizeMinimized()
		{
			int horizIdx = 0;

			int height = ChildWinMinimized[0].current.Height;
			int top = ParentClientWindow.Height - height;
			int left = 0;
			int width = ChildWinMinimized[0].current.Width;

			// determine the maximum number of minimized windows to place horizontally
			int maxHorizontal = ParentClientWindow.Width / ChildWinMinimized[0].current.Width;

			for (int i = 0; i < ChildWinMinimized.Count; i++)
			{
				if (horizIdx == maxHorizontal)
				{
					top -= height;
					left = ParentClientWindow.Left;
					horizIdx = 0;
				}

				ChildWinMinimized[i].proposed = new Rectangle(left, top, width, height);

				left += width;
				horizIdx++;
			}
		}

		// organize method 0 - proper cascade
		void OrganizeByProperCascade()
		{
			int top;
			int left;
			int right = ParentClientWindow.Width - _marginRight;
			int bottom = ParentClientWindow.Height - _marginBottom;
			int inverseCount = 0;

			int count = ChildWindows.Count;

			int idx = 0;

			for (int i = 0; i < count; i++)
			{
				inverseCount = count - idx;
				top = idx * TitleBarHeight + _marginTop;
				left = _offset * inverseCount + _marginLeft;

				ChildWindows[i].proposed = NewRectangle(left, top, right, bottom);

				idx++;
			}
		}

//		// organize by windows stupid cascade method
//		void OrganizeByBadCascade()
//		{
//			Rectangle rect;
//
//			int width = (int) (ParentClientWindow.Width * 0.6);
//			int height = (int) (ParentClientWindow.Height * 0.6);
//
//			width = width > MinWindowWidth ? width : MinWindowWidth;
//			height = height > MinWindowHeight ? height : MinWindowHeight;
//
//			int offset = TitleBarHeight;
//
//			int count = ChildWindows.Count;
//
//			int idx = 0;
//
//			for (int i = 0; i < count; i++)
//			{
//				rect = CalcBadCascadeRect(width, height, offset, ref idx);
//
//				ChildWindows[i].proposed = rect;
//
//				idx++;
//
//				if (ChildWindows[i].IsMinimized)
//				{
//					OrganizeMinimized(i);
//					break;
//				}
//			}
//		}

		Rectangle CalcBadCascadeRect(int width, int height, int offset, ref int idx)
		{
			int left = idx * offset + +_marginLeft;
			int top = idx * offset + _marginTop;

			int right = left + width;
			int bottom = top + height;

			if (right + _marginRight > ParentClientWindow.Right || bottom + _marginBottom > ParentClientWindow.Bottom)
			{
				idx = 0;
				left = _marginLeft;
				top = _marginTop;
			}

			return new Rectangle(left, top, width, height);
		}



		// based on the adjusted list of windows, reposition all of the windows.
		void RepositionWindows()
		{
			IntPtr hWinPosInfo =
				BeginDeferWindowPos(ChildWindows.Count);// + MinWindows.Count);

			foreach (RevitWindow rw in ChildWindows)
			{
				// process a non-minimized window
				hWinPosInfo =
					DeferWindowPos(hWinPosInfo, rw.Handle, ((IntPtr) 0),
						rw.proposed.Left, rw.proposed.Top, rw.proposed.Width, rw.proposed.Height,
						SWP_NOACTIVATE | SWP_SHOWWINDOW);

				if (hWinPosInfo.Equals(null)) { return; }
			}

			foreach (RevitWindow rw in ChildWinMinimized)
			{
				// process a minimized window
				hWinPosInfo =
					DeferWindowPos(hWinPosInfo, rw.Handle, ((IntPtr) 1),
						rw.proposed.Left, rw.proposed.Top, rw.proposed.Width, rw.proposed.Height,
						SWP_NOACTIVATE | SWP_NOSIZE | SWP_SHOWWINDOW);


				if (hWinPosInfo.Equals(null)) { return; }
			}

			EndDeferWindowPos(hWinPosInfo);

			if (ActiveWindow != IntPtr.Zero)
			{
				ShowWindow(ActiveWindow, ShowWinCmds.SW_SHOWNORMAL);
			}
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
