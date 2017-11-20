using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Rectangle = System.Drawing.Rectangle;

using static RevitWindows.Command;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowUtilities;

namespace RevitWindows
{
	internal class RevitWindow
	{
		internal enum WindowStatus : short
		{
			DOC_MIN = -1,
			DOC_ACTIVE = 0,
			DOC_SELECT = 50,
			DOC_NONSEL = 100
		}

		

		internal static List<RevitWindow> ChildWindows;

		internal static int SelectedWinCount { get; private set; }
		internal static int NonSelWinCount { get; private set; }
		internal static int MinimizedWinCount { get; private set; }

		private static bool _gotActive = false;

		private int			_sequence;
		private IntPtr		_handle;
		private int			_docIndex;
		private string		_windowTitle;
		private int			_viewType;
		private WindowStatus _winStatus;
		internal Rectangle	Current;
		internal Rectangle	Proposed;

		internal RevitWindow(IntPtr child, int selDocIdx, Rectangle current)
		{
			string windowTitle = GetWindowTitle(child);

			_docIndex = _formProjSel.IndexOfDocument(windowTitle);

			if (_docIndex < 0) throw new IndexOutOfRangeException();

			_viewType = ViewTypeIndexOf(windowTitle);

			if (IsIconic(child))
			{
				_winStatus = WindowStatus.DOC_MIN;
				MinimizedWinCount++;
				_sequence = (int) _winStatus;
			}
			else
			{
				if (selDocIdx == _docIndex)
				{
					SelectedWinCount++;

					if (_gotActive)
					{
						_winStatus = WindowStatus.DOC_SELECT;
						_sequence = (int) _winStatus + _viewType;
					}
					else
					{
						_winStatus = WindowStatus.DOC_ACTIVE;
						_gotActive = true;
						_sequence = (int) _winStatus + _viewType;
					}
				}
				else
				{
					NonSelWinCount++;
					_winStatus = WindowStatus.DOC_NONSEL;
					_sequence = (_docIndex + 1) * (int) _winStatus + _viewType;
				}
			}

			_handle = child;
			_windowTitle = windowTitle.ToLower();
			Current = current;
			Proposed = Rectangle.Empty;
		}

		private RevitWindow() { }

		internal int Sequence =>			_sequence;
		internal IntPtr Handle =>			_handle;
		internal string	WindowTitle =>		_windowTitle;
		internal WindowStatus WinStatus =>	_winStatus;
		internal int ViewType =>			_viewType;
		internal int DocIndex =>			_docIndex;
		internal string DocTitle =>			_formProjSel.GetDocName(_docIndex);

		internal bool IsMinimized =>	_winStatus == WindowStatus.DOC_MIN;
		internal bool IsSelected =>		_winStatus == WindowStatus.DOC_SELECT;
		internal bool IsNonSelected =>	_winStatus == WindowStatus.DOC_NONSEL;
		internal bool IsActive =>		_winStatus == WindowStatus.DOC_ACTIVE;

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

		internal static void ResetRevitWindows()
		{
			_gotActive = false;

			SelectedWinCount = 0;
			NonSelWinCount = 0;
			MinimizedWinCount = 0;

			ChildWindows = new List<RevitWindow>(5);

			InitViewTypeOrderList2();
		}

//		internal bool MakeActive()
//		{
//			if (!IsNormal) return false;
//
//			int idx = FindActive();
//
//			if (idx >= 0)
//			{
//				ChildWindows[idx]._winStatus = WindowStatus.DOC_NORMAL;
//				ChildWindows[idx]._sequence = (int) _viewType + (int) _winStatus;
//			}
//
//			_winStatus = WindowStatus.DOC_ACTIVE;
//			_sequence = (int) _viewType + (int) _winStatus;
//			_gotActive = true;
//
//			return true;
//		}

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