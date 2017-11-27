#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Rectangle = System.Drawing.Rectangle;

using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowListingUtilities;
using static RevitWindows.ProjectSelectForm;

#endregion

// itemname:	WindowUtilities
// username:	jeffs
// created:		10/23/2017 8:42:32 PM


namespace RevitWindows
{
	class WindowUtilities
	{
		internal const string APP_NAME = "Revit Windows";

		internal const int VIEW_TYPE_VOID = 199;
		

		// an AutoDesk rectangle from a system rectangle
		internal static Autodesk.Revit.DB.Rectangle ConvertTo(Rectangle r)
		{
			return new Autodesk.Revit.DB.Rectangle(r.Left, r.Top, r.Right, r.Bottom);
		}

		// system rectangle from an AutoDesk rectangle
		internal static Rectangle NewRectangle(Autodesk.Revit.DB.Rectangle r)
		{
			return NewRectangle(r.Left, r.Top, r.Right, r.Bottom);
		}

		// system rectangle from a RECT struct
		internal static Rectangle NewRectangle(RECT r)
		{
			return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
		}

		// system rectangle from two sets of coordinates
		internal static Rectangle NewRectangle(int left, int top, int right, int bottom)
		{
			int w = right - left;
			int h = bottom - top;

			return new Rectangle(left, top, w < 0 ? -1 * w : w, h < 0 ? -1 * h : h);
		}

		internal static void logMsg(string message)
		{
			Debug.Write(message);
		}

		internal static void logMsgln(string message)
		{
			logMsg(message + nl);
		}

		internal static void SetupForm(IntPtr parent, Rectangle mainClientRect)
		{
			SetupFormMainClientRect(parent, mainClientRect);
			SetupFormChildCurr();
		}

		internal static void SetupFormMainClientRect(IntPtr parent, Rectangle mainClientRect)
		{
			Command.MForm.RevitMainWorkArea = mainClientRect;

			WINDOWINFO wip = new WINDOWINFO(true);
			GetWindowInfo(parent, ref wip);

			logMsgln("      win info win rect| " + ListRect(wip.rcWindow));
			logMsgln("   win info client rect| " + ListRect(wip.rcClient));

			Command.MForm.ParentRectForm = NewRectangle(wip.rcWindow);
			Command.MForm.ParentRectClient = NewRectangle(wip.rcClient);
		}

		internal static void SetupFormChildCurr()
		{
			int idx = 0;

			foreach (RevitWindow rw in RevitWindow.ChildWindows)
			{
				Command.MForm.SetChildCurr(idx++, rw.Current, rw.WindowTitle);
			}

//			foreach (RevitWindow rw in RevitWindow.ChildWinMinimized)
//			{
//				Command.MForm.SetChildCurr(idx++, rw.Current, rw.WindowTitle, true);
//			}
//
//			foreach (RevitWindow rw in RevitWindow.ChildWinOther)
//			{
//				Command.MForm.SetChildCurr(idx++, rw.Current, rw.WindowTitle);
//			}


		}

		internal static Rectangle ParentWindow { get; private set; }
		internal static Rectangle DisplayScreenRect { get; private set; }
		internal static int MinWindowHeight { get; private set; }
		internal static int MinWindowWidth { get; private set; }
		internal static int TitleBarHeight { get; private set; }

		internal static void SortChildWindows()
		{
//			sortChildWindows(RevitWindow.ChildWinMinimized);
			SortChildWindows(RevitWindow.ChildWindows);
//			sortChildWindows(RevitWindow.ChildWinOther);
		}

		internal static void SortChildWindows(List<RevitWindow> w)
		{
			if (w.Count == 0 || w.Count == 1) { return; }
			w.Sort((a, b) => a.Sequence.CompareTo(b.Sequence));
		}

		private static int GetTitleBarHeight(IntPtr parent)
		{
			double dpi = GetDpiForWindow(parent) / 96.0f;
			int captionHeight = GetSystemMetrics(SystemMetric.SM_CYCAPTION);
			int frameHeight = GetSystemMetrics(SystemMetric.SM_CYFRAME);
			int addedBorderHeight = GetSystemMetrics(SystemMetric.SM_CXPADDEDBORDER);
			//
			//			TITLEBARINFO ti = new TITLEBARINFO();
			//			ti.cbSize = (uint) Marshal.SizeOf(typeof(TITLEBARINFO));
			//			GetTitleBarInfo(parent, ref ti);
			//			return ti.rcTitleBar.Bottom - ti.rcTitleBar.Top;
			return ((int) ((captionHeight + frameHeight) * dpi)) + addedBorderHeight;
		}

		internal static void GetScreenMetrics(IntPtr parent)
		{
			// determine the main client rectangle - the repositioned
			// view window go here
			ParentWindow = NewRectangle(Uiapp.DrawingAreaExtents).Adjust(-2);
			TitleBarHeight = GetTitleBarHeight(parent);
			DisplayScreenRect = GetScreenRectFromWindow(parent);

			MinWindowHeight = GetSystemMetrics(SystemMetric.SM_CYMIN);
			MinWindowWidth = GetSystemMetrics(SystemMetric.SM_CXMIN);
		}

		internal static bool GetRevitChildWindows(IntPtr parent)
		{
			RevitWindow.ResetRevitWindows();

			List<IntPtr> children = GetChildWindows(parent);
			IList<View> views = GetRevitChildViews(Uidoc);

			bool activeSet = false;

			if (children == null || children.Count == 0) { return false; }

			string currDoc = Doc.Title.ToLower();

			foreach (IntPtr child in children)
			{
				WINDOWINFO wi = new WINDOWINFO(true);
				GetWindowInfo(child, ref wi);

				if ((wi.dwExStyle & (uint) WinStyleEx.WS_EX_MDICHILD) == 0) { continue; }

				// got a good window - store it for later
				// create the revit window data
				RevitWindow rw = new RevitWindow(child, NewRectangle(wi.rcWindow), currDoc);

				RevitWindow.ChildWindows.Add(rw);

				// no maximized windows allowed
				if (IsZoomed(child))
				{
					ShowWindow(child, ShowWinCmds.SW_RESTORE);
				}
			}

			return true;
		}

		internal static string GetWindowTitle(IntPtr child)
		{
			StringBuilder winTitle = new StringBuilder(255);
			GetWindowText(child, winTitle, 255);

			return winTitle.ToString();
		}

		internal static IList<UIView> GetRevitChildUiViews(UIDocument uidoc)
		{
			return uidoc.GetOpenUIViews();
		}

		internal static IList<View> GetRevitChildViews(UIDocument uidoc)
		{
			IList<UIView> uiViews = GetRevitChildUiViews(uidoc);

			IList<View> views = new List<View>(uiViews.Count);

			foreach (UIView u in uiViews)
			{
				views.Add((View) Doc.GetElement(u.ViewId));
			}
			return views;
		}

		internal static View FindRevitView(IList<View> views, string winTitle)
		{
			string test = winTitle.ToLower();

			foreach (View v in views)
			{
				if (test.Contains(v.Title.ToLower()))
				{
					return v;
				}
			}
			return null;
		}

		internal static int GetRevitViewType(IList<View> views, string winTitle)
		{
			View v = FindRevitView(views, winTitle);

			if (v == null) return VIEW_TYPE_VOID;

			int viewType = (int) v.ViewType > VIEW_TYPE_VOID ? VIEW_TYPE_VOID : (int) v.ViewType;

			return viewType;
		}
//
//		internal static Dictionary<int, string> ViewTypeOrder;
//
//		internal static int ViewTypeIndexOf(string viewTitle, out string ViewTypeName)
//		{
//			foreach (KeyValuePair<int, string> viewType in ViewTypeOrder)
//			{
//				if (viewTitle.ToLower().StartsWith(viewType.Value, StringComparison.Ordinal))
//				{
//					ViewTypeName = viewType.Value;
//					return viewType.Key;
//				}
//			}
//
//			ViewTypeName = "unknown";
//			return 99;	// place at the end of the list
//		}
//
//		internal static string ViewTypeNameByIdx(int idx)
//		{
//			string viewTitle;
//
//			bool result = ViewTypeOrder.TryGetValue(idx, out viewTitle);
//
//			if (!result) return null;
//
//			return viewTitle;
//		}
//
//		internal static void InitViewTypeOrderList()
//		{
//			ViewTypeOrder = new Dictionary<int, string>(25);
//			int idx = 0;
//			ViewTypeOrder.Add(99,	 "unknown");
//			ViewTypeOrder.Add(idx++, "sheet");						//* sheet view
//			ViewTypeOrder.Add(idx++, "floor plan");					//* Floor plan type of view. 
//			ViewTypeOrder.Add(idx++, "reflected ceiling plan");		//* reflected ceiling plan type of view.
//			ViewTypeOrder.Add(idx++, "area plan");					//* an area plan type of view. 
//			ViewTypeOrder.Add(idx++, "structural plan");			//* a structural plan or engineering plan type of view
//			ViewTypeOrder.Add(idx++, "elevation");					//* elevation type of view.
//			ViewTypeOrder.Add(idx++, "section");					//* a cross section type of view. 
//			ViewTypeOrder.Add(idx++, "drafting view");				//* drafting type of view.
//			ViewTypeOrder.Add(idx++, "detail view");				//* detail type of view.
//			ViewTypeOrder.Add(idx++, "3d view");					//* 3d type of view.
//			ViewTypeOrder.Add(idx++, "walkthrough");				//walk-through type of 3d view.
//			ViewTypeOrder.Add(idx++, "legend");						//a legend type of view.
//			ViewTypeOrder.Add(idx++, "schedule");					//* schedule type of view.
//			ViewTypeOrder.Add(idx++, "graphical column schedule");	//* column schedule type of view. 
//			ViewTypeOrder.Add(idx++, "rendering");					//rendering type of view.
//			ViewTypeOrder.Add(idx++, "report");						//report type of view.
//			ViewTypeOrder.Add(idx++, "cost report");				//cost report view. 
//			ViewTypeOrder.Add(idx++, "loads report");				//loads report view. 
//			ViewTypeOrder.Add(idx++, "presure loss report");		//pressure loss report view.
//			ViewTypeOrder.Add(idx++, "panel schedule");				//* panel schedule report view.
//			ViewTypeOrder.Add(idx++, "project browser");			//the project browser view.
//			ViewTypeOrder.Add(idx++, "system browser");				//the mep system browser view.
//			ViewTypeOrder.Add(idx++, "undefined");					//an undefined/ unspecified type of view.
//			ViewTypeOrder.Add(idx++, "internal");					//revit's internal type of view
//
//		}


	}
}
