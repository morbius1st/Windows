#region Using directives
using System;
using Autodesk.Revit.DB;
using static Windows.Command;
using static Windows.WindowUtilities;
using static Windows.WindowApiUtilities.DeferWinPos;
using Rectangle = System.Drawing.Rectangle;

#endregion

// itemname:	WindowManager
// username:	jeffs
// created:		10/19/2017 8:34:36 PM


namespace Windows
{
	class WindowManager
	{
		private IntPtr _parent;
		private Rectangle _mainClientRect;
		private int _titleBarHeight;

		public WindowManager(IntPtr parent, Rectangle mainClientRect, 
			int titelBarHeight)
		{
			_parent = parent;
			_mainClientRect = mainClientRect;
			_titleBarHeight = titelBarHeight;

		}

		internal bool AdjustWindowLayout(int windowLayoutStyle)
		{
			bool result = false;

			switch (windowLayoutStyle)
			{
				case 0:
					result = OrganizeByProperCascade();

					SetupFormChildProp();

//					_form.useCurrent = false;
//					_form.MessageText = "proposed windows";
//					_form.ShowDialog();
//					_form.useCurrent = true;

					RepositionWindows();

					break;
				case 1:
					break;
			}

			return true;
		}

		bool OrganizeByProperCascade()
		{
			int marginTop = 0;
			int marginLeft = 20;
			int marginRight = 0;
			int marginBottom = 0;

			int idx = 0;

			int top;
			int left;
			int right = _mainClientRect.Right - marginRight;
			int bottom = _mainClientRect.Bottom - marginBottom;
			int inverseCount = 0;
			int baseTitleBarHeight = (int) (1.5 * _titleBarHeight);

			int count = Command.ActWindows.Count;

			idx = 0;

			for (int i = 0; i < count; i++)
			{
				Command.ActWindows[i].sequence = idx;

				inverseCount = count - idx;
				top = _mainClientRect.Top + idx * _titleBarHeight + marginTop;
				left = _mainClientRect.Left + baseTitleBarHeight * inverseCount + marginLeft;

				Command.ActWindows[i].proposed = NewRectangle(left, top, right, bottom);

				idx++;
			}

			OrganizeMinimized(idx);

			return true;
		}

		bool OrganizeMinimized(int idx)
		{
			if (Command.MinWindows.Count == 0) { return true;}

			int horizIdx = 0;

			int height = Command.MinWindows[0].current.Height;
			int top = _mainClientRect.Bottom - height;
			int left = _mainClientRect.Left;
			int width = Command.MinWindows[0].current.Width;

			// determine the maximum number of minimized windows to place horizontally
			int maxHorizontal = _mainClientRect.Width / Command.MinWindows[0].current.Width;

			for (int i = 0; i < Command.MinWindows.Count; i++)
			{
				if (horizIdx == maxHorizontal)
				{
					top -= height;
					left = _mainClientRect.Left;
					horizIdx = 0;

				}

				Command.MinWindows[i].sequence = idx++;
				Command.MinWindows[i].proposed = new Rectangle(left, top, width, height);

				left += width;
				horizIdx++;
			}
			return true;
		}

		// set up form with revised windows
		void SetupFormChildProp()
		{
			int idx = 0;

			foreach (RevitWindow rw in Command.ActWindows)
			{
				if (rw.proposed.Width == 0 || !rw.IsValid) { continue; }

				_form.SetChildProp(idx++, rw.proposed, rw.winTitle);
			}

			foreach (RevitWindow rw in Command.MinWindows)
			{
				if (rw.proposed.Width == 0 || !rw.IsValid) { continue; }

				_form.SetChildProp(idx++, rw.proposed, rw.winTitle, true);
			}
		}

		void RepositionWindows()
		{
			IntPtr hWinPosInfo =
				WindowApiUtilities.BeginDeferWindowPos(ActWindows.Count + MinWindows.Count);

			foreach (RevitWindow rw in ActWindows)
			{
				hWinPosInfo =
					WindowApiUtilities.DeferWindowPos(hWinPosInfo, rw.handle, ((IntPtr) 1),
						rw.proposed.Left, rw.proposed.Top, rw.proposed.Width, rw.proposed.Height,
						SWP_NOZORDER | SWP_NOACTIVATE | SWP_SHOWWINDOW);

				if (hWinPosInfo.Equals(null)) { return; }
			}

			foreach (RevitWindow rw in MinWindows)
			{
				hWinPosInfo =
					WindowApiUtilities.DeferWindowPos(hWinPosInfo, rw.handle, ((IntPtr) 0),
						rw.proposed.Left, rw.proposed.Top, rw.proposed.Width, rw.proposed.Height,
						SWP_NOZORDER | SWP_NOACTIVATE | SWP_NOSIZE | SWP_SHOWWINDOW);

				if (hWinPosInfo.Equals(null)) { return; }
			}

			using (Transaction t = new Transaction(_doc, "move windows"))
			{
				t.Start();
				WindowApiUtilities.EndDeferWindowPos(hWinPosInfo);
				t.Commit();
			}
		}

	}
}
