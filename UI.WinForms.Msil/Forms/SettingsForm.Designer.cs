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
            this.defaultsButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // outputLabel
            // 
            this.outputLabel.AutoSize = true;
            this.outputLabel.Location = new System.Drawing.Point(54, 15);
            this.outputLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.outputLabel.Name = "outputLabel";
            this.outputLabel.Size = new System.Drawing.Size(84, 15);
            this.outputLabel.TabIndex = 0;
            this.outputLabel.Text = "Output Folder:";
            this.outputTooltip.SetToolTip(this.outputLabel, "Base folder where all the dumps will be created as subfolders");
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.Location = new System.Drawing.Point(147, 12);
            this.outputTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.Size = new System.Drawing.Size(462, 23);
            this.outputTextBox.TabIndex = 1;
            this.outputTextBox.Text = ".\\";
            this.outputTooltip.SetToolTip(this.outputTextBox, "Base folder where all the dumps will be created as subfolders");
            this.outputTextBox.TextChanged += new System.EventHandler(this.outputTextBox_TextChanged);
            // 
            // outputBrowseButton
            // 
            this.outputBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputBrowseButton.Location = new System.Drawing.Point(617, 9);
            this.outputBrowseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outputBrowseButton.Name = "outputBrowseButton";
            this.outputBrowseButton.Size = new System.Drawing.Size(88, 27);
            this.outputBrowseButton.TabIndex = 2;
            this.outputBrowseButton.Text = "Browse...";
            this.outputBrowseButton.UseVisualStyleBackColor = true;
            this.outputBrowseButton.Click += new System.EventHandler(this.outputBrowseButton_Click);
            // 
            // outputTooltip
            // 
            this.outputTooltip.ToolTipTitle = "Output Folder";
            // 
            // namePatternExampleLabel
            // 
            this.namePatternExampleLabel.AutoSize = true;
            this.namePatternExampleLabel.Location = new System.Drawing.Point(147, 105);
            this.namePatternExampleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.namePatternExampleLabel.Name = "namePatternExampleLabel";
            this.namePatternExampleLabel.Size = new System.Drawing.Size(157, 15);
            this.namePatternExampleLabel.TabIndex = 8;
            this.namePatternExampleLabel.Text = "Weebs in Space [BLUS12345]";
            this.outputTooltip.SetToolTip(this.namePatternExampleLabel, "Example folder name for the dump");
            // 
            // labelIrd
            // 
            this.labelIrd.AutoSize = true;
            this.labelIrd.Location = new System.Drawing.Point(29, 46);
            this.labelIrd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIrd.Name = "labelIrd";
            this.labelIrd.Size = new System.Drawing.Size(100, 15);
            this.labelIrd.TabIndex = 3;
            this.labelIrd.Text = "IRD Cache Folder:";
            this.irdTooltip.SetToolTip(this.labelIrd, "Folder where IRD files will be stored for future use");
            // 
            // irdTextBox
            // 
            this.irdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.irdTextBox.Location = new System.Drawing.Point(147, 43);
            this.irdTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.irdTextBox.Name = "irdTextBox";
            this.irdTextBox.Size = new System.Drawing.Size(462, 23);
            this.irdTextBox.TabIndex = 4;
            this.irdTextBox.Text = ".\\ird";
            this.irdTooltip.SetToolTip(this.irdTextBox, "Folder where IRD files will be stored for future use");
            this.irdTextBox.TextChanged += new System.EventHandler(this.outputTextBox_TextChanged);
            // 
            // irdButton
            // 
            this.irdButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.irdButton.Location = new System.Drawing.Point(617, 40);
            this.irdButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.irdButton.Name = "irdButton";
            this.irdButton.Size = new System.Drawing.Size(88, 27);
            this.irdButton.TabIndex = 5;
            this.irdButton.Text = "Browse...";
            this.irdButton.UseVisualStyleBackColor = true;
            this.irdButton.Click += new System.EventHandler(this.irdButton_Click);
            // 
            // namePatternLabel
            // 
            this.namePatternLabel.AutoSize = true;
            this.namePatternLabel.Location = new System.Drawing.Point(16, 77);
            this.namePatternLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.namePatternLabel.Name = "namePatternLabel";
            this.namePatternLabel.Size = new System.Drawing.Size(119, 15);
            this.namePatternLabel.TabIndex = 6;
            this.namePatternLabel.Text = "Dump Name Pattern:";
            this.dumpNameTooltip.SetToolTip(this.namePatternLabel, "Pattern used to generate the folder name to store the decrypted game copy in (i.e" +
        ". game dump)");
            // 
            // namePatternTextBox
            // 
            this.namePatternTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.namePatternTextBox.Location = new System.Drawing.Point(147, 74);
            this.namePatternTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.namePatternTextBox.Name = "namePatternTextBox";
            this.namePatternTextBox.Size = new System.Drawing.Size(462, 23);
            this.namePatternTextBox.TabIndex = 7;
            this.namePatternTextBox.Text = "%title% [%product_code%]";
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
            this.groupBox1.Location = new System.Drawing.Point(14, 125);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(596, 112);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Available patterns:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(173, 83);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(130, 15);
            this.label10.TabIndex = 9;
            this.label10.Text = "- Game region (e.g. US)";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(173, 68);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(184, 15);
            this.label9.TabIndex = 8;
            this.label9.Text = "- Game title (e.g. Weebs in Space)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(173, 53);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(252, 15);
            this.label8.TabIndex = 7;
            this.label8.Text = "- Number part of the Product Code (i.e. 12345)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(173, 38);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(235, 15);
            this.label7.TabIndex = 6;
            this.label7.Text = "- Letter part of the Product Code (i.e. BLUS)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(173, 23);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(212, 15);
            this.label6.TabIndex = 5;
            this.label6.Text = "- Game Product Code (e.g. BLUS12345)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 83);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "%region%";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 68);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "%title%";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 53);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(152, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "%product_code_numbers%";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 38);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(137, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "%product_code_letters%";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 23);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "%product_code%";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(618, 210);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(86, 27);
            this.cancelButton.TabIndex = 11;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.saveButton.Location = new System.Drawing.Point(618, 177);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(88, 27);
            this.saveButton.TabIndex = 10;
            this.saveButton.Text = "OK";
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
            // defaultsButton
            // 
            this.defaultsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultsButton.Location = new System.Drawing.Point(616, 71);
            this.defaultsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.defaultsButton.Name = "defaultsButton";
            this.defaultsButton.Size = new System.Drawing.Size(88, 27);
            this.defaultsButton.TabIndex = 12;
            this.defaultsButton.Text = "Defaults";
            this.defaultsButton.UseVisualStyleBackColor = true;
            this.defaultsButton.Click += new System.EventHandler(this.defaultsButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 250);
            this.Controls.Add(this.defaultsButton);
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
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
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
        private System.Windows.Forms.Button defaultsButton;
    }
}