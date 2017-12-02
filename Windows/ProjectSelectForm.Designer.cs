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
			this.lblStatus = new System.Windows.Forms.Label();
			this.lblMessasge = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnOK
			// 
			this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatAppearance.BorderColor = System.Drawing.Color.White;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOK.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnOK.Location = new System.Drawing.Point(230, 22);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(90, 30);
			this.btnOK.TabIndex = 2;
			this.btnOK.Text = "OK";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnExit
			// 
			this.btnExit.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnExit.DialogResult = System.Windows.Forms.DialogResult.No;
			this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.White;
			this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnExit.Location = new System.Drawing.Point(326, 22);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(90, 30);
			this.btnExit.TabIndex = 3;
			this.btnExit.Text = "Exit";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// cboWinStyle
			// 
			this.cboWinStyle.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.cboWinStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboWinStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboWinStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cboWinStyle.FormattingEnabled = true;
			this.cboWinStyle.Location = new System.Drawing.Point(97, 28);
			this.cboWinStyle.Name = "cboWinStyle";
			this.cboWinStyle.Size = new System.Drawing.Size(127, 23);
			this.cboWinStyle.TabIndex = 4;
			// 
			// lblWinStyle
			// 
			this.lblWinStyle.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.lblWinStyle.AutoSize = true;
			this.lblWinStyle.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWinStyle.Location = new System.Drawing.Point(9, 31);
			this.lblWinStyle.Name = "lblWinStyle";
			this.lblWinStyle.Size = new System.Drawing.Size(79, 15);
			this.lblWinStyle.TabIndex = 5;
			this.lblWinStyle.Text = "Window Style";
			// 
			// lblStatus
			// 
			this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblStatus.Location = new System.Drawing.Point(9, 136);
			this.lblStatus.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(407, 14);
			this.lblStatus.TabIndex = 6;
			// 
			// lblMessasge
			// 
			this.lblMessasge.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblMessasge.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblMessasge.Location = new System.Drawing.Point(9, 105);
			this.lblMessasge.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.lblMessasge.Name = "lblMessasge";
			this.lblMessasge.Size = new System.Drawing.Size(407, 27);
			this.lblMessasge.TabIndex = 7;
			this.lblMessasge.Click += new System.EventHandler(this.lblMessasge_Click);
			// 
			// ProjectSelectForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(428, 159);
			this.Controls.Add(this.lblMessasge);
			this.Controls.Add(this.lblStatus);
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
		private Label lblStatus;
		private Label lblMessasge;
	}
}