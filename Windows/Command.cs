#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Rectangle = System.Drawing.Rectangle;
using static Windows.Command.WinStyleEx;

#endregion

namespace Windows
{
	[Transaction(TransactionMode.Manual)]
	public class Command : IExternalCommand
	{
		private const string MAIN_WINDOW_KEY = "Main :: Window";

		internal static string nl = Environment.NewLine;
		internal static string pattRect = "x1|{0,5:D} y1|{1,5:D} x2|{2,5:D} y2|{3,5:D}";

		private UIApplication _uiapp;
		private UIDocument _uidoc;
		private Application _app;
		private Document _doc;

		internal static MainForm _form;

		// found child windows - active and minimized
		private List<RevitWindow> ActWindows = new List<RevitWindow>(5);
		private List<RevitWindow> MinWindows = new List<RevitWindow>(5);


		public Result Execute(
			ExternalCommandData commandData,
			ref string message,
			ElementSet elements)
		{
			_uiapp = commandData.Application;
			_uidoc = _uiapp.ActiveUIDocument;
			_app = _uiapp.Application;
			_doc = _uidoc.Document;

			WindowManager winMgr;

			Rectangle mainClientRect;

			int WindowLayoutStyle = 0;
			int titleBarHeight;

			bool result;

			_form = new MainForm();

			logMsgln("      screen name| " + _form.screen.DeviceName);
			logMsgln("  screen primary?| " + _form.screen.Primary);
			logMsgln(" screen work rect| " + ListRect(_form.screen.WorkingArea));
			logMsgln("screen bound rect| " + ListRect(_form.screen.Bounds));

			// get the revit process
			Process revitProcess = GetRevit();
			if (revitProcess == null) { return Result.Failed; }

			// from the process, get the parent window handle
			IntPtr parent = GetMainWinHandle(revitProcess);

			// determine the main client rectangle - the repositioned
			// view window go here
			mainClientRect = NewRectangle(_uiapp.DrawingAreaExtents).Adjust(-2);
			titleBarHeight = GetTitleBarHeight(parent);

			// get the list of child windows
			List<RevitWindow> revitWindows =
				GetRevitChildWindows(parent);

			_form.MakeChildrenLabels(revitWindows.Count);

			// these are just testing routines
			//			ShowInfo(revitWindows, form, parent, mainClientRect);
			//			ListAllChildWindows(parent);
			ShowInfo(_form, revitWindows, parent, mainClientRect);


			// process and adjust the windows
			winMgr = new WindowManager(parent, mainClientRect, titleBarHeight);
			winMgr.AdjustWindowLayout(WindowLayoutStyle, revitWindows);

			return Result.Succeeded;
		}

		void ShowInfo(MainForm form, List<RevitWindow> revitWindows2,
			IntPtr parent, Rectangle mainClientRect)
		{
			// list the child windows
			ListChildWindowInfo(form, revitWindows2);

			// setup the information for the form
			// show the form
			SetupForm(form, parent, mainClientRect, revitWindows2);

			form.useCurrent = true;
			form.ShowDialog(new WinHandle(parent));
		}

		internal Process GetRevit()
		{
			string mainWinTitle = _doc.Title.ToLower();

			Process[] revits = Process.GetProcessesByName("Revit");
			Process foundRevit = null;

			if (revits.Length > 0)
			{
				foreach (Process revit in revits)
				{
					if (revit.MainWindowTitle.ToLower().Contains(mainWinTitle))
					{
						foundRevit = revit;
						break;
					}
				}
			}
			else
			{
				foundRevit = revits[0];
			}

			return foundRevit;
		}

		

		// an AutoDesk rectangle from a system rectangle
		internal static Autodesk.Revit.DB.Rectangle ConvertTo(Rectangle r)
		{
			return new Autodesk.Revit.DB.Rectangle(r.Left, r.Top, r.Right, r.Bottom);
		}

		// system rectangle from an AutoDesk rectangle
		internal static Rectangle NewRectangle(Autodesk.Revit.DB.Rectangle r)
		{
			return NewRectangle(r.Left, r.Top, r.Right, r.Bottom);
		}

		// system rectangle from a RECT struct
		internal static Rectangle NewRectangle(Command.RECT r)
		{
			return new Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
		}

		// system rectangle from two sets of coordinates
		internal static Rectangle NewRectangle(int left, int top, int right, int bottom)
		{
			int w = right - left;
			int h = bottom - top;

			return new Rectangle(left, top, w < 0 ? -1 * w : w, h < 0 ? -1 * h : h);
		}

		private void logMsg(string message)
		{
			Debug.Write(message);
		}

		private void logMsgln(string message)
		{
			logMsg(message + nl);
		}

		private IntPtr GetMainWinHandle(Process revitProcess)
		{
			IntPtr parent = revitProcess.MainWindowHandle;
			ListMainWinInfo(parent);
			return parent;
		}
		
		public static List<IntPtr> GetChildWindows(
			IntPtr parent)
		{
			List<IntPtr> result = new List<IntPtr>();
			GCHandle listHandle = GCHandle.Alloc(result);
			try
			{
				EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
				EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
			}
			finally
			{
				if (listHandle.IsAllocated)
					listHandle.Free();
			}
			return result;
		}

		bool GetRevitChildWindows(IntPtr parent)
		{
			List<IntPtr> children = GetChildWindows(parent);

			if (children == null || children.Count == 0) { return null; }

			foreach (IntPtr child in children)
			{
				WINDOWINFO wi = new WINDOWINFO(true);
				GetWindowInfo(child, ref wi);

				if ((wi.dwExStyle & (uint) WS_EX_MDICHILD) == 0) { continue; }

				bool isMin = IsIconic(child);

				StringBuilder winTitle = new StringBuilder(255);
				GetWindowText(child, winTitle, 255);

				RevitWindow rw = new RevitWindow
				{
					sequence = -1, // flag as not specified
					handle = child, // handle to the window
					docTitle = _doc.Title,
					winTitle = winTitle.ToString(),
					IsMinimized = isMin,
					current = NewRectangle(wi.rcWindow),
					proposed = Rectangle.Empty
				};

				if (isMin)
				{
					MinWindows.Add(rw);
				}
				else
				{
					ActWindows.Add(rw);
				}
			}
			return true;
		}

		int GetTitleBarHeight(IntPtr window)
		{
			TITLEBARINFO ti = new TITLEBARINFO();
			ti.cbSize = (uint) Marshal.SizeOf(typeof(TITLEBARINFO));
			GetTitleBarInfo(window, ref ti);

			return ti.rcTitleBar.Bottom - ti.rcTitleBar.Top;
		}

		// adjusts the dimensions of the by the amount specified
		// adjustment is made thus: 
		// top & left += amount
		// bottom & right -= amount
		Rectangle AdjustWindowRectangle(RECT rect, int amount)
		{
			rect.Top += amount;
			rect.Left += amount;
			rect.Bottom -= amount;
			rect.Right -= amount;

			return NewRectangle(rect);
		}

		void ListTitleBarInfo(IntPtr win)
		{
			TITLEBARINFO ti = new TITLEBARINFO();
			ti.cbSize = (uint) Marshal.SizeOf(typeof(TITLEBARINFO));

			
			logMsgln("Title bar info");
			logMsgln("Title bar rect| " + ListRect(ti.rcTitleBar));
			logMsgln("Title bar ht  | " + (ti.rcTitleBar.Bottom - ti.rcTitleBar.Top));

		}


		private static int _WinStyleCount = Enum.GetNames(typeof(WinStyle)).Length;
		private static int _WinStyleExCount = Enum.GetNames(typeof(WinStyleEx)).Length;

		// short term - one item
		private int[] _WinStyleData = new int[_WinStyleCount];
		private int[] _WinStyleExData = new int[_WinStyleExCount];

		// long term - all items
		private int[] _WinStyleRecord = new int[_WinStyleCount];
		private int[] _WinStyleExRecord = new int[_WinStyleExCount];

		void InitWinStyleRecords()
		{
			for (int i = 0; i < _WinStyleRecord.Length; i++)
			{
				_WinStyleRecord[i] = 0;
			}
			for (int i = 0; i < _WinStyleExRecord.Length; i++)
			{
				_WinStyleExRecord[i] = 0;
			}
		}


		void AnalyzeWinStyles<T>(uint style, ref int[] record, ref int[] data, 
			ref uint copy, ref string result)
		{
			int idx = 0;

			uint[] vals = Enum.GetValues(typeof(T)) as uint[];

			foreach (uint ws in vals)
			{
				if ((style & ws) == ws)
				{
					copy = copy | ws;
					data[idx]++;
					record[idx]++;
					if (result.Length > 0) { result += " :: ";}
					result += Enum.GetName(typeof(T), ws);
				}

				idx++;
			}
		}

		void ListWinStyleResult(int[] StyleRecord)
		{
			foreach (int s in StyleRecord)
			{
				if (s > 0)
				{
					logMsg($"| {s:D4} ");
				}
				else
				{
					logMsg("|      ");
				}
			}
			logMsgln("|");
		}

		void ListWinStyleNames<T>()
		{
			string[] names = Enum.GetNames(typeof(T));

			int offset = 5;
			int num = ((names.Length + offset - 1) / offset) * offset;
			string patt;


			for (int j = 0; j < offset; j++)
			{
				patt = string.Format("{{0,{0}}}", (78 + 7 * j));

				logMsg(string.Format(patt, ">  "));
				for (int i = j; i < num; i += offset)
				{
					if (i >= names.Length) { continue; }
					logMsg($"{names[i], -35}");
				}

				logMsg(nl);
			}			
		}

		void ListAllChildWindows(IntPtr parent)
		{
			uint copy = 0;
			string result = "";

			List<IntPtr> children = GetChildWindows(parent);

			if (children == null || children.Count == 0) { return; }

			foreach (IntPtr child in children)
			{
				_WinStyleData = new int[_WinStyleCount];
				_WinStyleExData = new int[_WinStyleExCount];

				WINDOWINFO wi = new WINDOWINFO(true);
				GetWindowInfo(child, ref wi);

				WINDOWPLACEMENT wp = WINDOWPLACEMENT.Default;
				GetWindowPlacement(child, ref wp);

				StringBuilder className = new StringBuilder(40);
				GetClass(child, className, 40);

				StringBuilder winText = new StringBuilder(40);
				GetWindowText(child, winText, 40);

				AnalyzeWinStyles<WinStyle>(wi.dwStyle, ref _WinStyleRecord, 
					ref _WinStyleData, ref copy, ref result);
				AnalyzeWinStyles<WinStyleEx>(wi.dwExStyle, ref _WinStyleExRecord, 
					ref _WinStyleExData, ref copy, ref result);

				logMsg(" win class| " + $"{className,-42}");

				logMsg("    win style data| > ");
				ListWinStyleResult(_WinStyleData);

				logMsg("  win text| " + $"{winText, -42}");
				logMsg(" win style ex data| > ");
				ListWinStyleResult(_WinStyleExData);

			}

			logMsgln(nl);

			logMsg("winstyle R| " + $"{"> ", 64}");
			ListWinStyleNames<WinStyle>();
			ListWinStyleResult(_WinStyleRecord);


			logMsg("winstyleRx| " + $"{"> ",64}");
			ListWinStyleNames<WinStyleEx>();
			ListWinStyleResult(_WinStyleExRecord);

		}

		void ListMainWinInfo(IntPtr parent)
		{
			logMsgln("            main window| title| " + _doc.Title + "  (" + _doc.PathName + ")");
			logMsgln("                 intptr| " + parent.ToString());
			logMsgln("                extents| " + ListRect(NewRectangle(_uiapp.MainWindowExtents)));
			logMsgln(nl);

		}

		void ListRevitUiViews()
		{
			// process revit views
			logMsgln("revit window rectangles| ");
			IList<UIView> views = _uidoc.GetOpenUIViews();
			Autodesk.Revit.DB.Rectangle r = null;

			foreach (UIView v in views)
			{
				Element e = _doc.GetElement(v.ViewId);

				logMsg("           view name   | ");
				logMsgln(e.Name);

				r = v.GetWindowRectangle();
				logMsg("           view extents| ");
				logMsgln(String.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom));
				logMsg(nl);
			}
		}

//
//		void ListChildWindowInfo(MainForm form, Dictionary<string, RevitWindow> revitWindows)
//		{
//			if (revitWindows == null) { return; }
//
//			logMsgln(nl);
//			logMsgln("listing found revit windows");
//
//			RevitWindow rw;
//
//			uint copy = 0;
//			int idx = 0;
//
//			string result = "";
//
//			foreach (KeyValuePair<string, RevitWindow> kvp in revitWindows)
//			{
//				rw = kvp.Value;
////				InitWinStyleRecords();
//
//				logMsgln("               key| " + kvp.Key);
//				logMsgln(" child window text| " + rw.winTitle);
//				logMsgln("        child rect| " + ListRect(rw.winInfo.rcWindow));
//				logMsgln("   child win state| " + Enum.GetName(typeof(ShowWindowCommands), rw.winPlacement.ShowCmd));
//				logMsgln("      child intptr| " + rw.handle.ToString());
//
//				copy = 0;
//				result = "";
//
//				_WinStyleData = new int[_WinStyleCount];
//				AnalyzeWinStyles<WinStyle>(rw.winInfo.dwStyle, ref _WinStyleRecord, 
//					ref _WinStyleData, ref copy, ref result);
//
//				logMsgln("       child style| " + rw.winInfo.dwStyle);
//				logMsgln("child style calc'd| " + copy);
//				logMsgln("         winstyles| " + result);
//
//				ListWinStyleResult(_WinStyleRecord);
//
//				copy = 0;
//				result = "";
//
//				_WinStyleExData = new int[_WinStyleExCount];
//				AnalyzeWinStyles<WinStyleEx>(rw.winInfo.dwExStyle, ref _WinStyleExRecord, 
//					ref _WinStyleExData, ref copy, ref result);
//
//				logMsgln("    child style ex| " + rw.winInfo.dwExStyle);
//				logMsgln("child style calc'd| " + copy);
//				logMsgln("         winstyles| " + result);
//
//				ListWinStyleResult(_WinStyleExRecord);
//
//
//				form.AssignChildWindow(idx++, NewRectangle(rw.winInfo.rcWindow));
//			}
//		}

		void ListChildWindowInfo(MainForm form, List<RevitWindow> revitWindows2)
		{
			if (revitWindows2 == null) { return; }

			logMsgln(nl);
			logMsgln("listing found revit windows");
			logMsg(nl);

			foreach (RevitWindow rw in revitWindows2)
			{
				logMsgln("       child handle| " + rw.handle);
				logMsgln(" child is minimized| " + rw.IsMinimized.ToString());
				logMsgln(" child rect current| " + ListRect(rw.current));
				logMsgln("child rect proposed| " + ListRect(rw.proposed));
				logMsg(nl);

//				form.AssignChildWindow(idx++, rw.current);
			}

		}

		string ListRect(RECT r)
		{
			return String.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom);
		}

		internal static string ListRect(Rectangle r)
		{
			return string.Format(pattRect, r.Left, r.Top, r.Right, r.Bottom);
		}

//		void SetupForm(MainForm form, IntPtr parent, Rectangle mainClientRect, 
//			Dictionary<string, RevitWindow> RevitWindows)
//		{
//			SetupFormMainClientRect(form, parent, mainClientRect);
//			SetupFormChildRect(form, RevitWindows);
//		}

		void SetupForm(MainForm form, IntPtr parent, Rectangle mainClientRect, 
			List<RevitWindow> RevitWindows2)
		{
			SetupFormMainClientRect(form, parent, mainClientRect);
			SetupFormChildCurr(form, RevitWindows2);
		}


		void SetupFormMainClientRect(MainForm form, IntPtr parent, Rectangle mainClientRect)
		{
			form.RevitMainWorkArea = mainClientRect;

			WINDOWINFO wip = new WINDOWINFO(true);
			GetWindowInfo(parent, ref wip);

			logMsgln("      win info win rect| " + ListRect(wip.rcWindow));
			logMsgln("   win info client rect| " + ListRect(wip.rcClient));

			form.ParentRectForm = NewRectangle(wip.rcWindow);
			form.ParentRectClient = NewRectangle(wip.rcClient);
		}

//		void SetupFormChildRect(MainForm form, 
//			Dictionary<string, RevitWindow> RevitWindows)
//		{
//			int idx = 0;
//
//			foreach (KeyValuePair<string, RevitWindow> kvp in RevitWindows)
//			{
//				form.AssignChildWindow(idx++, 
//					NewRectangle(kvp.Value.winInfo.rcWindow));
//			}
//		}

		void SetupFormChildCurr(MainForm form, List<RevitWindow> RevitWindows2)
		{
			int idx = 0;

			foreach (RevitWindow rw in RevitWindows2)
			{
				form.SetChildCurr(idx++, rw.current, rw.winTitle, rw.IsMinimized);
			}
		}


		public class WinHandle : IWin32Window
		{
			public WinHandle(IntPtr h)
			{
				if (h == null)
				{
					throw new NullReferenceException();
				}
				Handle = h;
			}

			public IntPtr Handle { get; }
		}

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsChild(IntPtr hWndParent, IntPtr hWndChild);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern IntPtr GetTopWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowInfo(IntPtr hWnd, ref WINDOWINFO lpwndpl);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetTitleBarInfo(IntPtr hwnd, ref TITLEBARINFO pti);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern int GetWindowText(
			IntPtr hWnd, [Out] StringBuilder lpString,
			int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern int GetWindowTextLength(
			IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetWindowRect(
			IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", EntryPoint = "GetClassName")]
		public static extern int GetClass(
			IntPtr hWnd, StringBuilder className, int nMaxCount);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumChildWindows(
			IntPtr window, EnumWindowProc callback, IntPtr i);

		public delegate bool EnumWindowProc(
			IntPtr hWnd, IntPtr parameter);

		private static bool EnumWindow(
			IntPtr handle,
			IntPtr pointer)
		{
			GCHandle gch = GCHandle.FromIntPtr(pointer);

			List<IntPtr> list = gch.Target as List<IntPtr>;
			if (list != null)
			{
				list.Add(handle);
			}

			return true;
		}

		internal struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct TITLEBARINFO
		{
			public const int CCHILDREN_TITLEBAR = 5;
			public uint cbSize;
			public RECT rcTitleBar;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = CCHILDREN_TITLEBAR + 1)]
			public uint[] rgstate;
		}

		/// <summary>
		/// Contains information about the placement of a window on the screen.
		/// </summary>
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWPLACEMENT
		{
			/// <summary>
			/// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
			/// <para>
			/// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
			/// </para>
			/// </summary>
			public int Length;

			/// <summary>
			/// Specifies flags that control the position of the minimized window and the method by which the window is restored.
			/// </summary>
			public int Flags;

			/// <summary>
			/// The current show state of the window.
			/// </summary>
			public ShowWindowCommands ShowCmd;

			/// <summary>
			/// The coordinates of the window's upper-left corner when the window is minimized.
			/// </summary>
			public POINT MinPosition;

			/// <summary>
			/// The coordinates of the window's upper-left corner when the window is maximized.
			/// </summary>
			public POINT MaxPosition;

			/// <summary>
			/// The window's coordinates when the window is in the restored position.
			/// </summary>
			public RECT NormalPosition;

			/// <summary>
			/// Gets the default (empty) value.
			/// </summary>
			public static WINDOWPLACEMENT Default
			{
				get
				{
					WINDOWPLACEMENT result = new WINDOWPLACEMENT();
					result.Length = Marshal.SizeOf(result);
					return result;
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWINFO
		{
			public uint cbSize;
			public RECT rcWindow;
			public RECT rcClient;
			public uint dwStyle;
			public uint dwExStyle;
			public uint dwWindowStatus;
			public uint cxWindowBorders;
			public uint cyWindowBorders;
			public ushort atomWindowType;
			public ushort wCreatorVersion;

			public WINDOWINFO(Boolean? filler) :
				this() // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
			{
				cbSize = (UInt32) (Marshal.SizeOf(typeof(WINDOWINFO)));
			}

		}

		internal struct POINT
		{
			public FIXED x;
			public FIXED y;
		}

		internal struct FIXED
		{
			public short fract;
			public short value;
		}

		internal enum ShowWindowCommands
		{
			/// <summary>
			/// Hides the window and activates another window.
			/// </summary>
			Hide = 0,

			/// <summary>
			/// Activates and displays a window. If the window is minimized or 
			/// maximized, the system restores it to its original size and position.
			/// An application should specify this flag when displaying the window 
			/// for the first time.
			/// </summary>
			Normal = 1,

			/// <summary>
			/// Activates the window and displays it as a minimized window.
			/// </summary>
			ShowMinimized = 2,

			/// <summary>
			/// Maximizes the specified window.
			/// </summary>
			Maximize = 3, // is this the right value?

			/// <summary>
			/// Activates the window and displays it as a maximized window.
			/// </summary>       
			ShowMaximized = 3,

			/// <summary>
			/// Displays a window in its most recent size and position. This value 
			/// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
			/// the window is not activated.
			/// </summary>
			ShowNoActivate = 4,

			/// <summary>
			/// Activates the window and displays it in its current size and position. 
			/// </summary>
			Show = 5,

			/// <summary>
			/// Minimizes the specified window and activates the next top-level 
			/// window in the Z order.
			/// </summary>
			Minimize = 6,

			/// <summary>
			/// Displays the window as a minimized window. This value is similar to
			/// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
			/// window is not activated.
			/// </summary>
			ShowMinNoActive = 7,

			/// <summary>
			/// Displays the window in its current size and position. This value is 
			/// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
			/// window is not activated.
			/// </summary>
			ShowNA = 8,

			/// <summary>
			/// Activates and displays the window. If the window is minimized or 
			/// maximized, the system restores it to its original size and position. 
			/// An application should specify this flag when restoring a minimized window.
			/// </summary>
			Restore = 9,

			/// <summary>
			/// Sets the show state based on the SW_* value specified in the 
			/// STARTUPINFO structure passed to the CreateProcess function by the 
			/// program that started the application.
			/// </summary>
			ShowDefault = 10,

			/// <summary>
			///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
			/// that owns the window is not responding. This flag should only be 
			/// used when minimizing windows from a different thread.
			/// </summary>
			ForceMinimize = 11
		}

		[Flags]
		internal enum WinStyle : uint
		{
			WS_OVERLAPPED			= 0x00000000,
			WS_MAXIMIZEBOX			= 0x00010000,
			WS_MINIMIZEBOX			= 0x00020000,
			WS_THICKFRAME			= 0x00040000,
			WS_SYSMENU				= 0x00080000,
			WS_HSCROLL				= 0x00100000,
			WS_VSCROLL				= 0x00200000,
			WS_DLGFRAME				= 0x00400000,
			WS_BORDER				= 0x00800000,
			WS_MAXIMIZE				= 0x01000000,
			WS_CLIPCHILDREN			= 0x02000000,
			WS_CLIPSIBLINGS			= 0x04000000,
			WS_DISABLED				= 0x08000000,
			WS_VISIBLE				= 0x10000000,
			WS_MINIMIZE				= 0x20000000,
			WS_CHILD				= 0x40000000,
			WS_POPUP				= 0x80000000,
		}

		[Flags]
		internal enum WinStyleEx : uint
		{
			WS_EX_LEFT				= 0x00000000,
			WS_EX_DLGMODALFRAME		= 0x00000001,
			WS_EX_NOPARENTNOTIFY	= 0x00000004,
			WS_EX_TOPMOST			= 0x00000008,
			WS_EX_ACCEPTFILES		= 0x00000010,
			WS_EX_TRANSPARENT		= 0x00000020,	
			WS_EX_MDICHILD			= 0x00000040,
			WS_EX_TOOLWINDOW		= 0x00000080,
			WS_EX_WINDOWEDGE		= 0x00000100,
			WS_EX_CLIENTEDGE		= 0x00000200,
			WS_EX_CONTEXTHELP		= 0x00000400,
			WS_EX_x00000800			= 0x00000800,
			WS_EX_RIGHT				= 0x00001000,
			WS_EX_RTLREADING		= 0x00002000,
			WS_EX_LEFTSCROLLBAR		= 0x00004000,
			WS_EX_x00008000			= 0x00008000,
			WS_EX_CONTROLPARENT		= 0x00010000,
			WS_EX_STATICEDGE		= 0x00020000,
			WS_EX_APPWINDOW			= 0x00040000,
			WS_EX_LAYERED			= 0x00080000,
			WS_EX_NOINHERITLAYOUT	= 0x00100000,
			WS_EX_00200000			= 0x00200000,
			WS_EX_LAYOUTRTL			= 0x00400000,
			WS_EX_x00800000			= 0x00800000,
			WS_EX_x01000000			= 0x01000000,
			WS_EX_COMPOSITED		= 0x02000000,
			WS_EX_x04000000			= 0x04000000,
			WS_EX_NOACTIVATE		= 0x08000000,
			WS_EX_x10000000			= 0x10000000,
			WS_EX_x20000000			= 0x20000000,
			WS_EX_x40000000			= 0x40000000,
			WS_EX_x80000000			= 0x80000000
		}
	}

	internal class RevitWindow
	{
		internal int sequence;
		internal IntPtr handle;
		internal string docTitle;
		internal string winTitle;
		internal bool IsMinimized;
		internal bool IsValid = true;
		internal Rectangle current;
		internal Rectangle proposed;

		internal RevitWindow Clone()
		{
			RevitWindow rwn = new RevitWindow();
			rwn.sequence = this.sequence;
			rwn.handle = this.handle;
			rwn.docTitle = this.docTitle;
			rwn.winTitle = this.winTitle;
			rwn.IsMinimized = this.IsMinimized;
			rwn.IsValid = this.IsValid;
			rwn.current = this.current;
			rwn.proposed = this.proposed;

			return rwn;

		}
	}

//	internal static class RevitWindowExtensions
//	{
//		internal static RevitWindow Clone(this RevitWindow rw)
//		{
//			RevitWindow rwn = new RevitWindow();
//			rwn.sequence = rw.sequence;
//			rwn.handle = rw.handle;
//			rwn.docTitle = rw.docTitle;
//			rwn.winTitle = rw.winTitle;
//			rwn.IsMinimized = rw.IsMinimized;
//			rwn.IsValid = rw.IsValid;
//			rwn.current = rw.current;
//			rwn.proposed = rw.proposed;
//
//			return rwn;
//		}
//	}

}
