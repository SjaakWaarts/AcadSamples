namespace AecScheduleSampleMgd
{
    partial class WizardSheetScheduleTableStyle
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
            this.textStyleName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.treeColumnHeader = new System.Windows.Forms.TreeView();
            this.btnAddColumn = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAddHeader = new System.Windows.Forms.Button();
            this.treeProperties = new System.Windows.Forms.TreeView();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textStyleName
            // 
            this.textStyleName.Location = new System.Drawing.Point(13, 26);
            this.textStyleName.Name = "textStyleName";
            this.textStyleName.Size = new System.Drawing.Size(665, 20);
            this.textStyleName.TabIndex = 1;
            this.textStyleName.Text = "My Schedule Table Style";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Column Header:";
            // 
            // treeColumnHeader
            // 
            this.treeColumnHeader.HideSelection = false;
            this.treeColumnHeader.LabelEdit = true;
            this.treeColumnHeader.Location = new System.Drawing.Point(13, 76);
            this.treeColumnHeader.Name = "treeColumnHeader";
            this.treeColumnHeader.Size = new System.Drawing.Size(255, 264);
            this.treeColumnHeader.TabIndex = 3;
            this.treeColumnHeader.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeColumnHeader_AfterLabelEdit);
            this.treeColumnHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeColumnHeader_MouseDown);
            this.treeColumnHeader.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.treeColumnHeader_BeforeLabelEdit);
            // 
            // btnAddColumn
            // 
            this.btnAddColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddColumn.Location = new System.Drawing.Point(274, 76);
            this.btnAddColumn.Name = "btnAddColumn";
            this.btnAddColumn.Size = new System.Drawing.Size(109, 23);
            this.btnAddColumn.TabIndex = 4;
            this.btnAddColumn.Text = "<< Add Column";
            this.btnAddColumn.UseVisualStyleBackColor = true;
            this.btnAddColumn.Click += new System.EventHandler(this.btnAddColumn_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.Location = new System.Drawing.Point(274, 149);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(109, 23);
            this.btnRemove.TabIndex = 5;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAddHeader
            // 
            this.btnAddHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddHeader.Location = new System.Drawing.Point(274, 120);
            this.btnAddHeader.Name = "btnAddHeader";
            this.btnAddHeader.Size = new System.Drawing.Size(109, 23);
            this.btnAddHeader.TabIndex = 6;
            this.btnAddHeader.Text = "Add Header";
            this.btnAddHeader.UseVisualStyleBackColor = true;
            this.btnAddHeader.Click += new System.EventHandler(this.btnAddHeader_Click);
            // 
            // treeProperties
            // 
            this.treeProperties.HideSelection = false;
            this.treeProperties.Location = new System.Drawing.Point(395, 76);
            this.treeProperties.Name = "treeProperties";
            this.treeProperties.Size = new System.Drawing.Size(283, 264);
            this.treeProperties.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(392, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Available Properties:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Schedule Table Style Name:";
            // 
            // WizardSheetScheduleTableStyle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAddHeader);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnAddColumn);
            this.Controls.Add(this.textStyleName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.treeProperties);
            this.Controls.Add(this.treeColumnHeader);
            this.Name = "WizardSheetScheduleTableStyle";
            this.Size = new System.Drawing.Size(692, 343);
            this.Tag = "Schedule Table Style|In this step, you define the way that columns are displayed " +
                "in the schedule table.";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textStyleName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TreeView treeColumnHeader;
        private System.Windows.Forms.Button btnAddColumn;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAddHeader;
        private System.Windows.Forms.TreeView treeProperties;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
    }
}
