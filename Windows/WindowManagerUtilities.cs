#region Using directives

using System;
using System.Drawing;

using static RevitWindows.WindowUtilities;
using static RevitWindows.RevitWindow;
using static RevitWindows.WindowApiUtilities;

using static UtilityLibrary.MessageUtilities;


#endregion

// itemname:	WindowManagerUtilities
// username:	jeffs
// created:		11/4/2017 7:54:03 AM


namespace RevitWindows
{
	internal class WindowManagerUtilities
	{
		// to be changed to settings layer
		private static int MarginLeft { get; } = 20;
		private static int MarginTop { get; } = 20;
		private static int MarginRight { get; } = 20;
		private static int MarginBottom { get; } = 20;

		private const int MIN_WIN_IN_CASCADE = 3;
		private const int MIN_WIDTH_PIX = 533; // pixels
		private const int MIN_HEIGHT_PIX = 300; // pixels
		private const double MIN_WIDTH_PCT = 0.33; // percent
		private const double MIN_HEIGHT_PCT = MIN_WIDTH_PCT; // percent

		private const double COL_ADJUST_HORIZ = 0.5;
		private const double COL_ADJUST_VERT = 0.5;

		private const double BAD_CASCADE_WIDTH_PCT = 0.6;
		private const double BAD_CASCADE_HEIGHT_PCT = 0.6;

		// 0.0 means no size increase - must be greater than zero
		private const double NON_ACT_WIDTH_INCREASE_PCT = 0.0;
		// 1.O means a 100% (double) increase above the minimim size
		// must be greater than zero
		private const double NON_ACT_HEIGHT_INCREASE_PCT = 1.0;
		private const double ACTIVE_VIEW_MIN_WIDTH_PCT = 0.60;
		private const double ACTIVE_VIEW_MIN_HEIGHT_PCT = 0.60;

		// 1.O means a 100% (double) increase above the minimim size
		// must be greater than zero
		private const double NON_ACT_WIDTH_INCREASE_OVERLAP_PCT = 0.0;
		// 0.O means no increase above the minimim size
		// must be greater than zero
		private const double NON_ACT_HEIGHT_INCREASE_OVERLAP_PCT = 1.0;


		// instance variables
		private int _availableWidth;
		private int _availableHeight;

		private int _selectedTop;
		private int _selectedLeft;
		private int _selectedBottom;
		private int _selectedRight;
		private int _selectedWidth;
		private int _selectedHeight;

		private int _nonActiveWidth;
		private int _nonActiveHeight;
		private int _nonActiveLastHeight;
		private int _nonActiveSpacingVert;
		private int _nonActiveSpacingHoriz;

		private int _minimizedTop;
		private int _minimizedLeft;
		private int _minMaxRight;

		private int _minimizedHeight = GetSystemMetrics(SystemMetric.SM_CYMINIMIZED);
		private int _minimizedWidth = GetSystemMetrics(SystemMetric.SM_CXMINIMIZED);

		private static int _nonCurrHeight;
		private static int _notCurrWidth;

		//		private int _indexNormal;
		//		private int _indexMinimized;
		//		private int _row;

		private static int _winAdjHoriz;

		private WindowManagerUtilities winMgrUtil;


		internal static int NonCurrHeight
		{
			get { return _nonCurrHeight; }

			set { _nonCurrHeight = value; }
		}

		internal static int NotCurrWidth
		{
			get { return _notCurrWidth; }

			set { _notCurrWidth = value; }
		}

		public static int WinAdjHoriz
		{
			get { return _winAdjHoriz; }

			set { _winAdjHoriz = value; }
		}

		private static int _winAdjVert;

		public static int WinAdjVert
		{
			get { return _winAdjVert; }

			set { _winAdjVert = value; }
		}


		// proper cascade = cascade from top right to bottom left - 
		// keep right edge at right margin and bottom edge at bottom margin
		// left edge to left margin max
		internal void RepositionWindows()
		{
			RevitWindow active = null;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsActive)
				{
					active = rw;
				}
				else if (rw.IsCurrDoc || rw.IsNonCurrDoc)
				{
					SetWindowPos(rw.Handle, HWND.Bottom, rw.Proposed.X, rw.Proposed.Y,
						rw.Proposed.Width, rw.Proposed.Height, SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE);
				}
				else if (rw.IsMinimized)
				{
					SetWindowPos(rw.Handle, HWND.Bottom, rw.Proposed.X, rw.Proposed.Y,
						rw.Proposed.Width, rw.Proposed.Height, SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOSIZE);
				}
			}

			SetWindowPos(active.Handle, HWND.Top, active.Proposed.X, active.Proposed.Y,
				active.Proposed.Width, active.Proposed.Height, SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE);
		}

		internal bool OrganizeByProperCascade()
		{
			if (CurrDocWinCount == 0 ||
				CurrDocWinCount < MIN_WIN_IN_CASCADE)
			{
				WindowManager.messageError = 
					"At least three windows are needed in order to organize";

				return false;
			}

			// for this cascade, double the normal horizontal adjustment
			_winAdjHoriz = (int) (_winAdjHoriz * 1.5);

			if (!ValidateProperCascade())
			{
				WindowManager.messageError = "Cannot adjust the window layout as there is not enough screen space to proceed";
				return false;
			}

			bool gotFirstMinimized = false;
			bool gotFirstNotSel = false;

			_selectedTop = MarginTop;
			_selectedLeft = MarginLeft + CurrDocWinCount * _winAdjHoriz;
			_selectedRight = ParentWindow.Width - MarginRight;
			_selectedBottom = ParentWindow.Height - MarginBottom;

			_minimizedTop = ParentWindow.Height;
			_minimizedLeft = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsMinimized)
				{
					if (!gotFirstMinimized)
					{
						_minimizedTop -= _minimizedHeight;
						_minimizedLeft = 0;
						gotFirstMinimized = true;
					}
					rw.Proposed = RectForMinimized();
				}
				else if (rw.IsCurrDoc || rw.IsActive)
				{
					rw.Proposed = RectForProperCascade();
					
				}
				else
				{
					if (!gotFirstNotSel)
					{
						_minimizedTop -= _nonCurrHeight;
						_minimizedLeft = 0;
						gotFirstNotSel = true;
					}
					rw.Proposed = RectForNotSelected();
				}
			}
			return true;
		}

		bool ValidateProperCascade()
		{
			if (CurrDocWinCount == 0) { return false; }

			int minWinHeight = Math.Max((int) (ParentWindow.Height * MIN_HEIGHT_PCT), MIN_HEIGHT_PIX);
			int minWinWidth = Math.Max((int) (ParentWindow.Width * MIN_WIDTH_PCT), MIN_WIDTH_PIX);

			int maxWindowsHoriz = (ParentWindow.Width - MarginRight
				- MarginLeft - minWinWidth) / _winAdjHoriz;

			int maxWindowsVert = (ParentWindow.Height - MarginTop
				- MarginBottom - minWinHeight) / _winAdjVert;

			// can all of the windows be cascaded?
			if (maxWindowsHoriz < CurrDocWinCount
				|| maxWindowsVert < CurrDocWinCount) { return false; }

			return true;
		}

		Rectangle RectForProperCascade()
		{
			Rectangle r = NewRectangle(_selectedLeft, _selectedTop,
				_selectedRight, _selectedBottom);

			_selectedLeft -= _winAdjHoriz;
			_selectedTop += _winAdjVert;

			return r;
		}

		Rectangle RectForMinimized()
		{
			if (_minimizedLeft + _minimizedWidth > ParentWindow.Width)
			{
				_minimizedTop -= _minimizedHeight;
				_minimizedLeft = 0;
			}

			Rectangle r = new Rectangle(_minimizedLeft, _minimizedTop, _minimizedWidth, _minimizedHeight);

			_minimizedLeft += _minimizedWidth;

			return r;
		}

		Rectangle RectForNotSelected()
		{
			if (_minimizedLeft + _notCurrWidth > ParentWindow.Width)
			{
				_minimizedTop -= _nonCurrHeight;
				_minimizedLeft = 0;
			}

			Rectangle r = new Rectangle(_minimizedLeft, _minimizedTop, _notCurrWidth, _nonCurrHeight);

			_minimizedLeft += _notCurrWidth;

			return r;
		}

		// organize by windows stupid cascade method
		internal bool OrganizeByBadCascade()
		{
			if (CurrDocWinCount == 0 ||
				CurrDocWinCount < MIN_WIN_IN_CASCADE)
			{
				WindowManager.messageError =
					"At least three windows are needed in order to organize";

				return false;
			}

			if (!ValidateBadCascade())
			{
				WindowManager.messageError = "Cannot adjust the window layout as there is not enough screen space to proceed";
				return false;
			}

			bool gotFirstMinimized = false;
			bool gotFirstNotSel = false;

			_selectedTop = MarginTop;
			_selectedLeft = MarginLeft + CurrDocWinCount * _winAdjHoriz;
			_selectedRight = ParentWindow.Width - MarginRight;
			_selectedBottom = ParentWindow.Height - MarginBottom;

			_minimizedTop = ParentWindow.Height;
			_minimizedLeft = 0;

			int idx = 0;
			int col = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsMinimized)
				{
					if (!gotFirstMinimized)
					{
						_minimizedTop -= _minimizedHeight;
						_minimizedLeft = 0;
						gotFirstMinimized = true;
					}

					rw.Proposed = RectForMinimized();
				}
				else if (rw.IsCurrDoc || rw.IsActive)
				{
					rw.Proposed = RectForBadCascade(_selectedWidth, _selectedHeight, ref idx, ref col);

					idx++;
				}
				else
				{
					if (!gotFirstNotSel)
					{
						_minimizedTop -= _nonCurrHeight;
						_minimizedLeft = 0;
						gotFirstNotSel = true;
					}

					rw.Proposed = RectForNotSelected();
				}
			}

			return true;
		}

		bool ValidateBadCascade()
		{
			// determine the window width and height
			_selectedWidth = (int) (ParentWindow.Width * BAD_CASCADE_WIDTH_PCT);
			_selectedHeight = (int) (ParentWindow.Height * BAD_CASCADE_HEIGHT_PCT);

			// make sure that the window width and height is at least 2x the minimum window height and width
			_selectedWidth = _selectedWidth > MinWindowWidth * 2 ? _selectedWidth : MinWindowWidth * 2;
			_selectedHeight = _selectedHeight > MinWindowHeight * 2 ? _selectedHeight : MinWindowHeight * 2;

			// make sure there is enough height and width to actually cascade the windows
			// allow for 3 times the offset amount
			if (MarginTop + _selectedHeight + MarginBottom + _winAdjVert * MIN_WIN_IN_CASCADE >= ParentWindow.Height
				|| MarginLeft + _selectedWidth + MarginRight + _winAdjHoriz * MIN_WIN_IN_CASCADE >= ParentWindow.Width)
			{
				return false;
			}

			return true;
		}

		Rectangle RectForBadCascade(int width, int height, ref int idx, ref int col)
		{
			int left = CalcTopLeft(idx, col, MarginLeft, _winAdjHoriz, COL_ADJUST_HORIZ * MinWindowWidth);
			int top = CalcTopLeft(idx, col, MarginTop, _winAdjVert, COL_ADJUST_VERT * MinWindowHeight);

			if (left + width + MarginRight > ParentWindow.Width ||
				top + height + MarginBottom > ParentWindow.Height)
			{
				idx = 0;
				col++;
				left = CalcTopLeft(idx, col, MarginLeft, _winAdjHoriz, COL_ADJUST_HORIZ * MinWindowWidth);
				top = CalcTopLeft(idx, col, MarginTop, _winAdjVert, COL_ADJUST_VERT * MinWindowHeight);
			}

			return new Rectangle(left, top, width, height);
		}

		int CalcTopLeft(int idx, int col, int margin, int stackAdjustment, double colAdjustment)
		{
			return (int) (idx * stackAdjustment) + ((int) colAdjustment * col) + margin;
		}

		internal bool OrganizeByActOnLeft()
		{
			// need at least 3 selected windows
			if (CurrDocWinCount == 0 ||
				CurrDocWinCount < MIN_WIN_IN_CASCADE)
			{
				WindowManager.messageError =
					"At least three windows are needed in order to organize";

				return false;
			}

			int numOfCols = ValidateActOnLeftOrRight();

			if (numOfCols < 0)
			{
				WindowManager.messageError =
						"Cannot adjust the window layout as there is not enough screen space to proceed";

				return false;
			}

			bool gotFirstMinimized = false;
			bool gotFirstNotSel = false;

			_selectedTop = MarginTop;
			_selectedLeft = MarginLeft;

			_minimizedTop = ParentWindow.Height;
			_minimizedLeft = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsMinimized)
				{
					if (!gotFirstMinimized)
					{
						_minimizedTop -= _minimizedHeight;
						_minimizedLeft = 0;
						gotFirstMinimized = true;
					}
					rw.Proposed = RectForMinimized();
				}
				else if (rw.IsActive)
				{
					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _selectedWidth, _selectedHeight);

					_selectedLeft += _selectedWidth;

				}
				else if (rw.IsCurrDoc)
				{
					if (_selectedTop + _nonActiveHeight > _availableHeight + MarginTop)
					{
						_selectedTop = MarginTop;
						_selectedLeft += _nonActiveWidth;
					}

					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _nonActiveWidth, _nonActiveHeight);

					_selectedTop += _nonActiveHeight;
				}
				else
				{
					if (!gotFirstNotSel)
					{
						_minimizedTop -= _nonCurrHeight;
						_minimizedLeft = 0;
						gotFirstNotSel = true;
					}
					rw.Proposed = RectForNotSelected();
				}
			}
			return true;
		}

		internal bool OrganizeByActOnRight()
		{
			// need at least 3 selected windows
			if (CurrDocWinCount == 0 ||
				CurrDocWinCount < MIN_WIN_IN_CASCADE)
			{
				WindowManager.messageError =
					"At least three windows are needed in order to organize";

				return false;
			}

			int numOfCols = ValidateActOnLeftOrRight();

			if (numOfCols < 0)
			{
				WindowManager.messageError =
						"Cannot adjust the window layout as there is not enough screen space to proceed";

				return false;
			}

			bool gotFirstMinimized = false;
			bool gotFirstNotSel = false;

			_selectedTop = MarginTop;
			_selectedLeft = MarginLeft + numOfCols * _nonActiveWidth;

			_minimizedTop = ParentWindow.Height;
			_minimizedLeft = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsMinimized)
				{
					if (!gotFirstMinimized)
					{
						_minimizedTop -= _minimizedHeight;
						_minimizedLeft = 0;
						gotFirstMinimized = true;
					}
					rw.Proposed = RectForMinimized();
				}
				else if (rw.IsActive)
				{
					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _selectedWidth, _selectedHeight);

					_selectedLeft = MarginLeft;

				}
				else if (rw.IsCurrDoc)
				{
					if (_selectedTop + _nonActiveHeight > _availableHeight + MarginTop)
					{
						_selectedTop = MarginTop;
						_selectedLeft += _nonActiveWidth;
					}

					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _nonActiveWidth, _nonActiveHeight);

					_selectedTop += _nonActiveHeight;
				}
				else
				{
					if (!gotFirstNotSel)
					{
						_minimizedTop -= _nonCurrHeight;
						_minimizedLeft = 0;
						gotFirstNotSel = true;
					}
					rw.Proposed = RectForNotSelected();
				}
			}
			return true;
		}

		// validates based on a minimum active window width and checking if there is enough
		// room to fit in all of the non-active windows 
		// basically, the non-active width is fixed and the active window can grow
		int ValidateActOnLeftOrRight()
		{
			_availableWidth = ParentWindow.Width - MarginLeft - MarginRight;
			_availableHeight = ParentWindow.Height - MarginTop - MarginBottom;

			// calc the minimum width
			_selectedWidth = (int) (_availableWidth * ACTIVE_VIEW_MIN_WIDTH_PCT);
			_selectedHeight = _availableHeight;

			// this is fixed
			_nonActiveWidth = (int) (MinWindowWidth * (1.0 + NON_ACT_WIDTH_INCREASE_PCT));
			// this is a minimum - it can grow
			_nonActiveHeight = (int) (MinWindowHeight * (1.0 + NON_ACT_HEIGHT_INCREASE_PCT));

			int totalNonActHeight = _nonActiveHeight * (CurrDocWinCount - 1);
			int numOfCols = (int) Math.Ceiling((double) totalNonActHeight / _availableHeight);

			int totalNonActWidth = numOfCols * _nonActiveWidth;

			if (_selectedWidth + totalNonActWidth > _availableWidth)
			{
				return -1;
			}

			int numOfRows = (int) Math.Ceiling((double) (CurrDocWinCount - 1) / numOfCols);

			// adjust the non act window height so that each column is filled up
			_nonActiveHeight = _availableHeight / numOfRows;

			_selectedWidth = _availableWidth - (numOfCols * _nonActiveWidth);

			return numOfCols;
		}

		internal bool OrganizeByActOnTop()
		{
			// need at least 3 selected windows
			if (CurrDocWinCount == 0 ||
				CurrDocWinCount < MIN_WIN_IN_CASCADE)
			{
				WindowManager.messageError =
					"At least three windows are needed in order to organize";

				return false;
			}

			int numOfRows = ValidateActOnTopOrBottom();

			if (numOfRows < 0)
			{
				WindowManager.messageError =
						"Cannot adjust the window layout as there is not enough screen space to proceed";

				return false;
			}

			bool gotFirstMinimized = false;
			bool gotFirstNotSel = false;

			_selectedTop = MarginTop;
			_selectedLeft = MarginLeft;

			_minimizedTop = ParentWindow.Height;
			_minimizedLeft = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsMinimized)
				{
					if (!gotFirstMinimized)
					{
						_minimizedTop -= _minimizedHeight;
						_minimizedLeft = 0;
						gotFirstMinimized = true;
					}
					rw.Proposed = RectForMinimized();
				}
				else if (rw.IsActive)
				{
					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _selectedWidth, _selectedHeight);

					_selectedTop += _selectedHeight;

				}
				else if (rw.IsCurrDoc)
				{
					if (_selectedLeft + _nonActiveWidth > MarginLeft + _availableWidth)
					{
						_selectedTop += _nonActiveHeight;
						_selectedLeft = MarginLeft;
					}

					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _nonActiveWidth, _nonActiveHeight);

					_selectedLeft += _nonActiveWidth;
				}
				else
				{
					if (!gotFirstNotSel)
					{
						_minimizedTop -= _nonCurrHeight;
						_minimizedLeft = 0;
						gotFirstNotSel = true;
					}
					rw.Proposed = RectForNotSelected();
				}
			}
			return true;
		}


		internal bool OrganizeByActOnBottom()
		{
			// need at least 3 selected windows
			if (CurrDocWinCount == 0 ||
				CurrDocWinCount < MIN_WIN_IN_CASCADE)
			{
				WindowManager.messageError =
					"At least three windows are needed in order to organize";

				return false;
			}

			int numOfRows = ValidateActOnTopOrBottom();

			if (numOfRows < 0)
			{
				WindowManager.messageError =
						"Cannot adjust the window layout as there is not enough screen space to proceed";

				return false;
			}

			bool gotFirstMinimized = false;
			bool gotFirstNotSel = false;

			_selectedTop = MarginTop + numOfRows * _nonActiveHeight;
			_selectedLeft = MarginLeft;

			_minimizedTop = ParentWindow.Height;
			_minimizedLeft = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsMinimized)
				{
					if (!gotFirstMinimized)
					{
						_minimizedTop -= _minimizedHeight;
						_minimizedLeft = 0;
						gotFirstMinimized = true;
					}
					rw.Proposed = RectForMinimized();
				}
				else if (rw.IsActive)
				{
					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _selectedWidth, _selectedHeight);

					_selectedTop = MarginTop;

				}
				else if (rw.IsCurrDoc)
				{
					if (_selectedLeft + _nonActiveWidth > MarginLeft + _availableWidth)
					{
						_selectedTop += _nonActiveHeight;
						_selectedLeft = MarginLeft;
					}

					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _nonActiveWidth, _nonActiveHeight);

					_selectedLeft += _nonActiveWidth;
				}
				else
				{
					if (!gotFirstNotSel)
					{
						_minimizedTop -= _nonCurrHeight;
						_minimizedLeft = 0;
						gotFirstNotSel = true;
					}
					rw.Proposed = RectForNotSelected();
				}
			}
			return true;
		}

		// validates based on a minimum active window height and seeing of there is enough
		// room to fit in all of the non-active windows 
		// basically, the non-active height is fixed and the active window can grow
		int ValidateActOnTopOrBottom()
		{
			_availableWidth = ParentWindow.Width - MarginLeft - MarginRight;
			_availableHeight = ParentWindow.Height - MarginTop - MarginBottom;

			// calc the width
			_selectedWidth = _availableWidth;
			// calc the min height
			_selectedHeight = (int) (_availableHeight * ACTIVE_VIEW_MIN_HEIGHT_PCT);

			// this is a minimum - it can grow
			_nonActiveWidth = (int) (MinWindowWidth * (1.0 + NON_ACT_WIDTH_INCREASE_PCT));
			// this is fixed
			_nonActiveHeight = (int) (MinWindowHeight * (1.0 + NON_ACT_HEIGHT_INCREASE_PCT));

			int totalNonActWidth = _nonActiveWidth * (CurrDocWinCount - 1);
			int numOfRows = (int) Math.Ceiling((double) totalNonActWidth / _availableWidth);

			int totalNonActHeight = numOfRows * _nonActiveHeight;

			if (_selectedHeight + totalNonActHeight > _availableHeight)
			{
				return -1;
			}

			int numOfCols = (int) Math.Ceiling((double) (CurrDocWinCount - 1) / numOfRows);

			// adjust the non-act window width so that each column is filled up
			_nonActiveWidth = _availableWidth / numOfCols;

			_selectedHeight = _availableHeight - (numOfRows * _nonActiveHeight);

			return numOfRows;
		}

		internal bool OrganizeByActOnLeftOverlapped()
		{
			// need at least 3 selected windows
			if (CurrDocWinCount == 0 ||
				CurrDocWinCount < MIN_WIN_IN_CASCADE)
			{
				WindowManager.messageError =
					"At least three windows are needed in order to organize";

				return false;
			}

			int numOfRows = ValidateActOnLeftOrRightOverlapped();

			if (numOfRows < 0)
			{
				WindowManager.messageError =
						"Cannot adjust the window layout as there is not enough screen space to proceed";

				return false;
			}

			bool gotFirstMinimized = false;
			bool gotFirstNotSel = false;

			int tempNonActHeight = _nonActiveLastHeight;

			_selectedTop = MarginTop;
			_selectedLeft = MarginLeft;

			_minimizedTop = ParentWindow.Height;
			_minimizedLeft = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsMinimized)
				{
					if (!gotFirstMinimized)
					{
						_minimizedTop -= _minimizedHeight;
						_minimizedLeft = 0;
						gotFirstMinimized = true;
					}
					rw.Proposed = RectForMinimized();
				}
				else if (rw.IsActive)
				{
					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _selectedWidth, _selectedHeight);

					_selectedLeft += _selectedWidth;
					_selectedTop = _availableHeight + MarginTop - tempNonActHeight;

				}
				else if (rw.IsCurrDoc)
				{
					if (_selectedTop < MarginTop)
					{
						_selectedLeft += _nonActiveWidth;
						_selectedTop = MarginTop + _availableHeight - _nonActiveLastHeight;
						tempNonActHeight = _nonActiveLastHeight;
					}

					rw.Proposed =
						new Rectangle(_selectedLeft, _selectedTop, _nonActiveWidth, tempNonActHeight);

					_selectedTop -= _nonActiveSpacingVert;
					tempNonActHeight = _nonActiveHeight;
				}
				else
				{
					if (!gotFirstNotSel)
					{
						_minimizedTop -= _nonCurrHeight;
						_minimizedLeft = 0;
						gotFirstNotSel = true;
					}
					rw.Proposed = RectForNotSelected();
				}
			}
			return true;



			return true;
		}

		// validates based on minimum active window width and checking if there is enough
		// room to fit in all of the non-active windows
		// basically, the non-active width is fixed and the active window can grow
		// also the non-active windows are overlapped so that just their title bar is visible (minimum)
		int ValidateActOnLeftOrRightOverlapped()
		{
			_availableWidth = ParentWindow.Width - MarginLeft - MarginRight;
			_availableHeight = ParentWindow.Height - MarginTop - MarginBottom;

			// calc the minimum width
			_selectedWidth = (int) (_availableWidth * ACTIVE_VIEW_MIN_WIDTH_PCT);
			_selectedHeight = _availableHeight;

			// this is fixed
			_nonActiveWidth = (int) (MinWindowWidth * (1.0 + NON_ACT_WIDTH_INCREASE_OVERLAP_PCT));
			// this is a minimum - it can grow - how ever this is the overlapping distance
			// there the last window will be "full height"
			_nonActiveSpacingVert = MinWindowHeight;
			_nonActiveHeight = (int) (MinWindowHeight * (1.0 + NON_ACT_HEIGHT_INCREASE_OVERLAP_PCT));
			// this is basically fixed but can grow a little to fill in the space
			_nonActiveLastHeight = _nonActiveHeight;

			int x = CurrDocNonSelWinCount;
			int y = CurrDocSelWinCount;
			int z = CurrDocWinCount;

			int lastHeightRemainder = _nonActiveLastHeight - _nonActiveSpacingVert;

			int adjAvailableHeight = _availableHeight - lastHeightRemainder;

			int totalNonActHeight = _nonActiveSpacingVert * (CurrDocWinCount - 1);

			int numOfCols = (int) Math.Ceiling((double) totalNonActHeight / adjAvailableHeight);

			int totalNonActWidth = numOfCols * _nonActiveWidth;

			if (_selectedWidth + totalNonActWidth > _availableWidth)
			{
				return -1;
			}

			int numOfRows = (int) Math.Ceiling((double) (CurrDocWinCount - 1) / numOfCols);

			clearConsole();

			logMsg(nl);
			logMsgFmtln("** adjustments| ", "before");
			logMsgFmtln("CurrDocWinCount| ", CurrDocWinCount);
			logMsgFmtln("_availableHeight| ", _availableHeight);
			logMsgFmtln("_selectedHeight| ", _selectedHeight);
			logMsgFmtln("_nonActiveSpacingVert| ", _nonActiveSpacingVert);
			logMsgFmtln("_nonActiveHeight| ", _nonActiveHeight);
			logMsgFmtln("_nonActiveLastHeight| ", _nonActiveLastHeight);
			logMsgFmtln("lastHeightRemainder| ", lastHeightRemainder);
			logMsgFmtln("adjAvailableHeight| ", adjAvailableHeight);
			logMsgFmtln("totalNonActHeight| ", totalNonActHeight);
			logMsgFmtln("totalNonActWidth| ", totalNonActWidth);
			logMsgFmtln("numOfCols| ", numOfCols);
			logMsgFmtln("numOfRows| ", numOfRows);
//			logMsgFmtln("| ", );


			// adjust the vert spacing so that each column is filled up
			_nonActiveSpacingVert = (_availableHeight - _nonActiveLastHeight) / (numOfRows - 1);

			if (_nonActiveSpacingVert > _nonActiveHeight)
			{
				_nonActiveHeight = _availableHeight / numOfRows;
				_nonActiveLastHeight = _availableHeight - _nonActiveHeight * (numOfRows - 1);
				_nonActiveSpacingVert = _nonActiveHeight;
			}
			else
			{
				_nonActiveLastHeight = _availableHeight - (_nonActiveSpacingVert * (numOfRows - 1));
			}
			_selectedWidth = _availableWidth - (numOfCols * _nonActiveWidth);

			logMsg(nl);
			logMsgFmtln("** adjustments| ", "after");
			logMsgFmtln("CurrDocWinCount| ", CurrDocWinCount);
			logMsgFmtln("_availableHeight| ", _availableHeight);
			logMsgFmtln("_selectedHeight| ", _selectedHeight);
			logMsgFmtln("_nonActiveSpacingVert| ", _nonActiveSpacingVert);
			logMsgFmtln("_nonActiveHeight| ", _nonActiveHeight);
			logMsgFmtln("_nonActiveLastHeight| ", _nonActiveLastHeight);
			logMsgFmtln("lastHeightRemainder| ", lastHeightRemainder);
			logMsgFmtln("adjAvailableHeight| ", adjAvailableHeight);
			logMsgFmtln("totalNonActHeight| ", totalNonActHeight);
			logMsgFmtln("totalNonActWidth| ", totalNonActWidth);
			logMsgFmtln("numOfCols| ", numOfCols);
			logMsgFmtln("numOfRows| ", numOfRows);
			//			logMsgFmtln("| ", );

			return numOfCols;
		}

	}
}
