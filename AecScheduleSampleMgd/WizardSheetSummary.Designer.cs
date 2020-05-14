namespace AecScheduleSampleMgd
{
    partial class WizardSheetSummary
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textSummary = new System.Windows.Forms.TextBox();
            this.panelFrame = new System.Windows.Forms.Panel();
            this.panelFrame.SuspendLayout();
            this.SuspendLayout();
            // 
            // textSummary
            // 
            this.textSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textSummary.Location = new System.Drawing.Point(10, 10);
            this.textSummary.Multiline = true;
            this.textSummary.Name = "textSummary";
            this.textSummary.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textSummary.Size = new System.Drawing.Size(672, 323);
            this.textSummary.TabIndex = 0;
            // 
            // panelFrame
            // 
            this.panelFrame.Controls.Add(this.textSummary);
            this.panelFrame.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFrame.Location = new System.Drawing.Point(0, 0);
            this.panelFrame.Name = "panelFrame";
            this.panelFrame.Padding = new System.Windows.Forms.Padding(10);
            this.panelFrame.Size = new System.Drawing.Size(692, 343);
            this.panelFrame.TabIndex = 1;
            // 
            // WizardSheetSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelFrame);
            this.Name = "WizardSheetSummary";
            this.Size = new System.Drawing.Size(692, 343);
            this.Tag = "Summary|A report is here showing the details of what you are about to create.";
            this.panelFrame.ResumeLayout(false);
            this.panelFrame.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textSummary;
        private System.Windows.Forms.Panel panelFrame;
    }
}
