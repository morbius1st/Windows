#region Using directives
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Rectangle = System.Drawing.Rectangle;

using static Windows.Command;
using static Windows.WindowApiUtilities;
using static Windows.WindowListingUtilities;
using static Windows.RevitWindow;

#endregion

// itemname:	WindowUtilities
// username:	jeffs
// created:		10/23/2017 8:42:32 PM


namespace Windows
{
	class WindowUtilities
	{

		private static Rectangle _parentClientWindow;
		private static Rectangle _displayRect;
		private static int _titleBarHeight;
		private static int _minWindowWidth;
		private static int _minWindowHeight;


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
		internal static Rectangle NewRectangle(WindowApiUtilities.RECT r)
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
			MForm.RevitMainWorkArea = mainClientRect;

			WINDOWINFO wip = new WINDOWINFO(true);
			GetWindowInfo(parent, ref wip);

			logMsgln("      win info win rect| " + ListRect(wip.rcWindow));
			logMsgln("   win info client rect| " + ListRect(wip.rcClient));

			MForm.ParentRectForm = NewRectangle(wip.rcWindow);
			MForm.ParentRectClient = NewRectangle(wip.rcClient);
		}

		internal static void SetupFormChildCurr()
		{
			int idx = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				MForm.SetChildCurr(idx++, rw.current, rw.WindowTitle);
			}

			foreach (RevitWindow rw in ChildWinMinimized)
			{
				MForm.SetChildCurr(idx++, rw.current, rw.WindowTitle, true);
			}

			foreach (RevitWindow rw in ChildWinOther)
			{
				MForm.SetChildCurr(idx++, rw.current, rw.WindowTitle);
			}


		}

		internal static int GetTitleBarHeight(IntPtr parent)
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
			_parentClientWindow = NewRectangle(UiApp.DrawingAreaExtents).Adjust(-2);
			_titleBarHeight = GetTitleBarHeight(parent);
			_displayRect = GetScreenRectFromWindow(parent);

			_minWindowHeight = GetSystemMetrics(SystemMetric.SM_CYMIN);
			_minWindowWidth = GetSystemMetrics(SystemMetric.SM_CXMIN);

		}

		internal static Rectangle ParentClientWindow => _parentClientWindow;
		internal static Rectangle DisplayRect => _displayRect;
		internal static int TitleBarHeight => _titleBarHeight;
		internal static int MinWindowHeight => _minWindowHeight;
		internal static int MinWindowWidth => _minWindowWidth;



		internal static bool GetRevitChildWindows(IntPtr parent)
		{
			List<IntPtr> children = GetChildWindows(parent);
			IList<View> views = GetRevitChildViews(UiDoc);

			bool activeSet = false;

			if (children == null || children.Count == 0) { return false; }

			foreach (IntPtr child in children)
			{
				WINDOWINFO wi = new WINDOWINFO(true);
				GetWindowInfo(child, ref wi);

				if ((wi.dwExStyle & (uint) WinStyleEx.WS_EX_MDICHILD) == 0) { continue; }

				StringBuilder winTitle = new StringBuilder(255);
				GetWindowText(child, winTitle, 255);

				string wTitle = winTitle.ToString().ToLower();

				View v = FindView(views, wTitle);

				if (v == null) { continue; }

				// got a good window - store it for later

				// create the revit window data
				RevitWindow rw = new RevitWindow(child, v);

//				rw.current = ValidateWindow(NewRectangle(wi.rcWindow));

				if (rw.IsMinimized)
				{
					ChildWinMinimized.Add(rw);
				}
				else
				{
					// save the active window for later
					if (ActiveWindow == IntPtr.Zero) { ActiveWindow = child;}

					ChildWindows.Add(rw);
				}
			}

			return true;
		}

		internal static void SortChildWindows()
		{
			sortChildWindows(ChildWinMinimized);
			sortChildWindows(ChildWindows);
			sortChildWindows(ChildWinOther);
		}

		internal static void sortChildWindows(List<RevitWindow> w)
		{
			if (w.Count == 0 || w.Count == 1) { return; }
			w.Sort((a, b) => a.Sequence.CompareTo(b.Sequence));
		}

//		// adjust the current rectangle to be with in the bounds of the 
//		// parent window
//		internal static Rectangle ValidateWindow(Rectangle r)
//		{
//
//			if (r.Left < ParentClientWindow.Left)
//			{
//				r.X = ParentClientWindow.Left;
//			}
//
//			if (r.Top < ParentClientWindow.Top)
//			{
//				r.Y = ParentClientWindow.Top;
//
//			}
//
//			if (r.Right > ParentClientWindow.Right)
//			{
//				r = r.SetRight(ParentClientWindow.Right);
//			}
//
//			if (r.Bottom > ParentClientWindow.Bottom)
//			{
//				r = r.SetBottom(ParentClientWindow.Bottom);
//			}
//			return r;
//		}

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

		internal static View FindView(IList<View> views, string WinTitle)
		{
			string test = WinTitle.ToLower();

			foreach (View v in views)
			{
				if (test.Contains(v.Title.ToLower()))
				{
					return v;
				}
			}
			return null;
		}


		internal static Dictionary<ViewType, int> ViewTypeOrder = 
			new Dictionary<ViewType, int>();

		internal int IndexOf(ViewType vt)
		{
			int order;
			return ViewTypeOrder.TryGetValue(vt, out order) ? order : -1;
		}

		internal static void InitViewTypeOrderList()
		{
			int idx = 0;

			ViewTypeOrder.Add(ViewType.FloorPlan, idx++);			//Floor plan type of view. 
			ViewTypeOrder.Add(ViewType.EngineeringPlan, idx++);		//a Structural plan or Engineering plan type of view.
			ViewTypeOrder.Add(ViewType.CeilingPlan, idx++);			//Reflected ceiling plan type of view.
			ViewTypeOrder.Add(ViewType.Elevation, idx++);			//Elevation type of view.
			ViewTypeOrder.Add(ViewType.Section, idx++);				//a Cross section type of view. 
			ViewTypeOrder.Add(ViewType.DraftingView, idx++);		//Drafting type of view.
			ViewTypeOrder.Add(ViewType.Detail, idx++);				//Detail type of view.
			ViewTypeOrder.Add(ViewType.ThreeD, idx++);				//3D type of view.
			ViewTypeOrder.Add(ViewType.Walkthrough, idx++);			//Walk-Through type of 3D view.
			ViewTypeOrder.Add(ViewType.AreaPlan, idx++);			//an Area plan type of view. 
			ViewTypeOrder.Add(ViewType.Legend, idx++);				//a Legend type of view.
			ViewTypeOrder.Add(ViewType.Schedule, idx++);			//a Schedule type of view.
			ViewTypeOrder.Add(ViewType.ColumnSchedule, idx++);		//Column Schedule type of view. 
			ViewTypeOrder.Add(ViewType.Rendering, idx++);			//Rendering type of view.
			ViewTypeOrder.Add(ViewType.Report, idx++);				//Report type of view.
			ViewTypeOrder.Add(ViewType.CostReport, idx++);			//Cost Report view. 
			ViewTypeOrder.Add(ViewType.LoadsReport, idx++);			//Loads Report view. 
			ViewTypeOrder.Add(ViewType.PresureLossReport, idx++);	//Pressure Loss Report view.
			ViewTypeOrder.Add(ViewType.DrawingSheet, idx++);		//Drawing sheet type of view. 
			ViewTypeOrder.Add(ViewType.ProjectBrowser, idx++);		//The project browser view.
			ViewTypeOrder.Add(ViewType.SystemBrowser, idx++);		//The MEP system browser view. 
			ViewTypeOrder.Add(ViewType.PanelSchedule, idx++);		//Panel Schedule Report view.
			ViewTypeOrder.Add(ViewType.Undefined, idx++);			//an Undefined/ unspecified type of view.
			ViewTypeOrder.Add(ViewType.Internal, idx++);			//Revit's internal type of view
		}


	}
}
