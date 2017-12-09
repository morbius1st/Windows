using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Rectangle = System.Drawing.Rectangle;

using static Autodesk.Revit.DB.ViewType;

using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowManager;

namespace RevitWindows
{
	internal class RevitWindow
	{
//		private RevitWindow() { }

		internal enum WindowStatus : short
		{
			DOC_MIN = -1,
			DOC_ACTIVE = 0,
			DOC_CURRENT_SELECTED = 1000,
			DOC_CURRENT_NONSELECTED = 2000,
			DOC_NONCURRENT = 3000
		}

		internal static List<RevitWindow> ChildWindows;
		private static IList<View> _views;

		private static List<int> SelectedWindowsOrder = new List<int>();
		

		// the number of "current document" windows
		internal static int CurrDocWinCount { get; private set; }
		internal static int CurrDocSelWinCount { get; private set; }
		internal static int CurrDocNonSelWinCount { get; private set; }

		// the number of "non-current document" windows
		internal static int NonCurrDocWinCount { get; private set; }
		// the number of minimized windows
		internal static int MinimizedWinCount { get; private set; }

		private static bool _gotActive = false;

		private readonly int			_sequence;
		private readonly IntPtr			_handle;
		private readonly string			_windowTitle;
		private readonly int			_viewType;
		private readonly WindowStatus	_winStatus;
		internal Rectangle				Current;
		internal Rectangle				Proposed;

		internal RevitWindow(IntPtr child, Rectangle current, string currDoc)
		{
			string winTitle = GetWindowTitle(child);

			bool isCurrDoc = winTitle.ToLower().Contains(currDoc);

			_viewType = VIEW_TYPE_VOID;

			if (IsIconic(child))
			{
				_winStatus = WindowStatus.DOC_MIN;
				MinimizedWinCount++;
				_sequence = (int) _winStatus;
			}
			else
			{
				if (isCurrDoc)
				{
					_viewType = GetRevitViewType(_views, winTitle);

					CurrDocWinCount++;

					if (_gotActive)
					{
						// two choices here - selected or non-selected
						// determined by if in the selection list
						if (SelectedWindowsOrder.Contains(_viewType))
						{
							// got a selected
							CurrDocSelWinCount++;
							_winStatus = WindowStatus.DOC_CURRENT_SELECTED;
						}
						else
						{
							// non-selected
							CurrDocNonSelWinCount++;
							_winStatus = WindowStatus.DOC_CURRENT_NONSELECTED;
						}
					}
					else
					{
						_winStatus = WindowStatus.DOC_ACTIVE;
						_gotActive = true;
					}
				}
				else
				{
					NonCurrDocWinCount++;
					_winStatus = WindowStatus.DOC_NONCURRENT;
				}
			}

			_sequence = (int) _winStatus + _viewType;

			_handle = child;
			_windowTitle = winTitle.ToLower();
			Current = current;
			Proposed = Rectangle.Empty;
		}

		internal int Sequence =>			_sequence;
		internal IntPtr Handle =>			_handle;
		internal string	WindowTitle =>		_windowTitle;
		internal WindowStatus WinStatus =>	_winStatus;
		internal int ViewType =>			_viewType;

		internal bool HasActive =>			_gotActive;
		internal bool IsMinimized =>		_winStatus == WindowStatus.DOC_MIN;
		internal bool IsActive =>			_winStatus == WindowStatus.DOC_ACTIVE;
		internal bool IsCurrDoc =>			_winStatus == WindowStatus.DOC_CURRENT_SELECTED ||
												_winStatus == WindowStatus.DOC_CURRENT_NONSELECTED ||
												_winStatus == WindowStatus.DOC_ACTIVE;
		internal bool IsCurrDocSeleced =>	_winStatus == WindowStatus.DOC_CURRENT_SELECTED;
		internal bool IsCurrDocNonSeleced => _winStatus == WindowStatus.DOC_CURRENT_NONSELECTED;
												
		internal bool IsNonCurrDoc =>		_winStatus == WindowStatus.DOC_NONCURRENT;
			
		internal static void ResetRevitWindows()
		{
			_gotActive = false;

			CurrDocWinCount = 0;
			CurrDocSelWinCount = 0;
			CurrDocNonSelWinCount = 0;
			NonCurrDocWinCount = 0;
			MinimizedWinCount = 0;

			ChildWindows = new List<RevitWindow>(5);

			_views = GetRevitChildViews(Uidoc);

			SelectedWindowsOrder = new List<int>()
			{
				(int) ThreeD,
				(int) FloorPlan,
				(int) Elevation,
				(int) Section
			};
		}

	}
}