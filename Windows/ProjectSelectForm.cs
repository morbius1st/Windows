using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;

using static RevitWindows.Command;
using static RevitWindows.WindowUtilities;
using static RevitWindows.WindowApiUtilities;
using static RevitWindows.RevitWindow;

using static UtilityLibrary.MessageUtilities;

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
		internal static UIApplication _uiapp;
		internal static UIDocument _uidoc;
		internal static Application _app;
		internal static Document _doc;

		private static IntPtr _parent;

		private static List<string> _docNames;// = new List<string>(3);

		private const string WHERE = "@projSelForm| ";

		internal static UIApplication UiApp => _uiapp;
		internal static UIDocument UiDoc => _uidoc;
		internal static Application App => _app;
		internal static Document Doc => _doc;

		public ProjectSelectForm()
		{
//			logMsgFmtln(WHERE, "form creating form");
			InitializeComponent();
			GetDocumentList(_doc.Title);
		}

		internal string GetSelDocName => _docNames[GetSelDocIdx];

		internal int GetSelDocIdx => IndexOfDocument((string) cboSelectProject.SelectedItem);

		internal IntPtr Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		private void cboSelectProject_DropDownClosed(object sender, EventArgs e)
		{
			this.Focus();
		}

		internal int IndexOfDocument(string docTitle)
		{
			string title = docTitle.ToLower();

			for (int i = 0; i < _docNames.Count; i++)
			{
				if (title.Contains(_docNames[i]))
				{
					return i;
				}
			}

			return -1;
		}

		internal void GetDocumentList(string documentTitle)
		{
			// initialize the list of documents currently open
			DocumentSet docs = _app.Documents;

			string[] dt = new string[docs.Size];

			string currDocTitle = documentTitle.ToLower();

			int idx = 1;

			cboSelectProject.Items.Clear();

			foreach (Document d in docs)
			{
				string docTitle = d.Title.ToLower();

				cboSelectProject.Items.Add(docTitle);

				if (docTitle.Equals(currDocTitle))
				{
					dt[0] = docTitle;

					cboSelectProject.SelectedIndex =
						cboSelectProject.Items.Count - 1;

				}
				else
				{
					dt[idx++] = docTitle;
				}
			}

//			_docNames.Clear();
			_docNames = new List<string>(dt);

			this.Refresh();
		}

		internal string GetDocName(int index)
		{
			return _docNames[index];
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < _docNames.Count; i++)
			{
				sb.Append(nl);
				sb.Append(" doc name| " + _docNames[i]).Append(nl);
				sb.Append("  doc idx| " + i).Append(nl);
			}

			return sb.ToString();
		}
//
//		private void ProjectSelectForm_Activated(object sender, EventArgs e)
//		{
//			logMsgFmtln(WHERE, "form Activated");
//		}
//
//		private void ProjectSelectForm_Deactivate(object sender, EventArgs e)
//		{
//			logMsgFmtln(WHERE, "form Deactivate");
//		}
//
//		private void ProjectSelectForm_FormClosed(object sender, FormClosedEventArgs e)
//		{
//			logMsgFmtln(WHERE, "form FormClosed");
//		}
//
//		private void ProjectSelectForm_FormClosing(object sender, FormClosingEventArgs e)
//		{
//			logMsgFmtln(WHERE, "form FormClosing");
//		}
//
//		private void ProjectSelectForm_Leave(object sender, EventArgs e)
//		{
//			logMsgFmtln(WHERE, "form Leave");
//		}
//
//		private void ProjectSelectForm_Load(object sender, EventArgs e)
//		{
//			logMsgFmtln(WHERE, "form Load");
//		}
//
//		private void ProjectSelectForm_Shown(object sender, EventArgs e)
//		{
//			logMsgFmtln(WHERE, "form Shown");
//		}
//
//		private void ProjectSelectForm_VisibleChanged(object sender, EventArgs e)
//		{
//			logMsgFmtln(WHERE, "form VisibleChanged");
//		}


		private void btnExit_Click(object sender, EventArgs e)
		{
			this.Close();
			return;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			logMsgFmtln(WHERE, "*** button pressed ***");

			bool result = InitializeRevitWindows(GetSelDocIdx);

			if (!result)
			{
				this.Close();
				return;
			}

			int WindowLayoutStyle = 0;

			WindowManager winMgr =
				new WindowManager();

			winMgr.AdjustWindowLayout(WindowLayoutStyle, GetSelDocIdx);
		}


		internal int SelectDocument()
		{
			if (_docNames.Count == 1) return 0;

			DialogResult r = this.ShowDialog();
			//			this.Show();

			if (r == DialogResult.OK)
			{
				return cboSelectProject.SelectedIndex;
			}

			return -1;
		}

		private bool InitializeRevitWindows(int selDocIdx)
		{
			GetScreenMetrics(_parent);

			// get the list of child windows
			if (!GetRevitChildWindows(_parent, selDocIdx) ||
				SelectedWinCount == 0)
			{
				return false;
			}

			return true;
		}

	}
}
