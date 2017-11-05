using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Rectangle = System.Drawing.Rectangle;

namespace RevitWindows
{
	internal class RevitWindow
	{
		enum WindowState : short
		{
			MINIMIZED = -1,
			NORMAL = 0,
			MAXIMIZED = 1
		}

		internal static List<RevitWindow> ChildWindows;
		internal static List<RevitWindow> ChildWinMinimized;
		internal static List<RevitWindow> ChildWinOther;

		internal static IntPtr ActiveWindow { get; private set; } = IntPtr.Zero;

		private int sequence;
		private IntPtr handle;
		private string docTitle;
		private string windowTitle;
		private ViewType viewType;
		private WindowState state;
		internal Rectangle current;
		internal Rectangle proposed;

		private int _nonActiveWinAdj = 100;

		internal RevitWindow(IntPtr intPtr, View v, string winTitle)
		{
			if (v == null)
			{
				// select project browser because this can never happen
				viewType = ViewType.ProjectBrowser;
			}
			else
			{
				viewType = v.ViewType;
			}

			handle = intPtr;
			sequence = (int) viewType + _nonActiveWinAdj;
			docTitle = Command.Doc.Title;
			windowTitle = winTitle;
			state = WindowState.NORMAL;
			current = Rectangle.Empty;
			proposed = Rectangle.Empty;


			if (WindowApiUtilities.IsIconic(intPtr))
			{
				state = WindowState.MINIMIZED;
			}
			else if (WindowApiUtilities.IsZoomed(intPtr))
			{
				state = WindowState.MAXIMIZED;
			}
		}

		private RevitWindow() { }

		internal int Sequence => sequence;
		internal string WindowTitle => windowTitle;
		internal string DocTitle => docTitle;
		internal ViewType ViewType => viewType;
		internal IntPtr Handle => handle;

		internal bool IsMaximized => state == WindowState.MAXIMIZED;
		internal bool IsMinimized => state == WindowState.MINIMIZED;
		internal bool IsNormal => state == WindowState.NORMAL;
		internal bool IsActiveWindow => sequence > _nonActiveWinAdj;

		internal RevitWindow Clone()
		{
			RevitWindow rwn = new RevitWindow();
			rwn.sequence = this.sequence;
			rwn.handle = this.handle;
			rwn.docTitle = this.docTitle;
			rwn.windowTitle = this.windowTitle;
			rwn.viewType = this.viewType;
			rwn.state = this.state;
			rwn.current = this.current;
			rwn.proposed = this.proposed;

			return rwn;
		}

		internal static void ResetActiveWindow()
		{
			ActiveWindow = IntPtr.Zero;
		}

		internal bool MakeActive()
		{
			if (ActiveWindow != IntPtr.Zero) { return false; }

			ActiveWindow = this.handle;
			this.sequence -= _nonActiveWinAdj;

			return true;
		}

	}
}