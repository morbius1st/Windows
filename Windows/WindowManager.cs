#region Using directives

using System;
using System.Collections.Generic;
using System.Windows.Interop;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Rectangle = System.Drawing.Rectangle;

using static RevitWindows.RevitWindow;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowListingUtilities;

#endregion

// itemname:	WindowManager
// username:	jeffs
// created:		10/19/2017 8:34:36 PM


namespace RevitWindows
{
	class WindowManager
	{
		internal const bool DISPLAY_INFO = false;

		internal const int MIN_WIN_IN_CASCADE = 3;
		internal const int MIN_WIDTH_PIX = 600; // pixels
		internal const int MIN_HEIGHT_PIX = 400; // pixels
		internal const double MIN_WIDTH_PCT = 0.40; // percent

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

		private WindowManagerUtilities winMgrUtil;


		private IntPtr _active = IntPtr.Zero;

		public WindowManager()
		{
			winMgrUtil = new WindowManagerUtilities();

		}

		internal bool AdjustWindowLayout(int windowLayoutStyle)
		{
			bool result = false;
			int row = 0;

//			InsureOneChildWindow();

			switch (windowLayoutStyle)
			{
				case 0:
					// for this, sort the windows first
					SortChildWindows();

					result = winMgrUtil.OrganizeByProperCascade();

					if (!result)
					{
						return false;
					}

					ListInfo("PROPER CASCADE");

					winMgrUtil.RepositionWindows();

					break;
				case 1:
					SortChildWindows();

					result = winMgrUtil.OrganizeByBadCascade();

					if (!result)
					{
						logMsgln("organize failed");
						return false;
					}

					ListInfo("WINDOW CASCADE");

					winMgrUtil.RepositionWindows();

					break;
				case 2: // left side
					SortChildWindows();

					result = winMgrUtil.OrganizeByActOnLeft();

					if (!result)
					{
						logMsgln("organize failed");
						return false;
					}

					ListInfo("AT LEFT");

					winMgrUtil.RepositionWindows();
					break;
				// bottom side
				case 3:
					SortChildWindows();

					result = winMgrUtil.OrganizeByActOnBottom();

					if (!result)
					{
						logMsgln("organize failed");
						return false;
					}

					ListInfo("AT BOTTOM");

					winMgrUtil.RepositionWindows();

					break;
				// right side
				case 4:
					SortChildWindows();

					result = winMgrUtil.OrganizeByActOnRight();

					if (!result)
					{
						logMsgln("organize failed");
						return false;
					}

					ListInfo("AT RIGHT");

					winMgrUtil.RepositionWindows();
					break;
				// top side
				case 5:
					SortChildWindows();

					result = winMgrUtil.OrganizeByActOnTop();

					if (!result)
					{
						logMsgln("organize failed");
						return false;
					}

					ListInfo("AT TOP");

					winMgrUtil.RepositionWindows();
					break;
				// left - stacked
				case 6:
					SortChildWindows();

					result = winMgrUtil.OrganizeByActOnLeftOverlapped();

					if (!result)
					{
						logMsgln("organize failed");
						return false;
					}

					ListInfo("AT LEFT - OVERLAPPED");

					winMgrUtil.RepositionWindows();
					break;
			}
			return true;
		}

		void ListInfo(string who)
		{
			if (!DISPLAY_INFO) return;

			UtilityLibrary.MessageUtilities.clearConsole();

			ListChildWin(ChildWindows, nl + "child windows after sort and organize - " + who,
				1, 2, 3, 4, 5, 6, 7, 10, 9);
		}


	}
}
