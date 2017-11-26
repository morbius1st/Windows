using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Application = Autodesk.Revit.ApplicationServices.Application;
using ComboBox = System.Windows.Forms.ComboBox;

namespace RevitWindows
{
	partial class ProjectSelectForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnOK = new System.Windows.Forms.Button();
			this.btnExit = new System.Windows.Forms.Button();
			this.cboWinStyle = new System.Windows.Forms.ComboBox();
			this.lblWinStyle = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatAppearance.BorderColor = System.Drawing.Color.White;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOK.Location = new System.Drawing.Point(230, 57);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(90, 30);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnExit
			// 
			this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExit.DialogResult = System.Windows.Forms.DialogResult.No;
			this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.White;
			this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnExit.Location = new System.Drawing.Point(326, 57);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(90, 30);
			this.btnExit.TabIndex = 3;
			this.btnExit.Text = "Exit";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// cboWinStyle
			// 
			this.cboWinStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboWinStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboWinStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboWinStyle.FormattingEnabled = true;
			this.cboWinStyle.Location = new System.Drawing.Point(97, 63);
			this.cboWinStyle.Name = "cboWinStyle";
			this.cboWinStyle.Size = new System.Drawing.Size(127, 21);
			this.cboWinStyle.TabIndex = 4;
			// 
			// lblWinStyle
			// 
			this.lblWinStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lblWinStyle.AutoSize = true;
			this.lblWinStyle.Location = new System.Drawing.Point(9, 66);
			this.lblWinStyle.Name = "lblWinStyle";
			this.lblWinStyle.Size = new System.Drawing.Size(72, 13);
			this.lblWinStyle.TabIndex = 5;
			this.lblWinStyle.Text = "Window Style";
			// 
			// ProjectSelectForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(428, 99);
			this.Controls.Add(this.lblWinStyle);
			this.Controls.Add(this.cboWinStyle);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.btnOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ProjectSelectForm";
			this.Text = "Select A Project";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Button btnOK;
		private Button btnExit;
		private ComboBox cboWinStyle;
		private Label lblWinStyle;
	}
}