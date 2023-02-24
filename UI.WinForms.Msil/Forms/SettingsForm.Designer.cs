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
            components = new System.ComponentModel.Container();
            outputLabel = new System.Windows.Forms.Label();
            outputTextBox = new System.Windows.Forms.TextBox();
            outputBrowseButton = new System.Windows.Forms.Button();
            outputTooltip = new System.Windows.Forms.ToolTip(components);
            namePatternExampleLabel = new System.Windows.Forms.Label();
            labelIrd = new System.Windows.Forms.Label();
            irdTextBox = new System.Windows.Forms.TextBox();
            irdButton = new System.Windows.Forms.Button();
            namePatternLabel = new System.Windows.Forms.Label();
            namePatternTextBox = new System.Windows.Forms.TextBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label10 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            cancelButton = new System.Windows.Forms.Button();
            saveButton = new System.Windows.Forms.Button();
            irdTooltip = new System.Windows.Forms.ToolTip(components);
            dumpNameTooltip = new System.Windows.Forms.ToolTip(components);
            defaultsButton = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // outputLabel
            // 
            outputLabel.AutoSize = true;
            outputLabel.Location = new System.Drawing.Point(54, 15);
            outputLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            outputLabel.Name = "outputLabel";
            outputLabel.Size = new System.Drawing.Size(84, 15);
            outputLabel.TabIndex = 0;
            outputLabel.Text = "Output Folder:";
            outputTooltip.SetToolTip(outputLabel, "Base folder where all the dumps will be created as subfolders");
            // 
            // outputTextBox
            // 
            outputTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            outputTextBox.Location = new System.Drawing.Point(147, 12);
            outputTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            outputTextBox.Name = "outputTextBox";
            outputTextBox.Size = new System.Drawing.Size(462, 23);
            outputTextBox.TabIndex = 1;
            outputTextBox.Text = ".\\";
            outputTooltip.SetToolTip(outputTextBox, "Base folder where all the dumps will be created as subfolders");
            outputTextBox.TextChanged += outputTextBox_TextChanged;
            // 
            // outputBrowseButton
            // 
            outputBrowseButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            outputBrowseButton.Location = new System.Drawing.Point(617, 9);
            outputBrowseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            outputBrowseButton.Name = "outputBrowseButton";
            outputBrowseButton.Size = new System.Drawing.Size(88, 27);
            outputBrowseButton.TabIndex = 2;
            outputBrowseButton.Text = "Browse...";
            outputBrowseButton.UseVisualStyleBackColor = true;
            outputBrowseButton.Click += outputBrowseButton_Click;
            // 
            // outputTooltip
            // 
            outputTooltip.ToolTipTitle = "Output Folder";
            // 
            // namePatternExampleLabel
            // 
            namePatternExampleLabel.AutoSize = true;
            namePatternExampleLabel.Location = new System.Drawing.Point(147, 105);
            namePatternExampleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            namePatternExampleLabel.Name = "namePatternExampleLabel";
            namePatternExampleLabel.Size = new System.Drawing.Size(247, 15);
            namePatternExampleLabel.TabIndex = 8;
            namePatternExampleLabel.Text = "My PS3 Game Can't Be This Cute [BLUS12345]";
            outputTooltip.SetToolTip(namePatternExampleLabel, "Example folder name for the dump");
            // 
            // labelIrd
            // 
            labelIrd.AutoSize = true;
            labelIrd.Location = new System.Drawing.Point(29, 46);
            labelIrd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelIrd.Name = "labelIrd";
            labelIrd.Size = new System.Drawing.Size(100, 15);
            labelIrd.TabIndex = 3;
            labelIrd.Text = "IRD Cache Folder:";
            irdTooltip.SetToolTip(labelIrd, "Folder where IRD files will be stored for future use");
            // 
            // irdTextBox
            // 
            irdTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            irdTextBox.Location = new System.Drawing.Point(147, 43);
            irdTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            irdTextBox.Name = "irdTextBox";
            irdTextBox.Size = new System.Drawing.Size(462, 23);
            irdTextBox.TabIndex = 4;
            irdTextBox.Text = ".\\ird";
            irdTooltip.SetToolTip(irdTextBox, "Folder where IRD files will be stored for future use");
            irdTextBox.TextChanged += outputTextBox_TextChanged;
            // 
            // irdButton
            // 
            irdButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            irdButton.Location = new System.Drawing.Point(617, 40);
            irdButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            irdButton.Name = "irdButton";
            irdButton.Size = new System.Drawing.Size(88, 27);
            irdButton.TabIndex = 5;
            irdButton.Text = "Browse...";
            irdButton.UseVisualStyleBackColor = true;
            irdButton.Click += irdButton_Click;
            // 
            // namePatternLabel
            // 
            namePatternLabel.AutoSize = true;
            namePatternLabel.Location = new System.Drawing.Point(16, 77);
            namePatternLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            namePatternLabel.Name = "namePatternLabel";
            namePatternLabel.Size = new System.Drawing.Size(119, 15);
            namePatternLabel.TabIndex = 6;
            namePatternLabel.Text = "Dump Name Pattern:";
            dumpNameTooltip.SetToolTip(namePatternLabel, "Pattern used to generate the folder name to store the decrypted game copy in (i.e. game dump)");
            // 
            // namePatternTextBox
            // 
            namePatternTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            namePatternTextBox.Location = new System.Drawing.Point(147, 74);
            namePatternTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            namePatternTextBox.Name = "namePatternTextBox";
            namePatternTextBox.Size = new System.Drawing.Size(462, 23);
            namePatternTextBox.TabIndex = 7;
            namePatternTextBox.Text = "%title% [%product_code%]";
            dumpNameTooltip.SetToolTip(namePatternTextBox, "Pattern used to generate the folder name to store the decrypted game copy in (i.e. game dump)");
            namePatternTextBox.TextChanged += namePatternTextBox_TextChanged;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new System.Drawing.Point(14, 125);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(596, 112);
            groupBox1.TabIndex = 9;
            groupBox1.TabStop = false;
            groupBox1.Text = "Available patterns:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(173, 83);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(130, 15);
            label10.TabIndex = 9;
            label10.Text = "- Game region (e.g. US)";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(173, 68);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(189, 15);
            label9.TabIndex = 8;
            label9.Text = "- Game title (e.g. Attack on Wallet)";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(173, 53);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(252, 15);
            label8.TabIndex = 7;
            label8.Text = "- Number part of the Product Code (i.e. 12345)";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(173, 38);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(235, 15);
            label7.TabIndex = 6;
            label7.Text = "- Letter part of the Product Code (i.e. BLUS)";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(173, 23);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(212, 15);
            label6.TabIndex = 5;
            label6.Text = "- Game Product Code (e.g. BLUS12345)";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(8, 83);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(61, 15);
            label5.TabIndex = 4;
            label5.Text = "%region%";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(8, 68);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(47, 15);
            label4.TabIndex = 3;
            label4.Text = "%title%";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(8, 53);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(152, 15);
            label3.TabIndex = 2;
            label3.Text = "%product_code_numbers%";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(8, 38);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(137, 15);
            label2.TabIndex = 1;
            label2.Text = "%product_code_letters%";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 23);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(100, 15);
            label1.TabIndex = 0;
            label1.Text = "%product_code%";
            // 
            // cancelButton
            // 
            cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(618, 210);
            cancelButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(86, 27);
            cancelButton.TabIndex = 11;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += cancelButton_Click;
            // 
            // saveButton
            // 
            saveButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            saveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            saveButton.Location = new System.Drawing.Point(618, 177);
            saveButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(88, 27);
            saveButton.TabIndex = 10;
            saveButton.Text = "OK";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // irdTooltip
            // 
            irdTooltip.ToolTipTitle = "IRD Cache Folder";
            // 
            // dumpNameTooltip
            // 
            dumpNameTooltip.ToolTipTitle = "Dump Name Pattern";
            // 
            // defaultsButton
            // 
            defaultsButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            defaultsButton.Location = new System.Drawing.Point(616, 71);
            defaultsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            defaultsButton.Name = "defaultsButton";
            defaultsButton.Size = new System.Drawing.Size(88, 27);
            defaultsButton.TabIndex = 12;
            defaultsButton.Text = "Defaults";
            defaultsButton.UseVisualStyleBackColor = true;
            defaultsButton.Click += defaultsButton_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(719, 250);
            Controls.Add(defaultsButton);
            Controls.Add(saveButton);
            Controls.Add(cancelButton);
            Controls.Add(groupBox1);
            Controls.Add(namePatternExampleLabel);
            Controls.Add(namePatternTextBox);
            Controls.Add(namePatternLabel);
            Controls.Add(irdButton);
            Controls.Add(irdTextBox);
            Controls.Add(labelIrd);
            Controls.Add(outputBrowseButton);
            Controls.Add(outputTextBox);
            Controls.Add(outputLabel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Settings";
            Load += SettingsForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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