#region Using directives

using System;
using System.Configuration;


using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using EnvDTE;
using static RevitWindows.RevitWindow;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowManagerUtilities;
using static RevitWindows.WindowListingUtilities;
using static RevitWindows.WindowManager.WindowLayoutStyle;
using Document = Autodesk.Revit.DB.Document;

#endregion

// itemname:	WindowManager
// username:	jeffs
// created:		10/19/2017 8:34:36 PM


namespace RevitWindows
{
	class WindowManager
	{
		internal const bool DISPLAY_INFO = false;

		internal static string messageStatus = "";
		internal static string messageError = "";

		private const string MSG_01 = "Could not Organize windows";


		internal string MessageStatus => messageStatus;
		internal string MessageError => messageError;

		internal static UIApplication Uiapp;
		internal static UIDocument Uidoc;
		internal static Application App;
		internal static Document Doc;

		internal static UserSettings Us;

		internal static bool _autoUpdateOnActivateWindow = true;

		private static WindowLayoutStyle _currWinLayoutStyle = ACTIVE_LEFT_OVERLAP;
		private static IntPtr _parent;

		private static WindowManager _WinMgr = null;
		private static WindowManagerUtilities _winMgrUtil = null;

		private WindowManager() { }

		internal static WindowManager GetInstance(ExternalCommandData commandData)
		{
			if (commandData == null) return null;

			if (_WinMgr == null)
			{
				_WinMgr = new WindowManager();

				Uiapp = commandData.Application;
				Uidoc = Uiapp.ActiveUIDocument;
				App = Uiapp.Application;
				Doc = Uidoc.Document;

				Us = new UserSettings();

				_parent = GetMainWinHandle(Doc);

				_winMgrUtil = new WindowManagerUtilities(_parent);
			}

			RevitWindows.Properties.Settings x = Properties.Settings.Default;
			

			TaskDialog.Show("revit windows", "test is| " + x.Alpha + " " + x.Beta);

			x.Beta++;
			x.Save();

			TaskDialog.Show("revit windows", "test is| " + x.Alpha + " " + x.Beta);

			return _WinMgr;
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
