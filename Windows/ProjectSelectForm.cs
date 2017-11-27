using System;
using System.Collections.Generic;
using Form = System.Windows.Forms.Form;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;

using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowListingUtilities;
using static RevitWindows.RevitWindow;

//using static UtilityLibrary.MessageUtilities;

/* form creation process
 * constructor / create & initilize the form
 * load
 * visible changed
 * activated
 * shown
 * deactivate [lose focus]
 * activated [get focus]
 * @ dialog form
 * *** ok button
 * form closing
 * form closed
 * deactivate
 * visible changed
 */


namespace RevitWindows
{
	public partial class ProjectSelectForm : Form
	{
		internal static UIApplication Uiapp;
		internal static UIDocument Uidoc;
		internal static Application App;
		internal static Document Doc;

		private static IntPtr _parent;

		private const string WHERE = "@projSelForm| ";

		private readonly List<string> _winStyles = new List<string>()
		{
			"Proper Cascade", 
			"Window's Cascade",
			"At Left", 
			"At Bottom", 
			"At Right", 
			"At Top",
			"At Left - Overlapped"
		};

		public ProjectSelectForm()
		{
			InitializeComponent();

			foreach (string s in _winStyles)
			{
				cboWinStyle.Items.Add(s);
			}

			cboWinStyle.SelectedIndex = 0;
		}

		internal IntPtr Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		private void cboSelectProject_DropDownClosed(object sender, EventArgs e)
		{
			this.Focus();
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			this.Close();
			return;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			UpdateWindowLayout();

		}

		internal void UpdateWindowLayout()
		{
			Uidoc = Uiapp.ActiveUIDocument;
			App = Uiapp.Application;
			Doc = Uidoc.Document;

			bool result = InitializeRevitWindows();

			SortChildWindows();

			ListChildWin(ChildWindows, nl + "child windows after initalize",
				1, 3, 4, 5, 6, 7, 10, 11, 12);

			result = false;

			if (!result)
			{
				this.Close();
				return;
			}

			int windowLayoutStyle =
				_winStyles.IndexOf((string) cboWinStyle.SelectedItem);

			WindowManager winMgr =
				new WindowManager();

			if (!winMgr.AdjustWindowLayout(windowLayoutStyle))
			{
//				this.Close();
				return;
			}
		}

		private bool InitializeRevitWindows()
		{
			GetScreenMetrics(_parent);

			// get the list of child windows
			if (!GetRevitChildWindows(_parent) ||
				CurrDocWinCount == 0)
			{
				return false;
			}

			return true;
		}

	}
}
