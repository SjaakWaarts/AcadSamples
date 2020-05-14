namespace AecScheduleSampleMgd
{
    partial class WizardSheetPropertySetDefinition
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
            this.textPsdName = new System.Windows.Forms.TextBox();
            this.listProperties = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listObjectType = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textPsdName
            // 
            this.textPsdName.Location = new System.Drawing.Point(13, 26);
            this.textPsdName.Name = "textPsdName";
            this.textPsdName.Size = new System.Drawing.Size(667, 20);
            this.textPsdName.TabIndex = 7;
            this.textPsdName.Text = "My Property Set Definition";
            // 
            // listProperties
            // 
            this.listProperties.FormattingEnabled = true;
            this.listProperties.Location = new System.Drawing.Point(341, 76);
            this.listProperties.Name = "listProperties";
            this.listProperties.Size = new System.Drawing.Size(339, 259);
            this.listProperties.TabIndex = 9;
            this.listProperties.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listProperties_ItemCheck);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(338, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(224, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Available Properties for Selected Object Type:";
            // 
            // listObjectType
            // 
            this.listObjectType.FormattingEnabled = true;
            this.listObjectType.Location = new System.Drawing.Point(13, 76);
            this.listObjectType.Name = "listObjectType";
            this.listObjectType.Size = new System.Drawing.Size(322, 264);
            this.listObjectType.TabIndex = 6;
            this.listObjectType.SelectedIndexChanged += new System.EventHandler(this.listObjectType_SelectedIndexChanged);
            this.listObjectType.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.listObjectType_Format);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 59);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Object Type List:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(181, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Name of your Property Set Definition:";
            // 
            // WizardSheetPropertySetDefinition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textPsdName);
            this.Controls.Add(this.listProperties);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listObjectType);
            this.Controls.Add(this.label1);
            this.Name = "WizardSheetPropertySetDefinition";
            this.Size = new System.Drawing.Size(692, 343);
            this.Tag = "Property Set Definition|In this step, you define the properties for each type of " +
                "object to be scheduled.";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textPsdName;
        private System.Windows.Forms.CheckedListBox listProperties;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listObjectType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
    }
}
