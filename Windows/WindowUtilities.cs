#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Rectangle = System.Drawing.Rectangle;

using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowListingUtilities;

#endregion

// itemname:	WindowUtilities
// username:	jeffs
// created:		10/23/2017 8:42:32 PM


namespace RevitWindows
{
	class WindowUtilities
	{
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
				Command.MForm.SetChildCurr(idx++, rw.current, rw.WindowTitle);
			}

			foreach (RevitWindow rw in RevitWindow.ChildWinMinimized)
			{
				Command.MForm.SetChildCurr(idx++, rw.current, rw.WindowTitle, true);
			}

			foreach (RevitWindow rw in RevitWindow.ChildWinOther)
			{
				Command.MForm.SetChildCurr(idx++, rw.current, rw.WindowTitle);
			}


		}

		internal static Rectangle ParentWindow { get; private set; }
		internal static Rectangle DisplayScreenRect { get; private set; }
		internal static int MinWindowHeight { get; private set; }
		internal static int MinWindowWidth { get; private set; }
		internal static int TitleBarHeight { get; private set; }

		internal static void SortChildWindows()
		{
			sortChildWindows(RevitWindow.ChildWinMinimized);
			sortChildWindows(RevitWindow.ChildWindows);
			sortChildWindows(RevitWindow.ChildWinOther);
		}

		internal static void sortChildWindows(List<RevitWindow> w)
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
			ParentWindow = NewRectangle(Command.UiApp.DrawingAreaExtents).Adjust(-2);
			TitleBarHeight = GetTitleBarHeight(parent);
			DisplayScreenRect = GetScreenRectFromWindow(parent);

			MinWindowHeight = GetSystemMetrics(SystemMetric.SM_CYMIN);
			MinWindowWidth = GetSystemMetrics(SystemMetric.SM_CXMIN);
		}

		internal static bool GetRevitChildWindows(IntPtr parent)
		{
			List<IntPtr> children = GetChildWindows(parent);
			IList<View> views = GetRevitChildViews(Command.UiDoc);

			bool activeSet = false;

			if (children == null || children.Count == 0) { return false; }

			foreach (IntPtr child in children)
			{
				WINDOWINFO wi = new WINDOWINFO(true);
				GetWindowInfo(child, ref wi);

				if ((wi.dwExStyle & (uint) WinStyleEx.WS_EX_MDICHILD) == 0) { continue; }

				// got a good window - store it for later

				string winTitle = GetWindowTitle(child);

				View v = FindRevitView(views, winTitle);

				// create the revit window data
				RevitWindow rw = new RevitWindow(child, v, winTitle);

				// rw.current = ValidateWindow(NewRectangle(wi.rcWindow));
				rw.current = NewRectangle(wi.rcWindow);

				if (v == null)
				{
					RevitWindow.ChildWinOther.Add(rw);
				}
				else if (rw.IsMinimized)
				{
					RevitWindow.ChildWinMinimized.Add(rw);
				}
				else
				{
					// save the active window for later
					if (RevitWindow.ActiveWindow == IntPtr.Zero)
					{
						rw.MakeActive();
//						RevitWindow.ActiveWindow = child;
					}

					RevitWindow.ChildWindows.Add(rw);
				}
			}

			return true;
		}

		internal static string GetWindowTitle(IntPtr child)
		{
			StringBuilder winTitle = new StringBuilder(255);
			GetWindowText(child, winTitle, 255);

			return winTitle.ToString().ToLower();
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
				views.Add((View) Command.Doc.GetElement(u.ViewId));
			}
			return views;
		}

		internal static View FindRevitView(IList<View> views, string WinTitle)
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
