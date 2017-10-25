#region Using directives
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Rectangle = System.Drawing.Rectangle;

using static Windows.Command;
using static Windows.WindowApiUtilities;
using static Windows.WindowListingUtilities;

#endregion

// itemname:	WindowUtilities
// username:	jeffs
// created:		10/23/2017 8:42:32 PM


namespace Windows
{
	class WindowUtilities
	{

		internal static Process GetRevit()
		{
			string mainWinTitle = _doc.Title.ToLower();

			Process[] revits = Process.GetProcessesByName("Revit");
			Process foundRevit = null;

			if (revits.Length > 0)
			{
				foreach (Process revit in revits)
				{
					if (revit.MainWindowTitle.ToLower().Contains(mainWinTitle))
					{
						foundRevit = revit;
						break;
					}
				}
			}
			else
			{
				foundRevit = revits[0];
			}

			return foundRevit;
		}

		internal static void GetSystemInfo(IntPtr parent, int titlebarheight)
		{
			logMsgln("   get win|    title bar height| " + titlebarheight);

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
			IntPtr hMonitor = MonitorFromWindow(parent, 0);

			WindowApiUtilities.MONITORINFOEX mi = new WindowApiUtilities.MONITORINFOEX();
			mi.Init();
			GetMonitorInfo(hMonitor, ref mi);

			logMsgln("monitor info|       device name| " + mi.DeviceName);
			logMsgln("monitor info|      monitor rect| " + ListRect(mi.rcMonitor));
			logMsgln("monitor info|    work area rect| " + ListRect(mi.rcWorkArea));
			logMsgln("monitor info|  primary monitor?| " +
				(mi.Flags == WindowApiUtilities.dwFlags.MONITORINFO_PRIMARY));

			ScreenLayout = mi.rcWorkArea.AsRectangle();

		}

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

		internal static IntPtr GetMainWinHandle(Process revitProcess)
		{
			IntPtr parent = revitProcess.MainWindowHandle;
			ListMainWinInfo(parent);
			return parent;
		}

		internal static List<IntPtr> GetChildWindows(
			IntPtr parent)
		{
			List<IntPtr> result = new List<IntPtr>();
			GCHandle listHandle = GCHandle.Alloc(result);
			try
			{
				WindowApiUtilities.EnumWindowProc childProc = new WindowApiUtilities.EnumWindowProc(EnumWindow);
				EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
			}
			finally
			{
				if (listHandle.IsAllocated)
					listHandle.Free();
			}
			return result;
		}

		internal static bool GetRevitChildWindows(IntPtr parent)
		{
			List<IntPtr> children = GetChildWindows(parent);

			if (children == null || children.Count == 0) { return false; }

			foreach (IntPtr child in children)
			{
				WindowApiUtilities.WINDOWINFO wi = new WindowApiUtilities.WINDOWINFO(true);
				GetWindowInfo(child, ref wi);

				if ((wi.dwExStyle & (uint) WindowApiUtilities.WinStyleEx.WS_EX_MDICHILD) == 0) { continue; }

				bool isMin = IsIconic(child);

				StringBuilder winTitle = new StringBuilder(255);
				GetWindowText(child, winTitle, 255);

				Rectangle rectCurr = ValidateWindow(NewRectangle(wi.rcWindow));

				RevitWindow rw = new RevitWindow
				{
					sequence = -1, // flag as not specified
					handle = child, // handle to the window
					docTitle = _doc.Title,
					winTitle = winTitle.ToString(),
					IsMinimized = isMin,
					current = rectCurr,
					proposed = Rectangle.Empty
				};

				if (isMin)
				{
					MinWindows.Add(rw);
				}
				else
				{
					ActWindows.Add(rw);
				}
			}
			return true;
		}

		internal static Rectangle ValidateWindow(Rectangle r)
		{

			if (r.Left < ParentClientWindow.Left)
			{
				r.X = ParentClientWindow.Left;
			}

			if (r.Top < ParentClientWindow.Top)
			{
				r.Y = ParentClientWindow.Top;

			}

			if (r.Right > ParentClientWindow.Right)
			{

				r = r.SetRight(ParentClientWindow.Right);
			}

			if (r.Bottom > ParentClientWindow.Bottom)
			{
				r = r.SetBottom(ParentClientWindow.Bottom);
			}
			return r;
		}

		internal static int GetTitleBarHeight(IntPtr window)
		{
			WindowApiUtilities.TITLEBARINFO ti = new WindowApiUtilities.TITLEBARINFO();
			ti.cbSize = (uint) Marshal.SizeOf(typeof(WindowApiUtilities.TITLEBARINFO));
			GetTitleBarInfo(window, ref ti);

			return ti.rcTitleBar.Bottom - ti.rcTitleBar.Top;
		}

		// adjusts the dimensions of the by the amount specified
		// adjustment is made thus: 
		// top & left += amount
		// bottom & right -= amount
		internal static Rectangle AdjustWindowRectangle(WindowApiUtilities.RECT rect, int amount)
		{
			rect.Top += amount;
			rect.Left += amount;
			rect.Bottom -= amount;
			rect.Right -= amount;

			return NewRectangle(rect);
		}

		internal static void SetupForm(IntPtr parent, Rectangle mainClientRect)
		{
			SetupFormMainClientRect(parent, mainClientRect);
			SetupFormChildCurr();
		}

		internal static void SetupFormMainClientRect(IntPtr parent, Rectangle mainClientRect)
		{
			_form.RevitMainWorkArea = mainClientRect;

			WindowApiUtilities.WINDOWINFO wip = new WindowApiUtilities.WINDOWINFO(true);
			GetWindowInfo(parent, ref wip);

			logMsgln("      win info win rect| " + ListRect(wip.rcWindow));
			logMsgln("   win info client rect| " + ListRect(wip.rcClient));

			_form.ParentRectForm = NewRectangle(wip.rcWindow);
			_form.ParentRectClient = NewRectangle(wip.rcClient);
		}

		internal static void SetupFormChildCurr()
		{
			int idx = 0;

			foreach (RevitWindow rw in ActWindows)
			{
				_form.SetChildCurr(idx++, rw.current, rw.winTitle);
			}

			foreach (RevitWindow rw in MinWindows)
			{
				_form.SetChildCurr(idx++, rw.current, rw.winTitle, true);
			}
		}

	}
}
