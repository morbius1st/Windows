using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Rectangle = System.Drawing.Rectangle;

using static Windows.WindowApiUtilities;
using static Windows.Command;

namespace Windows
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

		internal static IntPtr ActiveWindow = IntPtr.Zero;

		private int sequence;
		private IntPtr handle;
		private string docTitle;
		private string windowTitle;
		private ViewType viewType;
		private WindowState state;
		internal Rectangle current;
		internal Rectangle proposed;

		internal RevitWindow(IntPtr intPtr, View v)
		{
			handle = intPtr;
			viewType = v.ViewType;
			sequence = (int) viewType;
			docTitle = Doc.Title;
			windowTitle = v.Title;
			state = WindowState.NORMAL;
			current = Rectangle.Empty;
			proposed = Rectangle.Empty;

			if (IsIconic(intPtr))
			{
				state = WindowState.MINIMIZED;
			}
			else if (IsZoomed(intPtr))
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
	}
}