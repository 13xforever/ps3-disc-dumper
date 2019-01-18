namespace UI.WinForms.Msil
{
    partial class SettingsForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.outputLabel = new System.Windows.Forms.Label();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.outputBrowseButton = new System.Windows.Forms.Button();
            this.outputTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.namePatternExampleLabel = new System.Windows.Forms.Label();
            this.labelIrd = new System.Windows.Forms.Label();
            this.irdTextBox = new System.Windows.Forms.TextBox();
            this.irdButton = new System.Windows.Forms.Button();
            this.namePatternLabel = new System.Windows.Forms.Label();
            this.namePatternTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
            this.irdTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.dumpNameTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // outputLabel
            // 
            this.outputLabel.AutoSize = true;
            this.outputLabel.Location = new System.Drawing.Point(46, 13);
            this.outputLabel.Name = "outputLabel";
            this.outputLabel.Size = new System.Drawing.Size(74, 13);
            this.outputLabel.TabIndex = 0;
            this.outputLabel.Text = "Output Folder:";
            this.outputTooltip.SetToolTip(this.outputLabel, "Base folder where all the dumps will be created as subfolders");
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.Location = new System.Drawing.Point(126, 10);
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.Size = new System.Drawing.Size(397, 20);
            this.outputTextBox.TabIndex = 1;
            this.outputTextBox.Text = ".\\";
            this.outputTooltip.SetToolTip(this.outputTextBox, "Base folder where all the dumps will be created as subfolders");
            // 
            // outputBrowseButton
            // 
            this.outputBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputBrowseButton.Location = new System.Drawing.Point(529, 8);
            this.outputBrowseButton.Name = "outputBrowseButton";
            this.outputBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.outputBrowseButton.TabIndex = 2;
            this.outputBrowseButton.Text = "Browse...";
            this.outputBrowseButton.UseVisualStyleBackColor = true;
            // 
            // outputTooltip
            // 
            this.outputTooltip.ToolTipTitle = "Output Folder";
            // 
            // namePatternExampleLabel
            // 
            this.namePatternExampleLabel.AutoSize = true;
            this.namePatternExampleLabel.Location = new System.Drawing.Point(126, 91);
            this.namePatternExampleLabel.Name = "namePatternExampleLabel";
            this.namePatternExampleLabel.Size = new System.Drawing.Size(156, 13);
            this.namePatternExampleLabel.TabIndex = 8;
            this.namePatternExampleLabel.Text = "[BLUS 12345] Weebs in Space";
            this.outputTooltip.SetToolTip(this.namePatternExampleLabel, "Example folder name for the dump");
            // 
            // labelIrd
            // 
            this.labelIrd.AutoSize = true;
            this.labelIrd.Location = new System.Drawing.Point(25, 40);
            this.labelIrd.Name = "labelIrd";
            this.labelIrd.Size = new System.Drawing.Size(95, 13);
            this.labelIrd.TabIndex = 3;
            this.labelIrd.Text = "IRD Cache Folder:";
            this.irdTooltip.SetToolTip(this.labelIrd, "Folder where IRD files will be stored for future use");
            // 
            // irdTextBox
            // 
            this.irdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.irdTextBox.Location = new System.Drawing.Point(126, 37);
            this.irdTextBox.Name = "irdTextBox";
            this.irdTextBox.Size = new System.Drawing.Size(397, 20);
            this.irdTextBox.TabIndex = 4;
            this.irdTextBox.Text = ".\\ird";
            this.irdTooltip.SetToolTip(this.irdTextBox, "Folder where IRD files will be stored for future use");
            // 
            // irdButton
            // 
            this.irdButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.irdButton.Location = new System.Drawing.Point(529, 35);
            this.irdButton.Name = "irdButton";
            this.irdButton.Size = new System.Drawing.Size(75, 23);
            this.irdButton.TabIndex = 5;
            this.irdButton.Text = "Browse...";
            this.irdButton.UseVisualStyleBackColor = true;
            // 
            // namePatternLabel
            // 
            this.namePatternLabel.AutoSize = true;
            this.namePatternLabel.Location = new System.Drawing.Point(14, 67);
            this.namePatternLabel.Name = "namePatternLabel";
            this.namePatternLabel.Size = new System.Drawing.Size(106, 13);
            this.namePatternLabel.TabIndex = 6;
            this.namePatternLabel.Text = "Dump Name Pattern:";
            this.dumpNameTooltip.SetToolTip(this.namePatternLabel, "Pattern used to generate the folder name to store the decrypted game copy in (i.e" +
        ". game dump)");
            // 
            // namePatternTextBox
            // 
            this.namePatternTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.namePatternTextBox.Location = new System.Drawing.Point(126, 64);
            this.namePatternTextBox.Name = "namePatternTextBox";
            this.namePatternTextBox.Size = new System.Drawing.Size(397, 20);
            this.namePatternTextBox.TabIndex = 7;
            this.namePatternTextBox.Text = "[%product_code_letters% %product_code_numbers%] %title%";
            this.dumpNameTooltip.SetToolTip(this.namePatternTextBox, "Pattern used to generate the folder name to store the decrypted game copy in (i.e" +
        ". game dump)");
            this.namePatternTextBox.TextChanged += new System.EventHandler(this.namePatternTextBox_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 108);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(511, 96);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Available patterns:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(148, 72);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(118, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "- Game region (e.g. US)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(148, 59);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(169, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "- Game title (e.g. Weebs in Space)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(148, 46);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(225, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "- Number part of the Product Code (i.e. 12345)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(148, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(213, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "- Letter part of the Product Code (i.e. BLUS)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(148, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(197, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "- Game Product Code (e.g. BLUS12345)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "%region%";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "%title%";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(135, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "%product_code_numbers%";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "%product_code_letters%";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "%product_code%";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(530, 176);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(74, 23);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(530, 143);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 11;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // irdTooltip
            // 
            this.irdTooltip.ToolTipTitle = "IRD Cache Folder";
            // 
            // dumpNameTooltip
            // 
            this.dumpNameTooltip.ToolTipTitle = "Dump Name Pattern";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 211);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.namePatternExampleLabel);
            this.Controls.Add(this.namePatternTextBox);
            this.Controls.Add(this.namePatternLabel);
            this.Controls.Add(this.irdButton);
            this.Controls.Add(this.irdTextBox);
            this.Controls.Add(this.labelIrd);
            this.Controls.Add(this.outputBrowseButton);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.outputLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label outputLabel;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button outputBrowseButton;
        private System.Windows.Forms.ToolTip outputTooltip;
        private System.Windows.Forms.Label labelIrd;
        private System.Windows.Forms.TextBox irdTextBox;
        private System.Windows.Forms.Button irdButton;
        private System.Windows.Forms.Label namePatternLabel;
        private System.Windows.Forms.TextBox namePatternTextBox;
        private System.Windows.Forms.Label namePatternExampleLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.ToolTip irdTooltip;
        private System.Windows.Forms.ToolTip dumpNameTooltip;
    }
}