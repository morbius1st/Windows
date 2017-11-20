#region Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;

using UtilityLibrary;

using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.WindowListingUtilities;
using static RevitWindows.RevitWindow;
using static RevitWindows.ProjectSelectForm;

using static UtilityLibrary.MessageUtilities;

#endregion

namespace RevitWindows
{
	[Transaction(TransactionMode.Manual)]
	public class Command : IExternalCommand
	{
		private const string MAIN_WINDOW_KEY = "Main :: Window";

		private static MainForm _form;
		internal static ProjectSelectForm _formProjSel;

		internal static string SelectedDocument;


		public Result Execute(
			ExternalCommandData commandData,
			ref string message,
			ElementSet elements)
		{
			_uiapp = commandData.Application;
			_uidoc = _uiapp.ActiveUIDocument;
			_app = _uiapp.Application;
			_doc = _uidoc.Document;

			MessageUtilities.clearConsole();

			_form = new MainForm();

			logMsgFmtln("constructing form - start");
			_formProjSel = new ProjectSelectForm();
			logMsgFmtln("constructing form - end");

			_formProjSel.Parent = GetMainWinHandle();
			if (_formProjSel.Parent == IntPtr.Zero) { return Result.Failed; }

			_formProjSel.Show();

			//			logMsgFmtln("showing form - start");
			//			_formProjSel.SelectDocument();
			//			logMsgFmtln("showing form - end");
			//
			//			int selDocIdx = _formProjSel.GetSelDocIdx;
			//
			//			int WindowLayoutStyle = 0;
			//

			//
			//			GetScreenMetrics(parent);
			//
			//			// get the list of child windows
			//			if (!GetRevitChildWindows(parent, selDocIdx) || 
			//				SelectedWinCount == 0)
			//			{
			//				return Result.Failed;
			//			}
			//
			////			ListInfo();
			////			return Result.Succeeded;
			//
			//			// process and adjust the windows
			//			WindowManager winMgr = 
			//				new WindowManager();
			//
			//			winMgr.AdjustWindowLayout(WindowLayoutStyle, selDocIdx);

			RegisterDocEvents();

			return Result.Succeeded;
		}

//		internal static UIDocument UiDoc => ProjectSelectForm._uidoc;
//		internal static UIApplication UiApp => ProjectSelectForm._uiapp;
//		internal static Application App => ProjectSelectForm._app;
//		internal static Document Doc => ProjectSelectForm._doc;

		internal static MainForm MForm => _form;

//		void ListInfo()
//		{
//			logMsgln("selection made");
//			logMsgln("selected document| " + _formProjSel.GetSelDocName);
//			logMsgln("   selected index| " + _formProjSel.GetSelDocIdx);
//			logMsgln("");
//
//			logMsgln("document list");
//			ListDocuments();
//			logMsgln("");
//
//			SortChildWindows();
//			ListChildWin(ChildWindows, "child windows", 7);
//		}

		bool RegisterDocEvents()
		{
			try
			{
				_app.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(DocOpenEvent);
				_app.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(DocCreateEvent);

			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		void DocOpenEvent(object sender, DocumentOpenedEventArgs args) 
		{
			_formProjSel.GetDocumentList(args.Document.Title);
		}

		void DocCreateEvent(object sender, DocumentCreatedEventArgs args)
		{
			_formProjSel.GetDocumentList(args.Document.Title);
		}

//		void UnRegiseterDocumentOpenEvent()
//		{
//			_app.DocumentOpened -= DocOpenEvt;
//		}

	}
}
