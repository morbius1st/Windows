#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using UtilityLibrary;
using Rectangle = System.Drawing.Rectangle;

using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.RevitWindow;
using static RevitWindows.Command;
using static RevitWindows.ProjectSelectForm;


#endregion

// itemname:	WindowListingUtilities
// username:	jeffs
// created:		10/23/2017 8:50:38 PM


namespace RevitWindows
{
	class WindowListingUtilities
	{
		internal const string nl = "\r\n";
		internal const string pattRect = "x1|{0,5:D} y1|{1,5:D} x2|{2,5:D} y2|{3,5:D}";

		static internal void ListDocuments()
		{
			logMsg(_formProjSel.ToString());
		}


		static internal void ListSystemInformation(IntPtr parent, int titleBarHeight)
		{
			logMsgln("   get win|    title bar height| " + titleBarHeight);

			logMsgln("sys metric| caption area height| " + GetSystemMetrics(SystemMetric.SM_CYCAPTION));
			logMsgln("sys metric|   sm caption height| " + GetSystemMetrics(SystemMetric.SM_CYSMCAPTION));
			logMsgln("sys metric|        border width| " + GetSystemMetrics(SystemMetric.SM_CXBORDER));
			logMsgln("sys metric|       border height| " + GetSystemMetrics(SystemMetric.SM_CYBORDER));
			logMsgln("sys metric|  fixed frame| horiz| " + GetSystemMetrics(SystemMetric.SM_CXFIXEDFRAME));
			logMsgln("sys metric|  fixed frame|  vert| " + GetSystemMetrics(SystemMetric.SM_CYFIXEDFRAME));
			logMsgln("sys metric|   size frame| horiz| " + GetSystemMetrics(SystemMetric.SM_CXSIZEFRAME));
			logMsgln("sys metric|   size frame|  vert| " + GetSystemMetrics(SystemMetric.SM_CYSIZEFRAME));
			logMsgln("sys metric|  window min| height| " + GetSystemMetrics(SystemMetric.SM_CYMIN));
			logMsgln("sys metric|  window min|  width| " + GetSystemMetrics(SystemMetric.SM_CXMIN));

			// this works for getting the correct monitor and the
			// correct monitor location and size
			MONITORINFOEX mi = GetMonitorInfo(parent);

			logMsgln("monitor info|       device name| " + mi.DeviceName);
			logMsgln("monitor info|      monitor rect| " + ListRect(mi.rcMonitor));
			logMsgln("monitor info|    work area rect| " + ListRect(mi.rcWorkArea));
			logMsgln("monitor info|  primary monitor?| " +
				(mi.Flags == dwFlags.MONITORINFO_PRIMARY));

			logMsgln(nl);
			logMsgln("monitor DPI| " + GetDpiForWindow(parent));
		}

		internal static void ListAllChildWindows(IntPtr parent)
		{
			uint copy = 0;
			string result = "";

			List<IntPtr> children = GetChildWindows(parent);

			if (children == null || children.Count == 0) { return; }

			foreach (IntPtr child in children)
			{
				_WinStyleData = new int[_WinStyleCount];
				_WinStyleExData = new int[_WinStyleExCount];

				WINDOWINFO wi = new WINDOWINFO(true);
				GetWindowInfo(child, ref wi);

				WINDOWPLACEMENT wp = WINDOWPLACEMENT.Default;
				GetWindowPlacement(child, ref wp);

				StringBuilder className = new StringBuilder(40);
				GetClass(child, className, 40);

				StringBuilder winText = new StringBuilder(40);
				GetWindowText(child, winText, 40);

				AnalyzeWinStyles<WinStyle>(wi.dwStyle, ref _WinStyleRecord,
					ref _WinStyleData, ref copy, ref result);
				AnalyzeWinStyles<WinStyleEx>(wi.dwExStyle, ref _WinStyleExRecord,
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
			ListWinStyleNames<WinStyle>();
			ListWinStyleResult(_WinStyleRecord);


			logMsg("winstyleRx| " + $"{"> ",64}");
			ListWinStyleNames<WinStyleEx>();
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
			IList<UIView> views = GetRevitChildUiViews(_uidoc);

			Autodesk.Revit.DB.Rectangle r = null;

			foreach (UIView v in views)
			{
//				Element e = Doc.GetElement(v.ViewId);
				View e = (View) _doc.GetElement(v.ViewId);

				logMsgln("              view name| " + e.Name);
				logMsg(  "           view extents| " + ListRect(v.GetWindowRectangle()));
				logMsgln("              view type| " + e.ViewType 
					+ "  name| " + Enum.GetName(typeof(ViewType), e.ViewType));
				logMsg(nl);

				v.Dispose();
			}
		}

		internal static void ListChildWin(List<RevitWindow> rws, string title, 
			params int[] whichLst)
		{
			int selectedWinCount = 0;
			int normalWinCount = 0;
			int minWinCount = 0;

//			MessageUtilities.clearConsole();

			int change = 0;
			int count = 0;

			logMsgln(title);
			
			logMsg(nl);
			logMsgln("minimized windows");

			foreach (RevitWindow rw in rws)
			{
				if (change == 0 && rw.IsActive)
				{
					if (count == 0)
					{
						logMsgln("    none");
					}
					count = 0;

					logMsgln("active window");
					change = 1;
				}
				else if (change == 1 && rw.IsSelected)
				{
					if (count == 0)
					{
						logMsgln("    none");
					}
					count = 0;

					logMsgln("selected windows");
					change = 2;
				}
				else if (change == 2 && rw.IsNonSelected)
				{
					if (count == 0)
					{
						logMsgln("    none");
					}
					count = 0;

					logMsgln("non-selected windows");
					change = 3;
				}

				if (rw.IsActive || rw.IsSelected)
				{
					selectedWinCount++;
				}
				else if (rw.IsNonSelected)
				{
					normalWinCount++;
				}
				else if (rw.IsMinimized)
				{
					minWinCount++;
				}

				count++;
				foreach (int which in whichLst)
				{
					switch (which)
					{
						case 1:
							ListChildTitle(rw);
							break;
						case 2:
							ListChildHandle(rw);
							break;
						case 3:
							ListChildSeq(rw);
							break;
						case 4:
							ListChildWinStatus(rw);
							break;
						case 5:
							ListChildViewType(rw);
							break;
						case 6:
							ListChildIsMin(rw);
							break;
						case 7:
							ListChildIsActive(rw);
							break;
						case 8:
							ListChildCurrRect(rw);
							break;
						case 9:
							ListChildPropRect(rw);
							break;
						case 10:
							ListChildIsSelected(rw);
							break;
					}
				}
				logMsg(nl);
			}
			ListChildCounts();
			logMsg(nl);
			logMsgln("      calc'd values| ");
			logMsgln(" selected win count| " + selectedWinCount);
			logMsgln("non-sel'd win count| " + normalWinCount);
			logMsgln("minimized win count| " + minWinCount);
		}

		internal static void ListChildCounts()
		{
			logMsgln(" selected win count| " + SelectedWinCount);
			logMsgln("non-sel'd win count| " + NonSelWinCount);
			logMsgln("minimized win count| " + MinimizedWinCount);
		}

		// 1
		internal static void ListChildTitle(RevitWindow rw)
		{
			logMsgln("        child title| " + rw.WindowTitle);
		}
		// 2
		internal static void ListChildHandle(RevitWindow rw)
		{
			logMsgln("       child handle| " + rw.Handle);
		}
		// 3
		internal static void ListChildSeq(RevitWindow rw)
		{
			logMsgln("     child sequence| " + rw.Sequence);
		}
		// 4
		internal static void ListChildWinStatus(RevitWindow rw)
		{
			logMsgln("      window status| " + Enum.GetName(typeof(WindowStatus), rw.WinStatus));
		}
		// 5
		internal static void ListChildViewType(RevitWindow rw)
		{
			logMsgln("     child ViewType| >" + rw.ViewType 
				+ "<  viewtype value| " + (int) rw.ViewType);
		}
		// 6
		internal static void ListChildIsMin(RevitWindow rw)
		{
			logMsgln("       is minimized| " + rw.IsMinimized);
		}
		// 7
		internal static void ListChildIsActive(RevitWindow rw)
		{
			logMsgln("          is active| " + rw.IsActive);
		}
		// 8
		internal static void ListChildCurrRect(RevitWindow rw)
		{
			logMsgln("       rect current| " + ListRect(rw.Current));
		}
		// 9
		internal static void ListChildPropRect(RevitWindow rw)
		{
			logMsgln("      rect proposed| " + ListRect(rw.Proposed));
		}
		// 10
		internal static void ListChildIsSelected(RevitWindow rw)
		{
			logMsgln("        is selected| " + rw.IsSelected);
		}
		//
		//		internal static void ListChildDocIndex(RevitWindow rw)
		//		{
		//			logMsgln("   doc index & name| " + rw.DocIndex + " :: " + rw.DocTitle);
		//		}

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
