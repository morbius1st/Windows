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
using static Windows.RevitWindow;



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

		static internal void ListSystemInformation(IntPtr parent, int titleBarHeight)
		{
			logMsgln("   get win|    title bar height| " + titleBarHeight);

			logMsgln("sys metric| caption area height| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CYCAPTION));
			logMsgln("sys metric|   sm caption height| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CYSMCAPTION));
			logMsgln("sys metric|        border width| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CXBORDER));
			logMsgln("sys metric|       border height| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CYBORDER));
			logMsgln("sys metric|  fixed frame| horiz| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CXFIXEDFRAME));
			logMsgln("sys metric|  fixed frame|  vert| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CYFIXEDFRAME));
			logMsgln("sys metric|   size frame| horiz| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CXSIZEFRAME));
			logMsgln("sys metric|   size frame|  vert| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CYSIZEFRAME));
			logMsgln("sys metric|  window min| height| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CYMIN));
			logMsgln("sys metric|  window min|  width| " + GetSystemMetrics(WindowApiUtilities.SystemMetric.SM_CXMIN));

			// this works for getting the correct monitor and the
			// correct monitor location and size
			MONITORINFOEX mi = GetMonitorInfo(parent);

			logMsgln("monitor info|       device name| " + mi.DeviceName);
			logMsgln("monitor info|      monitor rect| " + ListRect(mi.rcMonitor));
			logMsgln("monitor info|    work area rect| " + ListRect(mi.rcWorkArea));
			logMsgln("monitor info|  primary monitor?| " +
				(mi.Flags == WindowApiUtilities.dwFlags.MONITORINFO_PRIMARY));

			logMsgln(nl);
			logMsgln("monitor DPI| " + GetDpiForWindow(parent));
		}

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
			logMsgln("            main window| title| " + Doc.Title + "  (" + Doc.PathName + ")");
			logMsgln("                 intptr| " + parent.ToString());
			logMsgln("                extents| " + ListRect(NewRectangle(UiApp.MainWindowExtents)));
			logMsgln(nl);

		}

		internal static void ListRevitUiViews()
		{
			// process revit views
			logMsgln("revit window rectangles| ");
			IList<UIView> views = GetRevitChildUiViews(UiDoc);

			Autodesk.Revit.DB.Rectangle r = null;

			foreach (UIView v in views)
			{
//				Element e = Doc.GetElement(v.ViewId);
				View e = (View) Doc.GetElement(v.ViewId);

				logMsgln("              view name| " + e.Name);
				logMsg(  "           view extents| " + ListRect(v.GetWindowRectangle()));
				logMsgln("              view type| " + e.ViewType 
					+ "  name| " + Enum.GetName(typeof(ViewType), e.ViewType));

				





				logMsg(nl);

				v.Dispose();
			}
		}

		internal static void ListChildWindowInfo()
		{
			if (ChildWindows == null) { return; }

			logMsgln(nl);
			logMsgln("listing of found revit windows| active|");
			logMsg(nl);

			logMsgln("normal windows");

			foreach (RevitWindow rw in ChildWindows)
			{
				ListChildWinInfo2(rw);
			}

			logMsgln("minimized windows");
			foreach (RevitWindow rw in ChildWinMinimized)
			{
				ListChildWinInfo2(rw);
			}

			logMsgln(nl + "active window| " + ActiveWindow);
		}

		internal static void ListChildWinInfo(RevitWindow rw)
		{
			logMsgln("       child handle| " + rw.Handle);
			logMsgln("     child sequence| " + rw.Sequence);
			logMsgln("     child ViewType| " + Enum.GetName(typeof(ViewType), rw.ViewType) 
				+ "  viewtype value| " + (int) rw.ViewType);
			logMsgln(" child rect current| " + ListRect(rw.current));
			logMsgln("child rect proposed| " + ListRect(rw.proposed));
			logMsgln("child  is minimized| " + rw.IsMinimized);
			logMsgln("child  is maximized| " + rw.IsMaximized);
			logMsg(nl);
		}

		internal static void ListChildWinInfo2(RevitWindow rw)
		{
			logMsgln("        child title| " + rw.WindowTitle);
			logMsgln("       child handle| " + rw.Handle);
			logMsgln("     child sequence| " + rw.Sequence);
			logMsgln("     child ViewType| " + Enum.GetName(typeof(ViewType), rw.ViewType)
				+ "  viewtype value| " + (int) rw.ViewType);
			logMsgln("child  is minimized| " + IsIconic(rw.Handle));
			logMsgln("child  is maximized| " + IsZoomed(rw.Handle));
			logMsg(nl);
		}

		internal static string ListRect(RECT r)
		{
			return String.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom);
		}

		internal static string ListRect(Rectangle r)
		{
			return string.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom);
		}

		internal static string ListRect(Autodesk.Revit.DB.Rectangle r)
		{
			return string.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom);
		}

	}
}
