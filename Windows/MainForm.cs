using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

using static Windows.Command;
using Label = System.Windows.Forms.Label;
using SystemColors = System.Windows.SystemColors;

namespace Windows
{
	public partial class MainForm : Form
	{
		internal Screen screen = null;

		// this is the "theoritical box for the form - includes 
		// a non-used area outside of the client area
		internal Rectangle ParentRectForm;
		// this is the actual usable area of the form
		internal Rectangle ParentRectClient;
		// this is the main form rectangle - from revit
		internal Rectangle RevitMainWorkArea;

		private WindowInfo[] _WiCurr;
		private WindowInfo[] _WiProp;

//		private Rectangle[] WinsCurr;
//		private Label[] LblsCurr;
//
//		private Rectangle[] WinsProp;
//		private Label[] LblsProp;

		private struct WindowInfo
		{
			internal Rectangle rect;
			internal Label label;
			internal bool ismin;
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

				return Color.FromArgb(128, colors[i]);
			}
		}

		private const int alpha = 64;

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

			screen = Screen.FromControl(this);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.Location = new Point(0, 0);
			this.Size = new Size(1920, 1200);

			label1.Text = "";
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
				sb.Append(ListChildrenCurrent(g));
			}
			else
			{
				sb.Append(ListChildrenProposed(g));
			}

			label1.Text = sb.ToString();
		}

		internal string MessageText
		{
//			get { return this.Text; }
			set { this.Text = value; }
		}


		string  ListChildrenCurrent(Graphics g)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("children windows| current|").Append(nl);

			// draw the child rectangles
			sb.Append(ListChildren(_WiCurr, g, _colorsCurr));
			return sb.ToString();
		}

		string ListChildrenProposed(Graphics g)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("children windows| proposed|").Append(nl);

			// draw the child rectangles
			sb.Append(ListChildren(_WiProp, g, _colorsProp));

			return sb.ToString();
		}

		StringBuilder ListChildren(WindowInfo[] wInfos, Graphics g, ColorChart cc)
		{
			StringBuilder sb = new StringBuilder();
			Color color = cc.GetActiveAsTransparent;

			cc.Reset();

			foreach (WindowInfo wi in wInfos)
			{
				if (wi.rect.Width == 0) { continue; }

				PlaceLabel(wi, color);
				color = cc.Next();

				sb.Append("      child rect|").Append(ListRect(wi.rect)).Append(nl);
			}

			return sb;
		}

		void PlaceLabel(WindowInfo wi, Color backColor)
		{
			Label l = wi.label;
			Rectangle r = wi.rect;

			l.Location = new Point(r.Left, r.Top);
			l.BackColor = backColor;
			l.Size = new Size(r.Width, r.Height);
			l.Visible = true;
		}

		internal void SetChildCurr(int idx, Rectangle rect, string text, bool isMinimized)
		{
			VerifySize(rect);

			if (idx >= _WiCurr.Length) { return; }
			SetChildInfo(ref _WiCurr[idx], rect, text, isMinimized);
		}

		internal void SetChildProp(int idx, Rectangle rect, string text, bool isMinimized)
		{
			VerifySize(rect);

			if (idx >= _WiProp.Length) { return; }
			SetChildInfo(ref _WiProp[idx], rect, text, isMinimized);
		}

		private void VerifySize(Rectangle r)
		{
			if (r.Right > ParentRectClient.Right)
			{
				r = r.SetRight(ParentRectClient.Right);
			}

			if (r.Bottom > ParentRectClient.Bottom)
			{
				r = r.SetBottom(ParentRectClient.Bottom);
			}
		}

		private void SetChildInfo(ref WindowInfo wi, Rectangle rect, string text, bool isMinimized)
		{
			wi.rect = rect;
			wi.label.Text = text;
			wi.ismin = isMinimized;
		}

		internal void MakeChildrenLabels(int count)
		{
			_WiCurr = new WindowInfo[count];
			_WiProp = new WindowInfo[count];

			for (int i = 0; i < count; i++)
			{
				_WiCurr[i].label = new Label();
				SetLabel(_WiCurr[i].label);
				_WiCurr[i].label.Name = $"current_{i}";

				_WiProp[i].label = new Label();
				SetLabel(_WiProp[i].label);
				_WiCurr[i].label.Name = $"proposed_{i}";

				this.Controls.Add(_WiCurr[i].label);
				this.Controls.Add(_WiProp[i].label);
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

		// make labels invisible and restore state
		void SetLabelsDefault(WindowInfo[] wInfos)
		{
			foreach (WindowInfo wi in wInfos)
			{
				SetLabel(wi.label);
			}
		}
	}
}
