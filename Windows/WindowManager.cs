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
//
//		internal const int MIN_WIN_IN_CASCADE = 3;
//		internal const int MIN_WIDTH_PIX = 600; // pixels
//		internal const int MIN_HEIGHT_PIX = 400; // pixels
//		internal const double MIN_WIDTH_PCT = 0.40; // percent
//
//		private int _topSelected;
//		private int _leftSelected;
//		private int _bottomSelected;
//		private int _rightSelected;
//		
//		private int _topMinimized;
//		private int _leftMinimized;
//		private int _rightMinMax;
//		
//		private int _heightMinimized = GetSystemMetrics(SystemMetric.SM_CYMINIMIZED);
//		private int _widthMinimized = GetSystemMetrics(SystemMetric.SM_CXMINIMIZED);
//		
//		private int _heightNotSel = GetSystemMetrics(SystemMetric.SM_CYMINTRACK);
//		private int _widthNotSel = GetSystemMetrics(SystemMetric.SM_CXMINTRACK);
//		
//		private int _indexNormal;
//		private int _indexMinimized;
//		private int _row;
//		
//		private int _winAdjVert = TitleBarHeight;
//		private int _winAdjHoriz = TitleBarHeight;

		private readonly WindowManagerUtilities _winMgrUtil;

		private IntPtr _active = IntPtr.Zero;

		internal static string messageStatus = "";
		internal static string messageError = "";

		private const string MSG_01 = "Could not Organize windows";

		internal string MessageStatus => messageStatus;
		internal string MessageError => messageError;


		public WindowManager()
		{
			_winMgrUtil = new WindowManagerUtilities();

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

					result = _winMgrUtil.OrganizeByProperCascade();

					if (!result)
					{
						messageStatus = MSG_01;
						return false;
					}

					ListInfo("PROPER CASCADE");

					_winMgrUtil.RepositionWindows();

					break;
				case 1:
					SortChildWindows();

					result = _winMgrUtil.OrganizeByBadCascade();

					if (!result)
					{
						messageStatus = MSG_01;
						return false;
					}

					ListInfo("WINDOW CASCADE");

					_winMgrUtil.RepositionWindows();

					break;
				case 2: // left side
					SortChildWindows();

					result = _winMgrUtil.OrganizeByActOnLeft();

					if (!result)
					{
						messageStatus = MSG_01;
						return false;
					}

					ListInfo("AT LEFT");

					_winMgrUtil.RepositionWindows();
					break;
				// bottom side
				case 3:
					SortChildWindows();

					result = _winMgrUtil.OrganizeByActOnBottom();

					if (!result)
					{
						messageStatus = MSG_01;
						return false;
					}

					ListInfo("AT BOTTOM");

					_winMgrUtil.RepositionWindows();

					break;
				// right side
				case 4:
					SortChildWindows();

					result = _winMgrUtil.OrganizeByActOnRight();

					if (!result)
					{
						messageStatus = MSG_01;
						return false;
					}

					ListInfo("AT RIGHT");

					_winMgrUtil.RepositionWindows();
					break;
				// top side
				case 5:
					SortChildWindows();

					result = _winMgrUtil.OrganizeByActOnTop();

					if (!result)
					{
						messageStatus = MSG_01;
						return false;
					}

					ListInfo("AT TOP");

					_winMgrUtil.RepositionWindows();
					break;
				// left - stacked
				case 6:
					SortChildWindows();

					result = _winMgrUtil.OrganizeByActOnLeftOverlapped();

					if (!result)
					{
						messageStatus = MSG_01;
						return false;
					}

					ListInfo("AT LEFT - OVERLAPPED");

					_winMgrUtil.RepositionWindows();
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
