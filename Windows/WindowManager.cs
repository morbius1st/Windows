#region Using directives
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static Windows.Command;

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

		internal bool AdjustWindowLayout(int WindowLayoutStyle,
			List<RevitWindow> revitWindows2)
		{
			bool result = false;

			List<RevitWindow> windows = revitWindows2;

			switch (WindowLayoutStyle)
			{
				case 0:
					result = OrganizeByProperCascade(ref windows);

					AssignProposedWindows(windows);

					_form.useCurrent = false;
					_form.MessageText = "proposed windows";
					_form.ShowDialog();
					_form.useCurrent = true;

					break;
				case 1:
					break;
			}

			return true;
		}

		bool OrganizeByProperCascade(ref List<RevitWindow> windows)
		{
			List<RevitWindow> minimized = new List<RevitWindow>();

			int marginTop = 0;
			int marginLeft = 20;
			int marginRight = 0;
			int marginBottom = 0;

			int idx = 0;
			int count = 0;

			int top;
			int left;
			int right = _mainClientRect.Right - marginRight;
			int bottom = _mainClientRect.Bottom - marginBottom;
			int inverseCount = 0;
			int baseTitleBarHeight = (int) (1.5 * _titleBarHeight);

			foreach (RevitWindow rw in windows)
			{
				rw.sequence = idx++;

				if (rw.IsMinimized)
				{
					minimized.Add(rw);
				}
			}

			count = windows.Count - minimized.Count - 1;
			idx = 0;

			foreach (RevitWindow rw in windows)
			{
				if (rw.IsMinimized)
				{
					continue; 
				}

				inverseCount = count - idx;
				top = _mainClientRect.Top + idx * _titleBarHeight + marginTop;
				left = _mainClientRect.Left + baseTitleBarHeight * inverseCount + marginLeft;

				rw.proposed = NewRectangle(left, top, right, bottom);

				idx++;
			}

			if (minimized.Count > 0)
			{
				minimized = OrganizeMinimized(minimized);
			}

			if (minimized.Count > 0 && minimized.Count < windows.Count && minimized[0].IsMinimized)
			{
				for (int i = 0; i < windows.Count; i++)
				{
					if (!windows[i].IsMinimized)
					{
						break;
					}

					windows.Add(windows[i].Clone());
					windows[i].IsValid = false;
				}
			}

			return true;
		}



		List<RevitWindow> OrganizeMinimized(List<RevitWindow> minimized)
		{
			int horizIdx = 0;

			int height = minimized[0].current.Height;
			int top = _mainClientRect.Bottom - height;
			int left = _mainClientRect.Left;
			int width = minimized[0].current.Width;

			// determine the maximum number of minimized windows to place horizontally
			int maxHorizontal = _mainClientRect.Width / minimized[0].current.Width;

			for (int i = 0; i < minimized.Count; i++)
			{
				if (horizIdx == maxHorizontal)
				{
					top -= height;
					left = _mainClientRect.Left;
					horizIdx = 0;
				}

				minimized[i].proposed = new Rectangle(left, top, width, height);

				left += width;
				horizIdx++;
			}
			return minimized;
		}


		void AssignProposedWindows(List<RevitWindow> windows)
		{
			int idx = 0;

			foreach (RevitWindow rw in windows)
			{
				if (rw.proposed.Width == 0) { continue; }
				if (!rw.IsValid) { continue; }

				_form.SetChildProp(idx++, rw.proposed, rw.winTitle, rw.IsMinimized);
			}
		}

	}
}
