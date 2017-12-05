#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion

// itemname:	RwFunctions
// username:	jeffs
// created:		12/3/2017 4:47:52 PM


namespace RevitWindows
{
	[Transaction(TransactionMode.Manual)]
	class OrganizeRevitWindows : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			string className = this.GetType().Name;

			TaskDialog.Show("Organize Revit Windows", "Class Name| " + className);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	class OrganizeProperCascade : OrganizeRevitWindows { }

	[Transaction(TransactionMode.Manual)]
	class OrganizeWindowsCascade : OrganizeRevitWindows { }

	[Transaction(TransactionMode.Manual)]
	class OrganizeRight : OrganizeRevitWindows { }

	[Transaction(TransactionMode.Manual)]
	class OrganizeTop : OrganizeRevitWindows { }

	[Transaction(TransactionMode.Manual)]
	class OrganizeLeft : OrganizeRevitWindows { }

	[Transaction(TransactionMode.Manual)]
	class OrganizeBottom : OrganizeRevitWindows { }

	[Transaction(TransactionMode.Manual)]
	class OrganizeRightOverlapped : OrganizeRevitWindows { }

	class ToggleAutoActivate : IExternalCommand
	{
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			string className = this.GetType().Name;

			TaskDialog.Show("Organize Revit Windows", "Class Name| " + className);

			return Result.Succeeded;
		}
	}

	[Transaction(TransactionMode.Manual)]
	class AutoActivateOn : ToggleAutoActivate { }

	[Transaction(TransactionMode.Manual)]
	class AutoActivateOff : ToggleAutoActivate { }

}
