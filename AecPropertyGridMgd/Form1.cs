#region Copyright
//      .NET Sample
//
//      Copyright (c) 2005 by Autodesk, Inc.
//
//      Permission to use, copy, modify, and distribute this software
//      for any purpose and without fee is hereby granted, provided
//      that the above copyright notice appears in all copies and
//      that both that copyright notice and the limited warranty and
//      restricted rights notice below appear in all supporting
//      documentation.
//
//      AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
//      AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
//      MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
//      DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
//      UNINTERRUPTED OR ERROR FREE.
//
//      Use, duplication, or disclosure by the U.S. Government is subject to
//      restrictions set forth in FAR 52.227-19 (Commercial Computer
//      Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
//      (Rights in Technical Data and Computer Software), as applicable.
//
#endregion

#region Namespaces
using System;
using System.Windows.Forms;
#endregion

public class Form1 : System.Windows.Forms.Form
{
	#region Windows Form Designer generated code
	private System.Windows.Forms.PropertyGrid PropertyGrid1;
	private System.Windows.Forms.Button Button1;
	private System.Windows.Forms.Button Button2;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public Form1()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();

		//
		// TODO: Add any constructor code after InitializeComponent call
		//
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			if (components != null) 
			{
				components.Dispose();
			}
		}
		base.Dispose( disposing );
	}

	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.PropertyGrid1 = new System.Windows.Forms.PropertyGrid();
		this.Button1 = new System.Windows.Forms.Button();
		this.Button2 = new System.Windows.Forms.Button();
		this.SuspendLayout();
		// 
		// PropertyGrid1
		// 
		this.PropertyGrid1.CommandsVisibleIfAvailable = true;
		this.PropertyGrid1.LargeButtons = false;
		this.PropertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
		this.PropertyGrid1.Name = "PropertyGrid1";
		this.PropertyGrid1.Size = new System.Drawing.Size(448, 336);
		this.PropertyGrid1.TabIndex = 0;
		this.PropertyGrid1.Text = "PropertyGrid1";
		this.PropertyGrid1.ViewBackColor = System.Drawing.SystemColors.Window;
		this.PropertyGrid1.ViewForeColor = System.Drawing.SystemColors.WindowText;
		// 
		// Button1
		// 
		this.Button1.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.Button1.Location = new System.Drawing.Point(232, 296);
		this.Button1.Name = "Button1";
		this.Button1.Size = new System.Drawing.Size(96, 24);
		this.Button1.TabIndex = 1;
		this.Button1.Text = "OK";
		// 
		// Button2
		// 
		this.Button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.Button2.Location = new System.Drawing.Point(344, 296);
		this.Button2.Name = "Button2";
		this.Button2.Size = new System.Drawing.Size(96, 24);
		this.Button2.TabIndex = 2;
		this.Button2.Text = "Cancel";
		// 
		// Form1
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(448, 334);
		this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		this.Button2,
																		this.Button1,
																		this.PropertyGrid1});
		this.Name = "Form1";
		this.Text = "Form1";
		this.ResumeLayout(false);

	}
	#endregion

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new Form1());
	}
#endregion

    #region ResetObjects
    public void ResetObjects()
    {
        this.PropertyGrid1.SelectedObject = null;
    }
    #endregion

	#region SetObjects
	public void SetObjects(Object obj)
	{
		this.PropertyGrid1.SelectedObject = obj;
		this.Text = obj.GetType().ToString();
	}
	#endregion

	#region OnResize
	override protected void OnResize(System.EventArgs e)
	{
		this.PropertyGrid1.Height = this.ClientSize.Height;
		this.PropertyGrid1.Width = this.ClientSize.Width;
		this.Button1.Top = this.ClientSize.Height - 40;
		this.Button1.Left = this.ClientSize.Width - 216;
		this.Button2.Top = this.ClientSize.Height - 40;
		this.Button2.Left = this.ClientSize.Width - 104;
	}
	#endregion
}
