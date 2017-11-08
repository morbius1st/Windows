using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Rectangle = System.Drawing.Rectangle;

using static RevitWindows.Command;
using static RevitWindows.WindowApiUtilities;

namespace RevitWindows
{
	internal class RevitWindow
	{
		internal enum WindowStatus : short
		{
			CURR_DOC_ACTIVE = -1000,
			CURR_DOC_NORMAL = 0,
			CURR_DOC_MIN = 1000,
			OTHER_DOC = 2000
		}


		internal static List<RevitWindow> ChildWindows;
//		internal static List<RevitWindow> ChildWinMinimized;
//		internal static List<RevitWindow> ChildWinOther;

//		internal static IntPtr ActiveWindow { get; private set; } = IntPtr.Zero;

		internal static int NormalWinCount { get; private set; }
		internal static int MinimizedWinCount { get; private set; }
		internal static int OtherDocWinCount { get; private set; }

		private static bool _gotActive = false;


		private int			_sequence;
		private IntPtr		_handle;
		private string		_windowTitle;
		private ViewType	_viewType;
		private WindowStatus _winStatus;
		internal Rectangle	Current;
		internal Rectangle	Proposed;

		internal RevitWindow(IntPtr intPtr, View v, string winTitle)
		{
			if (v == null)
			{
				_winStatus = WindowStatus.OTHER_DOC;
				_viewType = ViewType.Internal;

				OtherDocWinCount++;
			}
			else
			{
				_viewType = v.ViewType;

				if (IsIconic(intPtr))
				{
					_winStatus = WindowStatus.CURR_DOC_MIN;

					MinimizedWinCount++;
				}
				else
				{
					NormalWinCount++;

					if (_gotActive)
					{
						_winStatus = WindowStatus.CURR_DOC_NORMAL;
					}
					else
					{
						_winStatus = WindowStatus.CURR_DOC_ACTIVE;
						_gotActive = true;
					}

				}
			}

			_handle = intPtr;
			_sequence = (int) _viewType + (int) _winStatus;
			_windowTitle = winTitle.ToLower();
			Current = Rectangle.Empty;
			Proposed = Rectangle.Empty;
			
		}

		private RevitWindow() { }

		internal int Sequence =>			_sequence;
		internal IntPtr Handle =>			_handle;
		internal string	WindowTitle =>		_windowTitle;
		internal WindowStatus WinStatus =>	_winStatus;
		internal ViewType ViewType =>		_viewType;

		internal bool IsMinimized =>	_winStatus == WindowStatus.CURR_DOC_MIN;
		internal bool IsNormal =>		_winStatus == WindowStatus.CURR_DOC_NORMAL;
		internal bool IsActive =>		_winStatus == WindowStatus.CURR_DOC_ACTIVE;
		internal bool IsOtherDoc =>		_winStatus == WindowStatus.OTHER_DOC;

		//		internal RevitWindow Clone()
		//		{
		//			RevitWindow rwn = new RevitWindow();
		//			rwn._sequence = this._sequence;
		//			rwn._handle = this._handle;
		//			rwn._windowTitle = this._windowTitle;
		//			rwn._viewType = this._viewType;
		//			rwn._state = this._state;
		//			rwn.Current = this.Current;
		//			rwn.Proposed = this.Proposed;
		//
		//			return rwn;
		//		}

		internal static void ResetChileWindows()
		{
			_gotActive = false;

			NormalWinCount = 0;
			MinimizedWinCount = 0;
			OtherDocWinCount = 0;

			ChildWindows = new List<RevitWindow>(5);
		}

		internal bool MakeActive()
		{
			if (!IsNormal) return false;

			int idx = FindActive();

			if (idx >= 0)
			{
				ChildWindows[idx]._winStatus = WindowStatus.CURR_DOC_NORMAL;
				ChildWindows[idx]._sequence = (int) _viewType + (int) _winStatus;
			}

			_winStatus = WindowStatus.CURR_DOC_ACTIVE;
			_sequence = (int) _viewType + (int) _winStatus;
			_gotActive = true;

			return true;
		}

		internal static int FindActive()
		{
			if (!_gotActive) return -1;

			int idx = 0;

			foreach (RevitWindow rw in ChildWindows)
			{
				if (rw.IsActive)
				{
					return idx;
				}

				idx++;
			}

			return -1;
		}

//		internal bool FromCurrentDocument()
//		{
//			return _windowTitle.Contains(Command.Doc.Title.ToLower());
//		}

		//			_viewType = v?.ViewType ?? ViewType.CeilingPlan; // 2
		//			_viewType = v?.ViewType ?? ViewType.Internal; // 214
		//			_viewType = v?.ViewType ?? ViewType.ThreeD; // 4
		//			_viewType = v?.ViewType ?? ViewType.Undefined; // 0
		//			_viewType = v?.ViewType ?? ViewType.Walkthrough; // 124
		//			_viewType = v?.ViewType ?? ViewType.AreaPlan; // 116
		//			_viewType = v?.ViewType ?? ViewType.ColumnSchedule; // 122
		//			_viewType = v?.ViewType ?? ViewType.CostReport; // 119
		//			_viewType = v?.ViewType ?? ViewType.Detail; // 118
		//			_viewType = v?.ViewType ?? ViewType.DraftingView; // 10
		//			_viewType = v?.ViewType ?? ViewType.DrawingSheet; // 6
		//			_viewType = v?.ViewType ?? ViewType.Elevation; // 3
		//			_viewType = v?.ViewType ?? ViewType.EngineeringPlan; // 115
		//			_viewType = v?.ViewType ?? ViewType.FloorPlan; // 1
		//			_viewType = v?.ViewType ?? ViewType.AreaPlan; // 116
		//			_viewType = v?.ViewType ?? ViewType.Legend; // 11
		//			_viewType = v?.ViewType ?? ViewType.ColumnSchedule; // 122
		//			_viewType = v?.ViewType ?? ViewType.CostReport; // 119
		//			_viewType = v?.ViewType ?? ViewType.LoadsReport; // 120
		//			_viewType = v?.ViewType ?? ViewType.PanelSchedule; // 123
		//			_viewType = v?.ViewType ?? ViewType.PresureLossReport; // 121
		//			_viewType = v?.ViewType ?? ViewType.ProjectBrowser; // 7
		//			_viewType = v?.ViewType ?? ViewType.Rendering; // 125
		//			_viewType = v?.ViewType ?? ViewType.Report; // 8
		//			_viewType = v?.ViewType ?? ViewType.Schedule; // 5
		//			_viewType = v?.ViewType ?? ViewType.Section; // 117
		//			_viewType = v?.ViewType ?? ViewType.SystemBrowser; // 12
		//			_viewType = v?.ViewType ?? ViewType.ThreeD; // 4
		//			_viewType = v?.ViewType ?? ViewType.Undefined; // 0
		//


	}
}