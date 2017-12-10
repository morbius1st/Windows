#region Using directives

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using static RevitWindows.RevitWindow;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowManagerUtilities;
using static RevitWindows.WindowListingUtilities;
using static RevitWindows.WindowManager.WindowLayoutStyle;

#endregion

// itemname:	WindowManager
// username:	jeffs
// created:		10/19/2017 8:34:36 PM


namespace RevitWindows
{
	class WindowManager
	{
		internal const bool DISPLAY_INFO = false;

		private readonly WindowManagerUtilities _winMgrUtil;

		internal static string messageStatus = "";
		internal static string messageError = "";

		private const string MSG_01 = "Could not Organize windows";

		private static IntPtr _parent;

		internal string MessageStatus => messageStatus;
		internal string MessageError => messageError;

		internal static UIApplication Uiapp;
		internal static UIDocument Uidoc;
		internal static Application App;
		internal static Document Doc;

		private static WindowLayoutStyle _currWinLayoutStyle = ACTIVE_LEFT_OVERLAP;


		public WindowManager(ExternalCommandData commandData)
		{
			if (commandData == null) return;

			Uiapp = commandData.Application;
			Uidoc = Uiapp.ActiveUIDocument;
			App = Uiapp.Application;
			Doc = Uidoc.Document;

			_parent = GetMainWinHandle(Doc);

			_winMgrUtil = new WindowManagerUtilities(_parent);
		}

		internal enum WindowLayoutStyle
		{
			PROPER_CASCADE,
			WINDOWS_CASCADE,
			ACTIVE_LEFT,
			ACTIVE_TOP,
			ACTIVE_RIGHT,
			ACTIVE_BOTTOM,
			ACTIVE_LEFT_OVERLAP
		}

		internal bool UpdateWindowLayout()
		{
			if (_parent == IntPtr.Zero ||
				Uiapp == null)
			{
				return false;
			}

			bool result = InitializeRevitWindows();

			if (!result)
			{
				return false;
			}

			if (AdjustWindowLayout((int) _currWinLayoutStyle))
			{
				return false;
			}

			return true;
		}

		internal static IntPtr Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		internal WindowLayoutStyle CurrWinLayoutStyle
		{
			get { return _currWinLayoutStyle; }
			set { _currWinLayoutStyle = value; }
		}

		internal bool AdjustNonActWidth(bool increase)
		{

			return _winMgrUtil.AdjustNonActWidth(_currWinLayoutStyle, increase);
		}

		internal bool AdjustNonActHeight(bool increase)
		{

			return _winMgrUtil.AdjustNonActHeight(_currWinLayoutStyle, increase);
		}

		private bool InitializeRevitWindows()
		{
//			GetScreenMetrics(_parent);

			// get the list of child windows
			if (!GetRevitChildWindows(_parent) ||
				CurrDocWinCount == 0)
			{
				return false;
			}

			return true;
		}

		internal bool AdjustWindowLayout(int windowLayoutStyle)
		{
			bool result = false;
			int row = 0;

			switch (windowLayoutStyle)
			{
				// proper sort
				case (int) PROPER_CASCADE:
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
					// windows sort
				case (int) WINDOWS_CASCADE:
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
				case (int) ACTIVE_LEFT: // left side
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
				case (int) ACTIVE_BOTTOM:
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
				case (int) ACTIVE_RIGHT:
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
				case (int) ACTIVE_TOP:
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
				case (int) ACTIVE_LEFT_OVERLAP:
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
