#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using Rectangle = System.Drawing.Rectangle;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using static Windows.Command;
using static Windows.WindowUtilities;
using static Windows.WindowApiUtilities;



#endregion

// itemname:	WindowListingUtilities
// username:	jeffs
// created:		10/23/2017 8:50:38 PM


namespace Windows
{
	class WindowListingUtilities
	{
		internal const string nl = "\r\n";
		internal const string pattRect = "x1|{0,5:D} y1|{1,5:D} x2|{2,5:D} y2|{3,5:D}";

		void ListAllChildWindows(IntPtr parent)
		{
			uint copy = 0;
			string result = "";

			List<IntPtr> children = GetChildWindows(parent);

			if (children == null || children.Count == 0) { return; }

			foreach (IntPtr child in children)
			{
				_WinStyleData = new int[_WinStyleCount];
				_WinStyleExData = new int[_WinStyleExCount];

				WindowApiUtilities.WINDOWINFO wi = new WindowApiUtilities.WINDOWINFO(true);
				GetWindowInfo(child, ref wi);

				WindowApiUtilities.WINDOWPLACEMENT wp = WindowApiUtilities.WINDOWPLACEMENT.Default;
				GetWindowPlacement(child, ref wp);

				StringBuilder className = new StringBuilder(40);
				GetClass(child, className, 40);

				StringBuilder winText = new StringBuilder(40);
				GetWindowText(child, winText, 40);

				AnalyzeWinStyles<WindowApiUtilities.WinStyle>(wi.dwStyle, ref _WinStyleRecord,
					ref _WinStyleData, ref copy, ref result);
				AnalyzeWinStyles<WindowApiUtilities.WinStyleEx>(wi.dwExStyle, ref _WinStyleExRecord,
					ref _WinStyleExData, ref copy, ref result);

				logMsg(" win class| " + $"{className,-42}");

				logMsg("    win style data| > ");
				ListWinStyleResult(_WinStyleData);

				logMsg("  win text| " + $"{winText,-42}");
				logMsg(" win style ex data| > ");
				ListWinStyleResult(_WinStyleExData);

			}

			logMsgln(nl);

			logMsg("winstyle R| " + $"{"> ",64}");
			ListWinStyleNames<WindowApiUtilities.WinStyle>();
			ListWinStyleResult(_WinStyleRecord);


			logMsg("winstyleRx| " + $"{"> ",64}");
			ListWinStyleNames<WindowApiUtilities.WinStyleEx>();
			ListWinStyleResult(_WinStyleExRecord);

		}

		internal static void ListMainWinInfo(IntPtr parent)
		{
			logMsgln("            main window| title| " + _doc.Title + "  (" + _doc.PathName + ")");
			logMsgln("                 intptr| " + parent.ToString());
			logMsgln("                extents| " + ListRect(NewRectangle(_uiapp.MainWindowExtents)));
			logMsgln(nl);

		}

		internal static void ListRevitUiViews()
		{
			// process revit views
			logMsgln("revit window rectangles| ");
			IList<UIView> views = _uidoc.GetOpenUIViews();
			Autodesk.Revit.DB.Rectangle r = null;

			foreach (UIView v in views)
			{
				Element e = _doc.GetElement(v.ViewId);

				logMsg("           view name   | ");
				logMsgln(e.Name);

				r = v.GetWindowRectangle();
				logMsg("           view extents| ");
				logMsgln(String.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom));
				logMsg(nl);
			}
		}

		internal static void ListChildWindowInfo()
		{
			if (ActWindows == null) { return; }

			logMsgln(nl);
			logMsgln("listing of found revit windows| active|");
			logMsg(nl);

			foreach (RevitWindow rw in ActWindows)
			{
				ListChildWinInfo(rw);
			}

			if (MinWindows == null) { return; }

			logMsgln(nl);
			logMsgln("listing of found revit windows| minimized|");

			if (MinWindows.Count == 0)
			{
				logMsgln("no minimized windows|");
				logMsgln(nl);
				return;
			}

			logMsg(nl);

			foreach (RevitWindow rw in MinWindows)
			{
				ListChildWinInfo(rw);
			}
		}

		internal static void ListChildWinInfo(RevitWindow rw)
		{
			logMsgln("       child handle| " + rw.handle);
			logMsgln(" child rect current| " + ListRect(rw.current));
			logMsgln("child rect proposed| " + ListRect(rw.proposed));
			logMsg(nl);
		}

		internal static string ListRect(WindowApiUtilities.RECT r)
		{
			return String.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom);
		}

		internal static string ListRect(Rectangle r)
		{
			return string.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom);
		}

	}
}
