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
			this.cboSelectProject = new System.Windows.Forms.ComboBox();
			this.lblSelProject = new System.Windows.Forms.Label();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnExit = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// cboSelectProject
			// 
			this.cboSelectProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cboSelectProject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboSelectProject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cboSelectProject.FormattingEnabled = true;
			this.cboSelectProject.Location = new System.Drawing.Point(12, 40);
			this.cboSelectProject.Name = "cboSelectProject";
			this.cboSelectProject.Size = new System.Drawing.Size(284, 21);
			this.cboSelectProject.TabIndex = 0;
			this.cboSelectProject.DropDownClosed += new System.EventHandler(this.cboSelectProject_DropDownClosed);
			// 
			// lblSelProject
			// 
			this.lblSelProject.AutoSize = true;
			this.lblSelProject.Location = new System.Drawing.Point(9, 22);
			this.lblSelProject.Name = "lblSelProject";
			this.lblSelProject.Size = new System.Drawing.Size(82, 13);
			this.lblSelProject.TabIndex = 1;
			this.lblSelProject.Text = "Select a Project";
			// 
			// btnOK
			// 
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.FlatAppearance.BorderColor = System.Drawing.Color.White;
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOK.Location = new System.Drawing.Point(206, 81);
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
			this.btnExit.Location = new System.Drawing.Point(110, 81);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(90, 30);
			this.btnExit.TabIndex = 3;
			this.btnExit.Text = "Exit";
			this.btnExit.UseVisualStyleBackColor = true;
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// ProjectSelectForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(308, 123);
			this.Controls.Add(this.btnExit);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.lblSelProject);
			this.Controls.Add(this.cboSelectProject);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ProjectSelectForm";
			this.Text = "Select A Project";
//			this.Activated += new System.EventHandler(this.ProjectSelectForm_Activated);
//			this.Deactivate += new System.EventHandler(this.ProjectSelectForm_Deactivate);
//			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProjectSelectForm_FormClosing);
//			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProjectSelectForm_FormClosed);
//			this.Load += new System.EventHandler(this.ProjectSelectForm_Load);
//			this.Shown += new System.EventHandler(this.ProjectSelectForm_Shown);
//			this.VisibleChanged += new System.EventHandler(this.ProjectSelectForm_VisibleChanged);
//			this.Leave += new System.EventHandler(this.ProjectSelectForm_Leave);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ComboBox cboSelectProject;
		private Label lblSelProject;
		private Button btnOK;
		private Button btnExit;
	}
}