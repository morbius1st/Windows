#region Namespaces

using System;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using static RevitWindows.WindowApiUtilities;
using static RevitWindows.ProjectSelectForm;

using static UtilityLibrary.MessageUtilities;

#endregion

namespace RevitWindows
{
	[Transaction(TransactionMode.Manual)]
	public class Command : IExternalCommand
	{
		private const bool AUTO_UPDATE_ON_OPEN_VIEW = true;
		private const bool AUTO_UPDATE_ON_ACTIVATE_DOCUMENT = true;


		private static MainForm _form;
		internal static ProjectSelectForm _formProjSel;

		private static bool EventsRegistered = false;

		public Result Execute(
			ExternalCommandData commandData,
			ref string message,
			ElementSet elements)
		{
			_uiapp = commandData.Application;
			_uidoc = _uiapp.ActiveUIDocument;
			_app = _uiapp.Application;
//			_doc = _uidoc.Document;

			clearConsole();

			_form = new MainForm();

			_formProjSel = new ProjectSelectForm();

			_formProjSel.Parent = GetMainWinHandle(_uidoc.Document);
			if (_formProjSel.Parent == IntPtr.Zero) { return Result.Failed; }

			_formProjSel.Show();

			if (!RegisterDocEvents()) return Result.Failed;

			return Result.Succeeded;
		}

		internal static MainForm MForm => _form;

		bool RegisterDocEvents()
		{
			if (EventsRegistered) return true;

			try
			{
//				_app.DocumentOpened += new EventHandler<DocumentOpenedEventArgs>(DocOpenEvent);
//				_app.DocumentCreated += new EventHandler<DocumentCreatedEventArgs>(DocCreateEvent);

				_uiapp.ViewActivated += ViewActivated;
				_uiapp.ApplicationClosing += AppClosing;
			}
			catch (Exception)
			{
				return false;
			}

			EventsRegistered = true;

			return true;
		}

		private void ViewActivated(object sender, ViewActivatedEventArgs args)
		{
			View vPrev = args.PreviousActiveView;
			View vCurr = args.CurrentActiveView;

			if (AUTO_UPDATE_ON_ACTIVATE_DOCUMENT &&
				!vPrev.Document.Title.ToLower().Equals(vCurr.Document.Title.ToLower()))
			{
				_formProjSel.UpdateWindowLayout();
			}
//			else if (AUTO_UPDATE_ON_OPEN_VIEW)
//			{
//				_formProjSel.UpdateWindowLayoutDelay();
//			}
		}

		private void AppClosing(object sender, ApplicationClosingEventArgs args)
		{
			_uiapp.ViewActivated -= ViewActivated;
			_uiapp.ApplicationClosing -= AppClosing;

		}

	}
}
