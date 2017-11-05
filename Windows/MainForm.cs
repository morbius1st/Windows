using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Label = System.Windows.Forms.Label;

namespace RevitWindows
{
	public partial class MainForm : Form
	{
//		internal Screen screen = null;

		// this is the "theoritical box for the form - includes 
		// a non-used area outside of the client area
		internal Rectangle ParentRectForm;
		// this is the actual usable area of the form
		internal Rectangle ParentRectClient;
		// this is the main form rectangle - from revit
		internal Rectangle RevitMainWorkArea;

		private WindowInfo[] _WiCurr;
		private WindowInfo[] _WiProp;

		private struct WindowInfo
		{
			internal Rectangle Rect;
			internal Label Label;
		}


		private readonly Font _labelFont = new System.Drawing.Font("DejaVu Sans Mono", 8.00F, System.Drawing.FontStyle.Regular,
					System.Drawing.GraphicsUnit.Point, ((byte) (0)));

		internal struct ColorChart
		{
			private int idx;
			private Color[] colors;

			internal ColorChart(Color[] colors)
			{
				this.colors = colors;
				idx = 0;
			}

			internal Color Next()
			{
				if (++idx == colors.Length) { idx = 1; }
				return colors[idx];
			}

			internal Color NextAsTransparent()
			{
				if (++idx == colors.Length) { idx = 1; }
				return ColorTransparent(idx);
			}

			internal Color CurrentAsTransparent()
			{
				return ColorTransparent(idx);
			}

			internal void Reset()
			{
				idx = 1;
			}

			internal Color GetActive => colors[0];
			internal Color GetActiveAsTransparent => ColorTransparent(0);

			private Color ColorTransparent(int i)
			{

				return Color.FromArgb(ALPHA, colors[i]);
			}
		}

		private const int ALPHA = 64;

		internal static Color[] _colorsCurrent =
		{
			Color.FromArgb(255, 255, 118, 0),
			Color.DarkBlue,
			Color.FromArgb(255, 0, 0, 187),
			Color.Blue,
			Color.RoyalBlue,
			Color.DarkCyan,
			Color.SteelBlue,
			Color.CadetBlue,
			Color.DeepSkyBlue,
			Color.Aqua
		};

		private static Color[] _colorsProposed =
		{
			Color.FromArgb(255, 124,255,5),
			Color.Maroon, 
			Color.FromArgb(255,129,0,0),
			Color.Red,
			Color.OrangeRed,
			Color.MediumVioletRed, 
			Color.DeepPink, 
			Color.Tomato, 
			Color.IndianRed,
			Color.PaleVioletRed,
			Color.RosyBrown,
		};

		internal ColorChart _colorsCurr = new ColorChart(_colorsCurrent);
		internal ColorChart _colorsProp = new ColorChart(_colorsProposed);

		internal bool useCurrent = true;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.Location = new Point(WindowUtilities.DisplayScreenRect.Left, WindowUtilities.DisplayScreenRect.Top);
			this.Size = new Size(WindowUtilities.DisplayScreenRect.Width, WindowUtilities.DisplayScreenRect.Height);

			label1.Text = "";
		}


		private void MainForm_Activated(object sender, EventArgs e)
		{
//			this.Location = new Point(Command.ScreenLayout.Left, Command.ScreenLayout.Top);
//			this.Size = new Size(Command.ScreenLayout.Width, Command.ScreenLayout.Height);
//
//			label1.Text = "";
		}

		private void button1_Click(object sender, EventArgs e)
		{
			SetLabelsDefault(_WiCurr);
			SetLabelsDefault(_WiProp);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			StringBuilder sb = new StringBuilder();

			Graphics g = this.CreateGraphics();

			if (useCurrent)
			{
				sb.Append(ShowChildrenCurrent(g));
			}
			else
			{
				sb.Append(ShowChildrenProposed(g));
			}

			label1.Text = sb.ToString();
		}

		internal string MessageText
		{
//			get { return this.Text; }
			set { this.Text = value; }
		}

		internal void MakeChildrenLabels(int count)
		{
			_WiCurr = new WindowInfo[count];
			_WiProp = new WindowInfo[count];

			for (int i = 0; i < count; i++)
			{
				_WiCurr[i].Label = new Label();
				SetLabel(_WiCurr[i].Label);
				_WiCurr[i].Label.Name = $"current_{i}";

				_WiProp[i].Label = new Label();
				SetLabel(_WiProp[i].Label);
				_WiCurr[i].Label.Name = $"proposed_{i}";

				this.Controls.Add(_WiCurr[i].Label);
				this.Controls.Add(_WiProp[i].Label);
			}
		}

		void  SetLabel(Label l)
		{
			l.AutoSize = false;
			l.AutoEllipsis = true;
			l.BackColor = System.Drawing.SystemColors.ControlLight;
			l.Font = _labelFont;
			l.Location = Point.Empty;
			l.Text = "empty";
			l.Visible = false;
		}

		void SetLabelsDefault(WindowInfo[] wInfos)
		{
			foreach (WindowInfo wi in wInfos)
			{
				SetLabel(wi.Label);
			}
		}

		void PlaceLabel(WindowInfo wi, Color backColor)
		{
			Label l = wi.Label;
			Rectangle r = wi.Rect;

			l.Location = new Point(r.Left - WindowUtilities.DisplayScreenRect.Left, r.Top - WindowUtilities.DisplayScreenRect.Top);
			l.BackColor = backColor;
			l.Size = new Size(r.Width, r.Height);
			l.Visible = true;
		}

		internal void SetChildCurr(int idx, Rectangle rect, string text, bool moveToFront = false)
		{
			if (idx >= _WiCurr.Length) { return; }

			if (moveToFront)
			{
				this.Controls.SetChildIndex(_WiCurr[idx].Label, 0);
			}
			SetChildInfo(ref _WiCurr[idx], rect, text);
		}

		internal void SetChildProp(int idx, Rectangle rect, string text, bool moveToFront)
		{
			if (idx >= _WiProp.Length) { return; }

			if (moveToFront)
			{
				this.Controls.SetChildIndex(_WiProp[idx].Label, 0);
			}
			SetChildInfo(ref _WiProp[idx], rect, text);
		}

		private void SetChildInfo(ref WindowInfo wi, Rectangle rect, string text)
		{
			wi.Rect = rect;
			wi.Label.Text = text;
		}

		// make labels invisible and restore state

		string  ShowChildrenCurrent(Graphics g)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("children windows| current|").Append(WindowListingUtilities.nl);

			// draw the child rectangles
			sb.Append(ShowChildren(_WiCurr, g, _colorsCurr));
			return sb.ToString();
		}

		string ShowChildrenProposed(Graphics g)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("children windows| proposed|").Append(WindowListingUtilities.nl);

			// draw the child rectangles
			sb.Append(ShowChildren(_WiProp, g, _colorsProp));

			return sb.ToString();
		}

		StringBuilder ShowChildren(WindowInfo[] wInfos, Graphics g, ColorChart cc)
		{
			StringBuilder sb = new StringBuilder();
			Color color = cc.GetActiveAsTransparent;

			cc.Reset();

			foreach (WindowInfo wi in wInfos)
			{
				if (wi.Rect.Width == 0) { continue; }

				PlaceLabel(wi, color);
				color = cc.NextAsTransparent();

				sb.Append("      child rect|").Append(WindowListingUtilities.ListRect(wi.Rect)).Append(WindowListingUtilities.nl);
			}

			return sb;
		}
	}
}
