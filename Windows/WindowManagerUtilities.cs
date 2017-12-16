#region Using directives

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using static RevitWindows.RevitWindow;
using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowManager;



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

		private const int CASCADE_MIN_WIN = 3;
		private const int CASCADE_MIN_WIDTH_PIX = 533; // pixels
		private const int CASCADE_MIN_HEIGHT_PIX = 300; // pixels
		private const double CASCADE_MIN_WIDTH_PCT = 0.33; // percent
		private const double CASCADE_MIN_HEIGHT_PCT = CASCADE_MIN_WIDTH_PCT; // percent

		private const double CASCADE_COL_ADJ_HORIZ = 0.5;
		private const double CASCADE_COL_ADJ_VERT = 0.5;

		private const double CASCADE_BAD_WIDTH_PCT = 0.6;
		private const double CASCADE_BAD_HEIGHT_PCT = 0.6;

		// 0.0 means no size increase - must be greater than zero
		private const double TILE_SIDE_VIEW_WIDTH_INCREASE_PCT = 1.0;
		// 1.O means a 100% (double) increase above the minimim size
		// must be greater than zero
		private const double TILE_SIDE_VIEW_HEIGHT_INCREASE_PCT = 1.0;
		// how much larger / smaller to make the non_active windows
		// each time the increase / decrease buttons are pressed
		private const double TILE_SIDE_VIEW_PCT_ADJUST_AMT = 0.25;
		//
		private const double TILE_MAIN_VIEW_MIN_WIDTH_PCT = 0.60;
		private const double TILE_MAIN_VIEW_MIN_HEIGHT_PCT = 0.60;


		// instance variables
		private readonly IntPtr _parent;

		private static bool _initalized = false;

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

		private static int _availableWidth;
		private static int _availableHeight;

		private static int _minimizedWidth;
		private static int _minimizedHeight;

		private static int _selectedMinWidth;
		private static int _selectedMinHeight;

		private static int _nonCurrHeight;
		private static int _notCurrWidth;

		private static int _winAdjHoriz;
		private static int _winAdjVert;

		private static double _nonActiveWidthIncreasePct;
		private static double _nonActiveHeightIncreasePct;

		private UserSettings us;

		internal WindowManagerUtilities(IntPtr parent)
		{
			_parent = parent;

			GetScreenMetrics(_parent);

			if (!_initalized)
			{
				Initalize();

				_initalized = true;
			}

			us = new UserSettings();
		}

		//		void x()
		//		{
		//			us.MarginLeft;						// x - min 0 - max 20%
		//			us.MarginTop;						// x - min 0 - max 20%
		//			us.MarginRight;						// x - min 0 - max 20%
		//			us.MarginBottom;					// x - min 0 - max 20%
		//
		//			us.CascadeMinWidthPix;				// *
		//			us.CascadeMinHeightPix;				// *

		//			us.CascadeMinWidthPct;				// x - min .25 - max .75
		//			us.CascadeMinHeightPct;				// x - min .25 - max .75
		//
		//			us.CascadeColAdjHoriz;				// x - min .5 - max 1.5
		//			us.CascadeColAdjVert;				// x - min .5 - max 1.5
		//
		//			us.CascadeBadWidthPct;				// x - min .25 - max .75
		//			us.CascadeBadHeightPct;				// x - min .25 - max .75
		//
		//			us.TileSideViewWidthIncreasePct;	// x - min 0.0 - max 5.0
		//			us.TileSideViewHeightIncreasePct;	// x - min 0.0 - max 5.0
		//
		//			us.TileSideViewSizeAdjustAmt;		// *
		//
		//			us.TileMainViewMinWidthPct;			// x - min .25 - max .75
		//			us.TileMainViewMinHeightPct;		// x - min .25 - max .75
		//
		//		}



		internal bool AdjustNonActWidth(WindowLayoutStyle winLayoutStyle, bool increase)
		{

			// must be one of these layout styles
			// otherwise ignore
			if (winLayoutStyle != WindowLayoutStyle.ACTIVE_LEFT &&
				winLayoutStyle != WindowLayoutStyle.ACTIVE_LEFT_OVERLAP &&
				winLayoutStyle != WindowLayoutStyle.ACTIVE_RIGHT)
			{
				return true;
			}

			double proposedWidthAdjust;

			if (!increase)
			{
				proposedWidthAdjust = _nonActiveWidthIncreasePct - TILE_SIDE_VIEW_PCT_ADJUST_AMT;
				if (proposedWidthAdjust >= 0)
				{
					_nonActiveWidthIncreasePct = proposedWidthAdjust;
					return true;
				}
				else return false;
			}

			proposedWidthAdjust = _nonActiveWidthIncreasePct 
				+ TILE_SIDE_VIEW_PCT_ADJUST_AMT;

			if (CalcTotalSize(proposedWidthAdjust,
				MinWindowWidth, _selectedMinWidth) > _availableWidth)
			{
				return false;
			}

			_nonActiveWidthIncreasePct = proposedWidthAdjust;

			return true;
		}

		internal bool AdjustNonActHeight(WindowLayoutStyle winLayoutStyle, bool increase)
		{
			// must be one of these layout styles
			// otherwise ignore
			if (winLayoutStyle != WindowLayoutStyle.ACTIVE_TOP &&
				winLayoutStyle != WindowLayoutStyle.ACTIVE_BOTTOM)
			{
				return true;
			}

			double proposedHeightIncrease;

			if (!increase)
			{
				proposedHeightIncrease = _nonActiveHeightIncreasePct - TILE_SIDE_VIEW_PCT_ADJUST_AMT;
				if (proposedHeightIncrease >= 0)
				{
					_nonActiveHeightIncreasePct = proposedHeightIncrease;
					return true;
				}
				else return false;
			}

			proposedHeightIncrease = _nonActiveHeightIncreasePct 
				+ TILE_SIDE_VIEW_PCT_ADJUST_AMT;

			if (CalcTotalSize(proposedHeightIncrease,
				MinWindowWidth, _selectedMinWidth) > _availableWidth)
			{
				return false;
			}

			_nonActiveHeightIncreasePct = proposedHeightIncrease;

			return true;
		}

		private int CalcTotalSize(double proposedIncPct, int minWinSize, int selMinSize)
		{
			return selMinSize + (int) (minWinSize * (1.0 + proposedIncPct));
		}

		internal void Initalize()
		{
			_nonCurrHeight = MinWindowHeight;
			_notCurrWidth = MinWindowWidth;

			_winAdjHoriz = TitleBarHeight;
			_winAdjVert = TitleBarHeight;

			_minimizedHeight = GetSystemMetrics(SystemMetric.SM_CYMINIMIZED);
			_minimizedWidth = GetSystemMetrics(SystemMetric.SM_CXMINIMIZED);

			_availableWidth = ParentWindow.Width - us.MarginLeft - us.MarginRight;
			_availableHeight = ParentWindow.Height - us.MarginTop - us.MarginBottom;

			_nonActiveWidthIncreasePct = TILE_SIDE_VIEW_WIDTH_INCREASE_PCT;
			_nonActiveHeightIncreasePct = TILE_SIDE_VIEW_HEIGHT_INCREASE_PCT;

			_selectedMinWidth = (int) (_availableWidth * TILE_MAIN_VIEW_MIN_WIDTH_PCT);
			_selectedMinHeight = (int) (_availableHeight * TILE_MAIN_VIEW_MIN_HEIGHT_PCT);
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
				CurrDocWinCount < us.CasadeMinWindows)
			{
				messageError = 
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

			_selectedTop = us.MarginTop;
			_selectedLeft = us.MarginLeft + CurrDocWinCount * _winAdjHoriz;
			_selectedRight = ParentWindow.Width - us.MarginRight;
			_selectedBottom = ParentWindow.Height - us.MarginBottom;

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

			int minWinHeight = Math.Max((int) (ParentWindow.Height * CASCADE_MIN_HEIGHT_PCT), CASCADE_MIN_HEIGHT_PIX);
			int minWinWidth = Math.Max((int) (ParentWindow.Width * CASCADE_MIN_WIDTH_PCT), CASCADE_MIN_WIDTH_PIX);

			int maxWindowsHoriz = (ParentWindow.Width - us.MarginRight
				- us.MarginLeft - minWinWidth) / _winAdjHoriz;

			int maxWindowsVert = (ParentWindow.Height - us.MarginTop
				- us.MarginBottom - minWinHeight) / _winAdjVert;

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
				CurrDocWinCount < us.CasadeMinWindows)
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

			_selectedTop = us.MarginTop;
			_selectedLeft = us.MarginLeft + CurrDocWinCount * _winAdjHoriz;
			_selectedRight = ParentWindow.Width - us.MarginRight;
			_selectedBottom = ParentWindow.Height - us.MarginBottom;

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
			_selectedWidth = (int) (ParentWindow.Width * CASCADE_BAD_WIDTH_PCT);
			_selectedHeight = (int) (ParentWindow.Height * CASCADE_BAD_HEIGHT_PCT);

			// make sure that the window width and height is at least 2x the minimum window height and width
			_selectedWidth = _selectedWidth > MinWindowWidth * 2 ? _selectedWidth : MinWindowWidth * 2;
			_selectedHeight = _selectedHeight > MinWindowHeight * 2 ? _selectedHeight : MinWindowHeight * 2;

			// make sure there is enough height and width to actually cascade the windows
			// allow for 3 times the offset amount
			if (us.MarginTop + _selectedHeight + us.MarginBottom + _winAdjVert * us.CasadeMinWindows >= ParentWindow.Height
				|| us.MarginLeft + _selectedWidth + us.MarginRight + _winAdjHoriz * us.CasadeMinWindows >= ParentWindow.Width)
			{
				return false;
			}

			return true;
		}

		Rectangle RectForBadCascade(int width, int height, ref int idx, ref int col)
		{
			int left = CalcTopLeft(idx, col, us.MarginLeft, _winAdjHoriz, CASCADE_COL_ADJ_HORIZ * MinWindowWidth);
			int top = CalcTopLeft(idx, col, us.MarginTop, _winAdjVert, CASCADE_COL_ADJ_VERT * MinWindowHeight);

			if (left + width + us.MarginRight > ParentWindow.Width ||
				top + height + us.MarginBottom > ParentWindow.Height)
			{
				idx = 0;
				col++;
				left = CalcTopLeft(idx, col, us.MarginLeft, _winAdjHoriz, CASCADE_COL_ADJ_HORIZ * MinWindowWidth);
				top = CalcTopLeft(idx, col, us.MarginTop, _winAdjVert, CASCADE_COL_ADJ_VERT * MinWindowHeight);
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
				CurrDocWinCount < us.CasadeMinWindows)
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

			_selectedTop = us.MarginTop;
			_selectedLeft = us.MarginLeft;

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
					if (_selectedTop + _nonActiveHeight > _availableHeight + us.MarginTop)
					{
						_selectedTop = us.MarginTop;
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
				CurrDocWinCount < us.CasadeMinWindows)
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

			_selectedTop = us.MarginTop;
			_selectedLeft = us.MarginLeft + numOfCols * _nonActiveWidth;

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

					_selectedLeft = us.MarginLeft;

				}
				else if (rw.IsCurrDoc)
				{
					if (_selectedTop + _nonActiveHeight > _availableHeight + us.MarginTop)
					{
						_selectedTop = us.MarginTop;
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
			// calc the minimum width
			_selectedWidth = _selectedMinWidth;
			_selectedHeight = _availableHeight;

			// this is fixed
			_nonActiveWidth = (int) (MinWindowWidth * (1.0 + _nonActiveWidthIncreasePct));
			// this is a minimum - it can grow
			_nonActiveHeight = (int) (MinWindowHeight * (1.0 + _nonActiveWidthIncreasePct));

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
				CurrDocWinCount < us.CasadeMinWindows)
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

			_selectedTop = us.MarginTop;
			_selectedLeft = us.MarginLeft;

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
					if (_selectedLeft + _nonActiveWidth > us.MarginLeft + _availableWidth)
					{
						_selectedTop += _nonActiveHeight;
						_selectedLeft = us.MarginLeft;
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
				CurrDocWinCount < us.CasadeMinWindows)
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

			_selectedTop = us.MarginTop + numOfRows * _nonActiveHeight;
			_selectedLeft = us.MarginLeft;

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

					_selectedTop = us.MarginTop;

				}
				else if (rw.IsCurrDoc)
				{
					if (_selectedLeft + _nonActiveWidth > us.MarginLeft + _availableWidth)
					{
						_selectedTop += _nonActiveHeight;
						_selectedLeft = us.MarginLeft;
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
//			_availableWidth = ParentWindow.Width - us.MarginLeft - us.MarginRight;
//			_availableHeight = ParentWindow.Height - us.MarginTop - us.MarginBottom;

			// calc the width
			_selectedWidth = _availableWidth;
			// calc the min height
			_selectedHeight = _selectedMinHeight;

			// this is a minimum - it can grow
			_nonActiveWidth = (int) (MinWindowWidth * (1.0 + _nonActiveWidthIncreasePct));
			// this is fixed
			_nonActiveHeight = (int) (MinWindowHeight * (1.0 + _nonActiveHeightIncreasePct));

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
				CurrDocWinCount < us.CasadeMinWindows)
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

			_selectedTop = us.MarginTop;
			_selectedLeft = us.MarginLeft;

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
					_selectedTop = _availableHeight + us.MarginTop - tempNonActHeight;

				}
				else if (rw.IsCurrDoc)
				{
					if (_selectedTop < us.MarginTop)
					{
						_selectedLeft += _nonActiveWidth;
						_selectedTop = us.MarginTop + _availableHeight - _nonActiveLastHeight;
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
		}

		// validates based on minimum active window width and checking if there is enough
		// room to fit in all of the non-active windows
		// basically, the non-active width is fixed and the active window can grow
		// also the non-active windows are overlapped so that just their title bar is visible (minimum)
		int ValidateActOnLeftOrRightOverlapped()
		{
//			_availableWidth = ParentWindow.Width - us.MarginLeft - us.MarginRight;
//			_availableHeight = ParentWindow.Height - us.MarginTop - us.MarginBottom;

			// calc the minimum width
			_selectedWidth = _selectedMinWidth;
			_selectedHeight = _availableHeight;

			// this is fixed
			_nonActiveWidth = (int) (MinWindowWidth * (1.0 + _nonActiveWidthIncreasePct));
			// this is a minimum - it can grow - how ever this is the overlapping distance
			// there the last window will be "full height"
			_nonActiveSpacingVert = MinWindowHeight;
			_nonActiveHeight = (int) (MinWindowHeight * (1.0 + _nonActiveHeightIncreasePct));
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
//
//			clearConsole();
//
//			logMsg(nl);
//			logMsgFmtln("** adjustments| ", "before");
//			logMsgFmtln("CurrDocWinCount| ", CurrDocWinCount);
//			logMsgFmtln("_availableHeight| ", _availableHeight);
//			logMsgFmtln("_selectedHeight| ", _selectedHeight);
//			logMsgFmtln("_nonActiveSpacingVert| ", _nonActiveSpacingVert);
//			logMsgFmtln("_nonActiveHeight| ", _nonActiveHeight);
//			logMsgFmtln("_nonActiveLastHeight| ", _nonActiveLastHeight);
//			logMsgFmtln("lastHeightRemainder| ", lastHeightRemainder);
//			logMsgFmtln("adjAvailableHeight| ", adjAvailableHeight);
//			logMsgFmtln("totalNonActHeight| ", totalNonActHeight);
//			logMsgFmtln("totalNonActWidth| ", totalNonActWidth);
//			logMsgFmtln("numOfCols| ", numOfCols);
//			logMsgFmtln("numOfRows| ", numOfRows);
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
//
//			logMsg(nl);
//			logMsgFmtln("** adjustments| ", "after");
//			logMsgFmtln("CurrDocWinCount| ", CurrDocWinCount);
//			logMsgFmtln("_availableHeight| ", _availableHeight);
//			logMsgFmtln("_selectedHeight| ", _selectedHeight);
//			logMsgFmtln("_nonActiveSpacingVert| ", _nonActiveSpacingVert);
//			logMsgFmtln("_nonActiveHeight| ", _nonActiveHeight);
//			logMsgFmtln("_nonActiveLastHeight| ", _nonActiveLastHeight);
//			logMsgFmtln("lastHeightRemainder| ", lastHeightRemainder);
//			logMsgFmtln("adjAvailableHeight| ", adjAvailableHeight);
//			logMsgFmtln("totalNonActHeight| ", totalNonActHeight);
//			logMsgFmtln("totalNonActWidth| ", totalNonActWidth);
//			logMsgFmtln("numOfCols| ", numOfCols);
//			logMsgFmtln("numOfRows| ", numOfRows);
			//			logMsgFmtln("| ", );

			return numOfCols;
		}

	}
}
