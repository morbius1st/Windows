#region Namespaces
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

using ComboBox = Autodesk.Revit.UI.ComboBox;

#endregion

namespace RevitWindows
{
	class Ribbon : IExternalApplication
	{
		// application: launch with revit - setup interface elements
		// display information
		
		private const string PANEL_NAME = "Revit Windows";
		private const string TAB_NAME = "AO Tools";

		private static string AddInPath = typeof(Ribbon).Assembly.Location;
		private const string CLASSPATH = "RevitWindows.";

		private const string SMALLICON = "information16.png";
		private const string LARGEICON = "information32.png";

		internal UIApplication uiApp;
//		internal UIControlledApplication uiCtrlApp;

//		public static PulldownButton pb;
//		public static SplitButton sb;


		public Result OnStartup(UIControlledApplication app)
		{
			try
			{
//				uiCtrlApp = app;

				app.ControlledApplication.ApplicationInitialized += OnAppInitalized;


				// create the ribbon tab first - this is the top level
				// ui item.  below this will be the panel that is "on" the tab
				// and below this will be a pull down or split button that is "on" the panel;

				// give the tab a name;
				string tabName = TAB_NAME;
				// give the panel a name
				string panelName = PANEL_NAME;

				// first try to create the tab
				try
				{
					app.CreateRibbonTab(tabName);
				}
				catch (Exception)
				{
					// might already exist - do nothing
				}

				// tab created or exists

				// create the ribbon panel if needed
				RibbonPanel ribbonPanel = null;

				// check to see if the panel already exists
				// get the Panel within the tab name
				List<RibbonPanel> rp = new List<RibbonPanel>();

				rp = app.GetRibbonPanels(tabName);

				foreach (RibbonPanel rpx in rp)
				{
					if (rpx.Name.ToUpper().Equals(panelName.ToUpper()))
					{
						ribbonPanel = rpx;
						break;
					}
				}

				// if panel not found
				// add the panel if it does not exist
				if (ribbonPanel == null)
				{
					// create the ribbon panel on the tab given the tab's name
					// FYI - leave off the ribbon panel's name to put onto the "add-in" tab
					ribbonPanel = app.CreateRibbonPanel(tabName, panelName);
				}

				//add a split pull down button to the panel
				if (!AddStackedComboBoxes(ribbonPanel))
				{
					TaskDialog td = new TaskDialog("Revit Windows");
					td.TitleAutoPrefix = false;
					td.MainInstruction = "Failed to Add the Stacked ComboBoxes!";
					td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
					td.CommonButtons = TaskDialogCommonButtons.Ok;

					td.Show();

//					// create the split button failed
//					MessageBox.Show("Failed to Add the Stacked ComboBoxes!", "Information",
//						MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
					return Result.Failed;
				}


				//

				//				// example
				//				// add a button to the panel
				//				ribbonPanel.AddItem(
				//					createButton("ModifyPoints1", "Modify\nPoints", "ModifyPoints",
				//						"Modify the points of a topography surface", SMALLICON, LARGEICON));


				//				// example 1
				//				//add a split pull down button to the panel
				//				if (!AddPullDownButton(ribbonPanel))
				//				{
				//					// create the split button failed
				//					MessageBox.Show("Failed to Add the Pull Down Button!", "Information",
				//						MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				//					return Result.Failed;
				//				}
				//
				//				// example 2
				//				//add a stacked pair of push buttons to the panel
				//				if (!AddStackedPushButtons(ribbonPanel))
				//				{
				//					// create the split button failed
				//					MessageBox.Show("Failed to Add the Stacked Push Buttons!", "Information",
				//						MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				//					return Result.Failed;
				//				}
				//
				//				// example 3
				//				//add a stacked pair of push buttons and a text box to the panel
				//				if (!AddStackedPushButtonsAndTextBox(ribbonPanel))
				//				{
				//					// create the split button failed
				//					MessageBox.Show("Failed to Add the Stacked Push Buttons and TextBox!", "Information",
				//						MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				//					return Result.Failed;
				//				}

				return Result.Succeeded;

			}
			catch (Exception e)
			{
				Debug.WriteLine("exception " + e.Message);
				return Result.Failed;
			}

			return Result.Succeeded;
		}

		private void OnAppInitalized(object sender, ApplicationInitializedEventArgs e)
		{
			Autodesk.Revit.ApplicationServices.Application app = 
				sender as Autodesk.Revit.ApplicationServices.Application;

			uiApp = new UIApplication(app);

		}

		// add a pair of combo boxes - first is the function selection and 
		// the second is basically a check box replacement since revit
		// does not have a check box and I don't feel like making one at the moment
		private bool AddStackedComboBoxes(RibbonPanel rp)
		{
			try
			{
				ComboBoxData cbxData1 = new ComboBoxData("functions");
				ComboBoxData cbxData2 = new ComboBoxData("autoactivate");

				IList<RibbonItem> ris = rp.AddStackedItems(cbxData1, cbxData2);

				ComboBox cbx0 = ris[0] as ComboBox;
				ComboBox cbx1 = ris[1] as ComboBox;

				cbx0.ItemText = "combobox 0";
				cbx0.ToolTip = "select a function";
				cbx0.LongDescription = "select a window organize function";

				cbx1.ItemText = "combobox 1";
				cbx1.ToolTip = "toggle auto activate";
				cbx1.LongDescription = " toggle auto activate on or off";

				CreateFunctionsCbx(ref cbx0);
				CreateAutoActivateCbx(ref cbx1);

				cbx0.CurrentChanged += Cbx0_CurrentChanged;
				cbx1.CurrentChanged += Cbx1_CurrentChanged;

			}
			catch
			{
				return false;
			}
			return true;
		}

		private void Cbx1_CurrentChanged(object sender, 
			Autodesk.Revit.UI.Events.ComboBoxCurrentChangedEventArgs e)
		{
			TaskDialog.Show("Auto Activate", "this is a test " + e.NewValue.ItemText
				+ " name (" + e.NewValue.Name + ")");
		}

		private void Cbx0_CurrentChanged(object sender, 
			Autodesk.Revit.UI.Events.ComboBoxCurrentChangedEventArgs e)
		{
			TaskDialog.Show("Auto Activate", "this is a test " + e.NewValue.ItemText
				+ " name (" + e.NewValue.Name + ")");
		}

		private void CreateFunctionsCbx(ref ComboBox cbx)
		{
			cbx.AddItem(createCbxMemberData("A", "Proper Cascade", SMALLICON));
			cbx.AddItem(createCbxMemberData("B", "Window Cascade", SMALLICON));
			cbx.AddItem(createCbxMemberData("C", "Active at Right", SMALLICON));
			cbx.AddItem(createCbxMemberData("D", "Active at Top", SMALLICON));
			cbx.AddItem(createCbxMemberData("E", "Active at Left", SMALLICON));
			cbx.AddItem(createCbxMemberData("F", "Active at Bottom", SMALLICON));
		}

		private void CreateAutoActivateCbx(ref ComboBox cbx)
		{
			cbx.AddItem(createCbxMemberData("cx0", "Auto Activate On", SMALLICON));
			cbx.AddItem(createCbxMemberData("cx1", "Auto Activate Off", SMALLICON));
		}

		private ComboBoxMemberData createCbxMemberData(string internalName, 
			string visibleName, string smallIcon)
		{
			ComboBoxMemberData cbxd = new ComboBoxMemberData(internalName, visibleName);

			cbxd.Image = RibbonUtil.GetBitmapImage(smallIcon);

			return cbxd;
		}

		
//		private bool AddStackedPushButtonsAndTextBox(RibbonPanel rp)
//		{
//			TextBoxData tbd = new TextBoxData("TopoSurfaceName");
//			PushButtonData[] pbd = new PushButtonData[1];
//
//			pbd[0] = createButton("RaiseLowerPoints", "Raise\nLower\nPoints", "RaiseLowerPoints", 
//				"Raise or Lower points by a fixed amount", SMALLICON, LARGEICON);
//
//			IList<RibbonItem> ribbonItems = rp.AddStackedItems(tbd, pbd[0]);
//
//			TopoName = ribbonItems[0] as Autodesk.Revit.UI.TextBox;
//			TopoName.Value = "";
//			TopoName.ToolTip = "Current Topo Surface Name";
//			TopoName.Width = 200.0;
//			TopoName.Enabled = false;
//
//			return true;
//		}
//
//		private void SetTextBoxValue(object sender, TextBoxEnterPressedEventArgs args)
//		{
//			Units units = new Units(UnitSystem.Imperial);
//			double length = 0;
//			bool result = UnitFormatUtils.TryParse(units, UnitType.UT_Length, ElevChange.Value.ToString(), out length);
//
//			if (result)
//			{
//				elevChangeValue = length;
//
//				FormatOptions fOpt = new FormatOptions(DisplayUnitType.DUT_DECIMAL_FEET, 0.001);
//				fOpt.SuppressTrailingZeros = true;
//
//
//				FormatValueOptions opt = new FormatValueOptions();
//				opt.AppendUnitSymbol = true;
//				opt.SetFormatOptions(fOpt);
//				ElevChange.Value = UnitFormatUtils.Format(units, UnitType.UT_Length, length, false, true, opt);
//			}
//			else
//			{
//				ElevChange.Value = "invalid";
////				TaskDialog.Show("Parse", "Worked!", TaskDialogCommonButtons.Ok);
//				MessageBox.Show("Elevation Change Value", "Amount is not a real distance", MessageBoxButtons.OK, MessageBoxIcon.Error);
//			}
//		}
//
//
//		private bool AddStackedPushButtons(RibbonPanel rp)
//		{
//			PushButtonData[] pbd = new PushButtonData[2];
//
//			pbd[0] = createButton("RaisePoints2", "Raise\nPoints", "RaisePoints", 
//				"Raise points by a fixed amount", SMALLICON, LARGEICON);
//
//			pbd[1] = createButton("OffsetPoints2", "Offset\nPoints", "OffsetPoints", 
//				"Move points by a fixed amount", SMALLICON, LARGEICON);
//
//			IList<RibbonItem> ribbonItems = rp.AddStackedItems(pbd[0], pbd[1]);
//
//			return true;
//		}
//
//
//		// add a set of pull down buttons (3)
//		private bool AddPullDownButton(RibbonPanel ribbonPanel)
//		{
//			PulldownButton pb;
//
//			PulldownButtonData pdData = new PulldownButtonData("pullDownButton1", "Edit Points");
//			pdData.Image = RibbonUtil.GetBitmapImage(SMALLICON);
//
//			pb = ribbonPanel.AddItem(pdData) as PulldownButton;
//
//			PushButtonData pbd;
//
//			pbd = createButton("RaisePoints1", "Raise Points", "RaisePoints", 
//				"Raise points by a fixed amount", SMALLICON, LARGEICON);
//			pb.AddPushButton(pbd);
//
//			pbd = createButton("OffsetPoints1", "Offset Points", "OffsetPoints", 
//				"Move points by a fixed amount", SMALLICON, LARGEICON);
//			pb.AddPushButton(pbd);
//
//			return true;
//		}

//		private PushButtonData createButton(string ButtonName, string ButtonText, 
//			string className, string ToolTip, string smallIcon, string largeIcon)
//		{
//			PushButtonData pbd;
//
//			try
//			{
//				pbd = new PushButtonData(ButtonName, ButtonText, AddInPath, string.Concat(CLASSPATH, className))
//				{
//					Image = RibbonUtil.GetBitmapImage(smallIcon),
//					LargeImage = RibbonUtil.GetBitmapImage(largeIcon),
//					ToolTip = ToolTip
//				};
//			}
//			catch (Exception e)
//			{
//				return null;
//			}
//
//			return pbd;
//		}

		// process when shutting down
		public Result OnShutdown(UIControlledApplication a)
		{
			try
			{
				return Result.Succeeded;
			}
			catch (Exception e)
			{
				return Result.Failed;
			}
		}

	}
}
