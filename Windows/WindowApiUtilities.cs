#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Rectangle = System.Drawing.Rectangle;

#endregion

// itemname:	WindowUtilities
// username:	jeffs
// created:		10/23/2017 6:15:42 AM


namespace RevitWindows
{
	class WindowApiUtilities
	{
		internal static Rectangle GetScreenRectFromWindow(IntPtr parent)
		{
			return GetMonitorInfo(parent).rcWorkArea.AsRectangle();
		}

		internal static MONITORINFOEX GetMonitorInfo(IntPtr parent)
		{
			IntPtr hMonitor = MonitorFromWindow(parent, 0);

			MONITORINFOEX mi = new MONITORINFOEX();
			mi.Init();
			GetMonitorInfo(hMonitor, ref mi);

			return mi;
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

		internal static IntPtr GetMainWinHandle()
		{
			Process revitProcess = GetRevit(Command.Doc);

			if (revitProcess == null) { return IntPtr.Zero; }

			IntPtr parent = revitProcess.MainWindowHandle;
			return parent;
		}

		internal static List<IntPtr> GetChildWindows(
			IntPtr parent)
		{
			List<IntPtr> result = new List<IntPtr>();
			GCHandle listHandle = GCHandle.Alloc(result);
			try
			{
				EnumWindowProc childProc = new WindowApiUtilities.EnumWindowProc(EnumWindow);
				EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
			}
			finally
			{
				if (listHandle.IsAllocated)
					listHandle.Free();
			}
			return result;
		}

		internal static Process GetRevit(Document doc)
		{
			string mainWinTitle = doc.Title.ToLower();

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


		[DllImport("user32.dll")]
		internal static extern IntPtr BeginDeferWindowPos(int nNumWindows);

		[DllImport("user32.dll")]
		internal static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd, 
			IntPtr hWndInsertAfter, int x, int y, int cx, int cy, DeferWinPos uFlags);

		[DllImport("user32.dll")]
		internal static extern IntPtr DeferWindowPos(IntPtr hWinPosInfo, IntPtr hWnd, 
			int x, int y, int cx, int cy, DeferWinPos uFlags);

		[DllImport("user32.dll")]
		internal static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsChild(IntPtr hWndParent, IntPtr hWndChild);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsZoomed(IntPtr hWnd);

		[DllImport("user32.dll")]
		internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool ShowWindow(IntPtr hWnd, ShowWinCmds nCmdShow);

		[DllImport("user32.dll")]
		internal static extern int GetDpiForWindow(IntPtr parent);

		[DllImport("user32.dll")]
		internal static extern int GetSystemMetrics(SystemMetric smIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		internal static extern IntPtr GetParent(IntPtr hWnd);

		[DllImport("user32.dll")]
		internal static extern IntPtr GetTopWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetWindowInfo(IntPtr hWnd, ref WINDOWINFO lpwndpl);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetTitleBarInfo(IntPtr hwnd, ref TITLEBARINFO pti);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern int GetWindowText(
			IntPtr hWnd, [Out] StringBuilder lpString,
			int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern int GetWindowTextLength(
			IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetWindowRect(
			IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", EntryPoint = "GetClassName")]
		internal static extern int GetClass(
			IntPtr hWnd, StringBuilder className, int nMaxCount);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool EnumChildWindows(
			IntPtr window, EnumWindowProc callback, IntPtr i);

		internal delegate bool EnumWindowProc(
			IntPtr hWnd, IntPtr parameter);

		internal static bool EnumWindow(
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
		internal struct TITLEBARINFO
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
			public int Length;
			public int Flags;
			public ShowWinCmds ShowCmd;
			public POINT MinPosition;
			public POINT MaxPosition;
			public RECT NormalPosition;
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

		// size of a device name string
		private const int CCHDEVICENAME = 32;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		internal struct MONITORINFOEX
		{
			public uint cbSize;
			public RECT rcMonitor;
			public RECT rcWorkArea;
			public dwFlags Flags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
			public string DeviceName;

			public void Init()
			{
				this.cbSize = 40 + 2 * CCHDEVICENAME;
				this.DeviceName = String.Empty;
			}
		}

		internal enum dwFlags : uint
		{
			MONITORINFO_PRIMARY = 1
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

		internal static IntPtr HWND_BOTTOM		= (IntPtr) 1;
		internal static IntPtr HWND_NOTOPMOST	= (IntPtr) (0 - 2);
		internal static IntPtr HWND_TOP			= (IntPtr) 0;
		internal static IntPtr HWND_TOPMOST		= (IntPtr) (0 - 1);


		[Flags]
		internal enum DeferWinPos : uint
		{
			SWP_NOSIZE			= 0x0001,
			SWP_NOMOVE			= 0x0002,
			SWP_NOZORDER		= 0x0004,
			SWP_NOREDRAW		= 0x0008,
			SWP_NOACTIVATE		= 0x0010,
			SWP_DRAWFRAME		= 0x0020,
			SWP_FRAMECHANGED	= 0x0020,
			SWP_SHOWWINDOW		= 0x0040,
			SWP_HIDEWINDOW		= 0x0080,
			SWP_NOCOPYBITS		= 0x0100,
			SWP_NOOWNERZORDER	= 0x0200,
			SWP_NOREPOSITION	= 0x0200,
			SWP_NOSENDCHANGING	= 0x0400,
		}

		internal enum ShowWinCmds
		{
			SW_HIDE				= 0,
			SW_SHOWNORMAL		= 1,
			SW_SHOWMINIMIZED	= 2,
			SW_MAXIMIZE			= 3, // is this the right value?
			SW_SHOWMAXIMIZED	= 3,
			SW_SHOWNOACTIVATE	= 4,
			SW_SHOW				= 5,
			SW_MINIMIZE			= 6,
			SW_SHOWMINNOACTIVE	= 7,
			SW_SHOWNA			= 8,
			SW_RESTORE			= 9,
			SW_SHOWDEFAULT		= 10,
			SW_FORCEMINIMIZE	= 11
		}

		[Flags]
		internal enum WinStyle : uint
		{
			WS_OVERLAPPED = 0x00000000,
			WS_MAXIMIZEBOX = 0x00010000,
			WS_MINIMIZEBOX = 0x00020000,
			WS_THICKFRAME = 0x00040000,
			WS_SYSMENU = 0x00080000,
			WS_HSCROLL = 0x00100000,
			WS_VSCROLL = 0x00200000,
			WS_DLGFRAME = 0x00400000,
			WS_BORDER = 0x00800000,
			WS_MAXIMIZE = 0x01000000,
			WS_CLIPCHILDREN = 0x02000000,
			WS_CLIPSIBLINGS = 0x04000000,
			WS_DISABLED = 0x08000000,
			WS_VISIBLE = 0x10000000,
			WS_MINIMIZE = 0x20000000,
			WS_CHILD = 0x40000000,
			WS_POPUP = 0x80000000,
		}

		[Flags]
		internal enum WinStyleEx : uint
		{
			WS_EX_LEFT = 0x00000000,
			WS_EX_DLGMODALFRAME = 0x00000001,
			WS_EX_NOPARENTNOTIFY = 0x00000004,
			WS_EX_TOPMOST = 0x00000008,
			WS_EX_ACCEPTFILES = 0x00000010,
			WS_EX_TRANSPARENT = 0x00000020,
			WS_EX_MDICHILD = 0x00000040,
			WS_EX_TOOLWINDOW = 0x00000080,
			WS_EX_WINDOWEDGE = 0x00000100,
			WS_EX_CLIENTEDGE = 0x00000200,
			WS_EX_CONTEXTHELP = 0x00000400,
			WS_EX_X00000800 = 0x00000800,
			WS_EX_RIGHT = 0x00001000,
			WS_EX_RTLREADING = 0x00002000,
			WS_EX_LEFTSCROLLBAR = 0x00004000,
			WS_EX_X00008000 = 0x00008000,
			WS_EX_CONTROLPARENT = 0x00010000,
			WS_EX_STATICEDGE = 0x00020000,
			WS_EX_APPWINDOW = 0x00040000,
			WS_EX_LAYERED = 0x00080000,
			WS_EX_NOINHERITLAYOUT = 0x00100000,
			WS_EX_00200000 = 0x00200000,
			WS_EX_LAYOUTRTL = 0x00400000,
			WS_EX_X00800000 = 0x00800000,
			WS_EX_X01000000 = 0x01000000,
			WS_EX_COMPOSITED = 0x02000000,
			WS_EX_X04000000 = 0x04000000,
			WS_EX_NOACTIVATE = 0x08000000,
			WS_EX_X10000000 = 0x10000000,
			WS_EX_X20000000 = 0x20000000,
			WS_EX_X40000000 = 0x40000000,
			WS_EX_X80000000 = 0x80000000
		}

		public enum SystemMetric
		{
			SM_CXSCREEN = 0,  // 0x00
			SM_CYSCREEN = 1,  // 0x01
			SM_CXVSCROLL = 2,  // 0x02
			SM_CYHSCROLL = 3,  // 0x03
			SM_CYCAPTION = 4,  // 0x04
			SM_CXBORDER = 5,  // 0x05
			SM_CYBORDER = 6,  // 0x06
			SM_CXDLGFRAME = 7,  // 0x07
			SM_CXFIXEDFRAME = 7,  // 0x07
			SM_CYDLGFRAME = 8,  // 0x08
			SM_CYFIXEDFRAME = 8,  // 0x08
			SM_CYVTHUMB = 9,  // 0x09
			SM_CXHTHUMB = 10, // 0x0A
			SM_CXICON = 11, // 0x0B
			SM_CYICON = 12, // 0x0C
			SM_CXCURSOR = 13, // 0x0D
			SM_CYCURSOR = 14, // 0x0E
			SM_CYMENU = 15, // 0x0F
			SM_CXFULLSCREEN = 16, // 0x10
			SM_CYFULLSCREEN = 17, // 0x11
			SM_CYKANJIWINDOW = 18, // 0x12
			SM_MOUSEPRESENT = 19, // 0x13
			SM_CYVSCROLL = 20, // 0x14
			SM_CXHSCROLL = 21, // 0x15
			SM_DEBUG = 22, // 0x16
			SM_SWAPBUTTON = 23, // 0x17
			SM_CXMIN = 28, // 0x1C
			SM_CYMIN = 29, // 0x1D
			SM_CXSIZE = 30, // 0x1E
			SM_CYSIZE = 31, // 0x1F
			SM_CXSIZEFRAME = 32, // 0x20
			SM_CXFRAME = 32, // 0x20
			SM_CYSIZEFRAME = 33, // 0x21
			SM_CYFRAME = 33, // 0x21
			SM_CXMINTRACK = 34, // 0x22
			SM_CYMINTRACK = 35, // 0x23
			SM_CXDOUBLECLK = 36, // 0x24
			SM_CYDOUBLECLK = 37, // 0x25
			SM_CXICONSPACING = 38, // 0x26
			SM_CYICONSPACING = 39, // 0x27
			SM_MENUDROPALIGNMENT = 40, // 0x28
			SM_PENWINDOWS = 41, // 0x29
			SM_DBCSENABLED = 42, // 0x2A
			SM_CMOUSEBUTTONS = 43, // 0x2B
			SM_SECURE = 44, // 0x2C
			SM_CXEDGE = 45, // 0x2D
			SM_CYEDGE = 46, // 0x2E
			SM_CXMINSPACING = 47, // 0x2F
			SM_CYMINSPACING = 48, // 0x30
			SM_CXSMICON = 49, // 0x31
			SM_CYSMICON = 50, // 0x32
			SM_CYSMCAPTION = 51, // 0x33
			SM_CXSMSIZE = 52, // 0x34
			SM_CYSMSIZE = 53, // 0x35
			SM_CXMENUSIZE = 54, // 0x36
			SM_CYMENUSIZE = 55, // 0x37
			SM_ARRANGE = 56, // 0x38
			SM_CXMINIMIZED = 57, // 0x39
			SM_CYMINIMIZED = 58, // 0x3A
			SM_CXMAXTRACK = 59, // 0x3B
			SM_CYMAXTRACK = 60, // 0x3C
			SM_CXMAXIMIZED = 61, // 0x3D
			SM_CYMAXIMIZED = 62, // 0x3E
			SM_NETWORK = 63, // 0x3F
			SM_CLEANBOOT = 67, // 0x43
			SM_CXDRAG = 68, // 0x44
			SM_CYDRAG = 69, // 0x45
			SM_SHOWSOUNDS = 70, // 0x46
			SM_CXMENUCHECK = 71, // 0x47
			SM_CYMENUCHECK = 72, // 0x48
			SM_SLOWMACHINE = 73, // 0x49
			SM_MIDEASTENABLED = 74, // 0x4A
			SM_MOUSEWHEELPRESENT = 75, // 0x4B
			SM_XVIRTUALSCREEN = 76, // 0x4C
			SM_YVIRTUALSCREEN = 77, // 0x4D
			SM_CXVIRTUALSCREEN = 78, // 0x4E
			SM_CYVIRTUALSCREEN = 79, // 0x4F
			SM_CMONITORS = 80, // 0x50
			SM_SAMEDISPLAYFORMAT = 81, // 0x51
			SM_IMMENABLED = 82, // 0x52
			SM_CXFOCUSBORDER = 83, // 0x53
			SM_CYFOCUSBORDER = 84, // 0x54
			SM_TABLETPC = 86, // 0x56
			SM_MEDIACENTER = 87, // 0x57
			SM_STARTER = 88, // 0x58
			SM_SERVERR2 = 89, // 0x59
			SM_MOUSEHORIZONTALWHEELPRESENT = 91, // 0x5B
			SM_CXPADDEDBORDER = 92, // 0x5C
			SM_DIGITIZER = 94, // 0x5E
			SM_MAXIMUMTOUCHES = 95, // 0x5F

			SM_REMOTESESSION = 0x1000, // 0x1000
			SM_SHUTTINGDOWN = 0x2000, // 0x2000
			SM_REMOTECONTROL = 0x2001, // 0x2001


			SM_CONVERTABLESLATEMODE = 0x2003,
			SM_SYSTEMDOCKED = 0x2004,
		}


		void ListTitleBarInfo(IntPtr win)
		{
			WindowApiUtilities.TITLEBARINFO ti = new WindowApiUtilities.TITLEBARINFO();
			ti.cbSize = (uint) Marshal.SizeOf(typeof(WindowApiUtilities.TITLEBARINFO));

			WindowUtilities.logMsgln("Title bar info");
			WindowUtilities.logMsgln("Title bar rect| " + WindowListingUtilities.ListRect(ti.rcTitleBar));
			WindowUtilities.logMsgln("Title bar ht  | " + (ti.rcTitleBar.Bottom - ti.rcTitleBar.Top));

		}


		internal static int _WinStyleCount = Enum.GetNames(typeof(WindowApiUtilities.WinStyle)).Length;
		internal static int _WinStyleExCount = Enum.GetNames(typeof(WindowApiUtilities.WinStyleEx)).Length;

		// short term - one item
		internal static int[] _WinStyleData = new int[_WinStyleCount];
		internal static int[] _WinStyleExData = new int[_WinStyleExCount];

		// long term - all items
		internal static int[] _WinStyleRecord = new int[_WinStyleCount];
		internal static int[] _WinStyleExRecord = new int[_WinStyleExCount];

		internal static void InitWinStyleRecords()
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


		internal static void AnalyzeWinStyles<T>(uint style, ref int[] record, ref int[] data,
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
					if (result.Length > 0) { result += " :: "; }
					result += Enum.GetName(typeof(T), ws);
				}

				idx++;
			}
		}

		internal static void ListWinStyleResult(int[] StyleRecord)
		{
			foreach (int s in StyleRecord)
			{
				if (s > 0)
				{
					WindowUtilities.logMsg($"| {s:D4} ");
				}
				else
				{
					WindowUtilities.logMsg("|      ");
				}
			}
			WindowUtilities.logMsgln("|");
		}

		internal static void ListWinStyleNames<T>()
		{
			string[] names = Enum.GetNames(typeof(T));

			int offset = 5;
			int num = ((names.Length + offset - 1) / offset) * offset;
			string patt;


			for (int j = 0; j < offset; j++)
			{
				patt = String.Format("{{0,{0}}}", (78 + 7 * j));

				WindowUtilities.logMsg(String.Format(patt, ">  "));
				for (int i = j; i < num; i += offset)
				{
					if (i >= names.Length) { continue; }
					WindowUtilities.logMsg($"{names[i],-35}");
				}

				WindowUtilities.logMsg(WindowListingUtilities.nl);
			}
		}
	}
}
