#region Using directives

using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;

using static RevitWindows.WindowManager.WindowLayoutStyle;
using static RevitWindows.WindowManager;
using static RevitWindows.WindowManagerUtilities;

#endregion

// itemname:	RwFunctions
// username:	jeffs
// created:		12/3/2017 4:47:52 PM


namespace RevitWindows
{
	[Transaction(TransactionMode.Manual)]
	abstract class OrganizeRevitWindows : IExternalCommand
	{
		internal static WindowManager winMgr = null;

		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			if (winMgr == null)
			{
				winMgr = new WindowManager(commandData);
			}

			winMgr.CurrWinLayoutStyle = GetValue();

			if (!winMgr.UpdateWindowLayout())
			{
				return Result.Failed;
			}

			return Result.Succeeded;
		}

		protected abstract WindowLayoutStyle GetValue();
	}

	[Transaction(TransactionMode.Manual)]
	class OrganizeProperCascade : OrganizeRevitWindows
	{
		protected override WindowLayoutStyle GetValue() { return PROPER_CASCADE; }
	}

	[Transaction(TransactionMode.Manual)]
	class OrganizeWindowsCascade : OrganizeRevitWindows
	{
		protected override WindowLayoutStyle GetValue() { return WINDOWS_CASCADE ; }
	}

	[Transaction(TransactionMode.Manual)]
	class OrganizeRight : OrganizeRevitWindows
	{
		protected override WindowLayoutStyle GetValue() { return ACTIVE_RIGHT; }
	}

	[Transaction(TransactionMode.Manual)]
	class OrganizeTop : OrganizeRevitWindows
	{
		protected override WindowLayoutStyle GetValue() { return ACTIVE_TOP; }
	}

	[Transaction(TransactionMode.Manual)]
	class OrganizeLeft : OrganizeRevitWindows
	{
		protected override WindowLayoutStyle GetValue() { return ACTIVE_LEFT; }
	}

	[Transaction(TransactionMode.Manual)]
	class OrganizeBottom : OrganizeRevitWindows
	{
		protected override WindowLayoutStyle GetValue() { return ACTIVE_BOTTOM; }
	}

	[Transaction(TransactionMode.Manual)]
	class OrganizeLeftOverlapped : OrganizeRevitWindows
	{
		protected override WindowLayoutStyle GetValue() { return ACTIVE_LEFT_OVERLAP; }
	}

	abstract class ToggleAutoActivate : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			TaskDialog.Show("Organize Revit Windows", "Auto Activate| " + GetValue());

			return Result.Succeeded;
		}

		protected abstract bool GetValue();
	}

	[Transaction(TransactionMode.Manual)]
	class AutoActivateOn : ToggleAutoActivate {
		protected override bool GetValue() { return true; }
	}

	[Transaction(TransactionMode.Manual)]
	class AutoActivateOff : ToggleAutoActivate {
		protected override bool GetValue() { return false; }
	}


	[Transaction(TransactionMode.Manual)]
	class Settings : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			TaskDialog.Show("Organize Revit Windows", "Settings");
			return Result.Succeeded;
		}
	}


	abstract class AdjustSideViewSize : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			//			TaskDialog.Show("Organize Revit Windows", "Adjust Active View Size| " + GetValue());
			if (!OrganizeRevitWindows.winMgr.AdjustNonActWidth(GetValue())
				&& !OrganizeRevitWindows.winMgr.AdjustNonActHeight(GetValue()))
			{
				return Result.Cancelled;
			}
			if (!OrganizeRevitWindows.winMgr.UpdateWindowLayout())
			{
				return Result.Failed;
			}
			return Result.Succeeded;
		}

		protected abstract bool GetValue();
	}

	[Transaction(TransactionMode.Manual)]
	class  IncreaseSideViewSize : AdjustSideViewSize
	{
		protected override bool GetValue() { return true; }
	}

	[Transaction(TransactionMode.Manual)]
	class  DecreaseSideViewSize : AdjustSideViewSize
	{
		protected override bool GetValue() { return false; }
	}

}
