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
		private RevitWindow() { }

		internal enum WindowStatus : short
		{
			DOC_MIN = -1,
			DOC_ACTIVE = 0,
			DOC_SELECT = 50,
			DOC_NONSEL = 100
		}

		internal static List<RevitWindow> ChildWindows;

		// the number of "selected" windows
		internal static int SelectedWinCount { get; private set; }
		// the number of "non-selected" windows
		internal static int NonSelWinCount { get; private set; }
		// the number of minimized windows
		internal static int MinimizedWinCount { get; private set; }

		private static bool _gotActive = false;

		private int			_sequence;
		private IntPtr		_handle;
//		private int			_docIndex;
		private string		_windowTitle;
		private int			_viewType;
		private WindowStatus _winStatus;
		internal Rectangle	Current;
		internal Rectangle	Proposed;

		internal RevitWindow(IntPtr child, Rectangle current, string currDoc)
		{
			string windowTitle = GetWindowTitle(child);

			bool isCurrDoc = windowTitle.ToLower().Contains(currDoc);

			_viewType = ViewTypeIndexOf(windowTitle);

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
					SelectedWinCount++;

					if (_gotActive)
					{
						_winStatus = WindowStatus.DOC_SELECT;
					}
					else
					{
						_winStatus = WindowStatus.DOC_ACTIVE;
						_gotActive = true;
					}
				}
				else
				{
					NonSelWinCount++;
					_winStatus = WindowStatus.DOC_NONSEL;
				}
			}

			_sequence = (int) _winStatus + _viewType;

			_handle = child;
			_windowTitle = windowTitle.ToLower();
			Current = current;
			Proposed = Rectangle.Empty;
		}

		

		internal int Sequence =>			_sequence;
		internal IntPtr Handle =>			_handle;
		internal string	WindowTitle =>		_windowTitle;
		internal WindowStatus WinStatus =>	_winStatus;
		internal int ViewType =>			_viewType;
//		internal int DocIndex =>			_docIndex;
//		internal string DocTitle =>			_formProjSel.GetDocName(_docIndex);

		internal bool IsMinimized =>	_winStatus == WindowStatus.DOC_MIN;
		internal bool IsSelected =>		_winStatus == WindowStatus.DOC_SELECT;
		internal bool IsNonSelected =>	_winStatus == WindowStatus.DOC_NONSEL;
		internal bool IsActive =>		_winStatus == WindowStatus.DOC_ACTIVE;

		internal static void ResetRevitWindows()
		{
			_gotActive = false;

			SelectedWinCount = 0;
			NonSelWinCount = 0;
			MinimizedWinCount = 0;

			ChildWindows = new List<RevitWindow>(5);

			InitViewTypeOrderList();
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