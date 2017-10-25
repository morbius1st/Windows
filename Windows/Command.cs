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

#endregion

namespace Windows
{
	[Transaction(TransactionMode.Manual)]
	public class Command : IExternalCommand
	{
		private const string MAIN_WINDOW_KEY = "Main :: Window";
		
		

		internal static UIApplication _uiapp;
		internal static UIDocument _uidoc;
//		internal static Application _app;
		internal static Document _doc;

		internal static MainForm _form;

		// found child windows - active and minimized
		internal static List<RevitWindow> ActWindows;
		internal static List<RevitWindow> MinWindows;

		internal static Rectangle ParentClientWindow;

		internal static Rectangle ScreenLayout;


		public Result Execute(
			ExternalCommandData commandData,
			ref string message,
			ElementSet elements)
		{
			_uiapp = commandData.Application;
			_uidoc = _uiapp.ActiveUIDocument;
//			_app = _uiapp.Application;
			_doc = _uidoc.Document;

			ActWindows = new List<RevitWindow>(5);
			MinWindows = new List<RevitWindow>(5);

			WindowManager winMgr;

			int WindowLayoutStyle = 0;
			int titleBarHeight;

			bool result;

			_form = new MainForm();

			// get the revit process
			Process revitProcess = GetRevit();
			if (revitProcess == null) { return Result.Failed; }

			// from the process, get the parent window handle
			IntPtr parent = GetMainWinHandle(revitProcess);

			// determine the main client rectangle - the repositioned
			// view window go here
			ParentClientWindow = NewRectangle(_uiapp.DrawingAreaExtents).Adjust(-2);
			titleBarHeight = GetTitleBarHeight(parent);

			GetSystemInfo(parent, titleBarHeight);

			// get the list of child windows
			result = GetRevitChildWindows(parent);

			_form.MakeChildrenLabels(ActWindows.Count + MinWindows.Count);

			// these are just testing routines
			//			ShowInfo(revitWindows, form, parent, mainClientRect);
			//			ListAllChildWindows(parent);
			ShowInfo(parent, ParentClientWindow);


			// process and adjust the windows
			winMgr = new WindowManager(parent, ParentClientWindow, titleBarHeight);
			winMgr.AdjustWindowLayout(WindowLayoutStyle);

			return Result.Succeeded;
		}


		void ShowInfo(IntPtr parent, Rectangle mainClientRect)
		{
			// list the child windows
			ListChildWindowInfo();

			// setup the information for the form
			// show the form
			WindowUtilities.SetupForm(parent, mainClientRect);

			_form.useCurrent = true;
			_form.ShowDialog(new WindowApiUtilities.WinHandle(parent));
		}



	}
}
