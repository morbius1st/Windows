using System;
using System.Windows.Forms;

using static RevitWindows.WindowManager;

namespace RevitWindows
{
	public partial class SettingsForm : Form
	{
		private const string FORMAT_PERCENT = "P2";
		private const string FORMAT_PIX = "D 'pix'";

		public SettingsForm()
		{
			InitializeComponent();

//			btnAutoUpdate.Text = Us.AutoUpdateGetDescription();

			tbMarginLeft.Text = Us.MarginLeft.ToString();
			tbMarginTop.Text = Us.MarginTop.ToString();
			tbMarginRight.Text = Us.MarginRight.ToString();
			tbMarginBottom.Text = Us.MarginBottom.ToString();

			tbCascadeColAdjHoriz.Text = Us.CascadeColAdjHoriz.ToString();
			tbCascadeColAdjVert.Text = Us.CascadeColAdjVert.ToString();

			tbCascadeViewMinWidthPct.Text = Us.CascadeViewMinWidthPct.ToString(FORMAT_PERCENT);
			tbCascadeViewMinHeightPct.Text = Us.CascadeViewMinHeightPct.ToString(FORMAT_PERCENT);

			tbCascadeBadViewMinWidthPct.Text = Us.CascadeBadViewMinWidthPct.ToString(FORMAT_PERCENT);
			tbCascadeBadViewMinHeightPct.Text = Us.CascadeBadViewMinHeightPct.ToString(FORMAT_PERCENT);

			tbTileMainViewMinWidthPct.Text = Us.TileMainViewMinWidthPct.ToString(FORMAT_PERCENT);
			tbTileMainViewMinHeightPct.Text = Us.TileMainViewMinHeightPct.ToString(FORMAT_PERCENT);

			tbTileSideViewWidthIncreasePct.Text = 
				Us.TileSideViewWidthIncreasePct.ToString(FORMAT_PERCENT);
			tbTileSideViewHeightIncreasePct.Text = 
				Us.TileSideViewHeightIncreasePct.ToString(FORMAT_PERCENT);

			// application buttings
			btnMinViews.Text = Us.MinViews.ToString();

			btnMarginMaxScreenPct.Text = Us.MarginMaxScreenPct.ToString(FORMAT_PERCENT);

			btnCascadeMaxColAdjPct.Text = Us.CascadeMaxColAdjPct.ToString(FORMAT_PERCENT);

			btnCascadeViewMinWidthPix.Text = Us.CascadeViewMinWidthPix.ToString(FORMAT_PIX);
			btnCascadeViewMinHeightPix.Text = Us.CascadeViewMinHeightPix.ToString(FORMAT_PIX);

			btnCascadeProperMaxViewSizePct.Text = 
				Us.CascadeProperMaxViewSizePct.ToString(FORMAT_PERCENT);
			btnCascadeBadMaxViewSizePct.Text = 
				Us.CascadeBadMaxViewSizePct.ToString(FORMAT_PERCENT);

			btnTileSideViewSizeAdjustAmtPct.Text = 
				Us.TileSideViewSizeAdjustAmtPct.ToString(FORMAT_PERCENT);

			btnTileSideViewMaxSizeIncreasPct.Text = 
				Us.TileSideViewMaxSizeIncreasePct.ToString(FORMAT_PERCENT);
			btnTileMainViewMaxSizePct.Text = Us.TileMainViewMaxSizePct.ToString(FORMAT_PERCENT);
		}

		private void btnAutoUpdate_Click(object sender, EventArgs e)
		{
//			Us.AutoUpdateFlipState();
//			btnAutoUpdate.Text = Us.AutoUpdateGetDescription();
		}

		private void hlpAutoUpdate_Click(object sender, EventArgs e)
		{
			tbInfo.Text = Properties.Resources.helpAutoUpdate;
		}
	}
}
