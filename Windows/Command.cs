#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Rectangle = System.Drawing.Rectangle;

using static Windows.WindowUtilities;
using static Windows.WindowListingUtilities;
using static Windows.WindowApiUtilities;
using static Windows.RevitWindow;

#endregion

namespace Windows
{
	[Transaction(TransactionMode.Manual)]
	public class Command : IExternalCommand
	{
		private const string MAIN_WINDOW_KEY = "Main :: Window";

		private static UIApplication _uiapp;
		private static UIDocument _uidoc;
//		internal static Application _app;
		private static Document _doc;

		private static MainForm _form;


		public Result Execute(
			ExternalCommandData commandData,
			ref string message,
			ElementSet elements)
		{
			_uiapp = commandData.Application;
			_uidoc = _uiapp.ActiveUIDocument;
//			_app = _uiapp.Application;
			_doc = _uidoc.Document;

			// must be here to insure a blank list
			ChildWindows = new List<RevitWindow>(5);
			ChildWinMinimized = new List<RevitWindow>(5);
			ChildWinOther = new List<RevitWindow>(5);

			_form = new MainForm();

			int WindowLayoutStyle = 0;

			IntPtr parent = GetMainWinHandle();
			if (parent == IntPtr.Zero) { return Result.Failed; }

			GetScreenMetrics(parent);

			// get the list of child windows
			if (!GetRevitChildWindows(parent))
			{
				return Result.Failed;
			}

			logMsgln("windows before sort");

			ListChildWindowInfo();

//			return Result.Cancelled;

			SortChildWindows();

			logMsgln("windows after sort");

			ListChildWindowInfo();

			// process and adjust the windows
			WindowManager winMgr = 
				new WindowManager(parent);
			winMgr.AdjustWindowLayout(WindowLayoutStyle);

			return Result.Succeeded;
		}

		internal static UIDocument UiDoc => _uidoc;
		internal static UIApplication UiApp => _uiapp;
		internal static Document Doc => _doc;

		internal static MainForm MForm => _form;

	}
}
