namespace UI.WinForms.Msil
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            groupBox1 = new System.Windows.Forms.GroupBox();
            updateButton = new System.Windows.Forms.Button();
            settingsButton = new System.Windows.Forms.Button();
            discSizeLabel = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            irdMatchLabel = new System.Windows.Forms.Label();
            gameTitleLabel = new System.Windows.Forms.Label();
            productCodeLabel = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            dumpingProgressBar = new System.Windows.Forms.ProgressBar();
            groupBox2 = new System.Windows.Forms.GroupBox();
            step4Label = new System.Windows.Forms.Label();
            step3Label = new System.Windows.Forms.Label();
            step2Label = new System.Windows.Forms.Label();
            step1Label = new System.Windows.Forms.Label();
            step4StatusLabel = new System.Windows.Forms.Label();
            step3StatusLabel = new System.Windows.Forms.Label();
            step2StatusLabel = new System.Windows.Forms.Label();
            step1StatusLabel = new System.Windows.Forms.Label();
            selectIrdButton = new System.Windows.Forms.Button();
            rescanDiscsButton = new System.Windows.Forms.Button();
            startDumpingButton = new System.Windows.Forms.Button();
            cancelDiscDumpButton = new System.Windows.Forms.Button();
            dumpingProgressLabel = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(updateButton);
            groupBox1.Controls.Add(settingsButton);
            groupBox1.Controls.Add(discSizeLabel);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(irdMatchLabel);
            groupBox1.Controls.Add(gameTitleLabel);
            groupBox1.Controls.Add(productCodeLabel);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new System.Drawing.Point(448, 14);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(460, 126);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Disc Information";
            // 
            // updateButton
            // 
            updateButton.Location = new System.Drawing.Point(391, 12);
            updateButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            updateButton.Name = "updateButton";
            updateButton.Size = new System.Drawing.Size(27, 27);
            updateButton.TabIndex = 11;
            updateButton.Text = "🆕";
            toolTip1.SetToolTip(updateButton, "New version available");
            updateButton.UseVisualStyleBackColor = true;
            updateButton.Visible = false;
            updateButton.Click += updateButton_Click;
            // 
            // settingsButton
            // 
            settingsButton.Location = new System.Drawing.Point(426, 12);
            settingsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            settingsButton.Name = "settingsButton";
            settingsButton.Size = new System.Drawing.Size(27, 27);
            settingsButton.TabIndex = 10;
            settingsButton.Text = "⚙";
            toolTip1.SetToolTip(settingsButton, "View or change program settings");
            settingsButton.UseVisualStyleBackColor = true;
            settingsButton.Click += settingsButton_Click;
            // 
            // discSizeLabel
            // 
            discSizeLabel.AutoSize = true;
            discSizeLabel.Location = new System.Drawing.Point(102, 76);
            discSizeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            discSizeLabel.Name = "discSizeLabel";
            discSizeLabel.Size = new System.Drawing.Size(46, 15);
            discSizeLabel.TabIndex = 9;
            discSizeLabel.Text = "45.7 GB";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(7, 76);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(57, 15);
            label4.TabIndex = 8;
            label4.Text = "Dump Size:";
            // 
            // irdMatchLabel
            // 
            irdMatchLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            irdMatchLabel.Location = new System.Drawing.Point(102, 103);
            irdMatchLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            irdMatchLabel.Name = "irdMatchLabel";
            irdMatchLabel.Size = new System.Drawing.Size(351, 15);
            irdMatchLabel.TabIndex = 5;
            irdMatchLabel.Text = "BCES00802-v2-0338EC334F5ADEF4DE4E4007D7E1D80B";
            // 
            // gameTitleLabel
            // 
            gameTitleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gameTitleLabel.Location = new System.Drawing.Point(102, 50);
            gameTitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            gameTitleLabel.Name = "gameTitleLabel";
            gameTitleLabel.Size = new System.Drawing.Size(351, 15);
            gameTitleLabel.TabIndex = 4;
            gameTitleLabel.Text = "My PS3 Game Can't Be This Cute™";
            // 
            // productCodeLabel
            // 
            productCodeLabel.AutoSize = true;
            productCodeLabel.Location = new System.Drawing.Point(102, 23);
            productCodeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            productCodeLabel.Name = "productCodeLabel";
            productCodeLabel.Size = new System.Drawing.Size(64, 15);
            productCodeLabel.TabIndex = 3;
            productCodeLabel.Text = "BLUS12345";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(7, 103);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(83, 15);
            label3.TabIndex = 2;
            label3.Text = "Matching Key:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 50);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(66, 15);
            label2.TabIndex = 1;
            label2.Text = "Game Title:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 23);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(83, 15);
            label1.TabIndex = 0;
            label1.Text = "Product Code:";
            // 
            // dumpingProgressBar
            // 
            dumpingProgressBar.Location = new System.Drawing.Point(448, 148);
            dumpingProgressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            dumpingProgressBar.Maximum = 10000;
            dumpingProgressBar.Name = "dumpingProgressBar";
            dumpingProgressBar.Size = new System.Drawing.Size(460, 27);
            dumpingProgressBar.Step = 1;
            dumpingProgressBar.TabIndex = 10;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(step4Label);
            groupBox2.Controls.Add(step3Label);
            groupBox2.Controls.Add(step2Label);
            groupBox2.Controls.Add(step1Label);
            groupBox2.Controls.Add(step4StatusLabel);
            groupBox2.Controls.Add(step3StatusLabel);
            groupBox2.Controls.Add(step2StatusLabel);
            groupBox2.Controls.Add(step1StatusLabel);
            groupBox2.Location = new System.Drawing.Point(14, 14);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(427, 196);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Steps";
            // 
            // step4Label
            // 
            step4Label.AutoSize = true;
            step4Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            step4Label.Location = new System.Drawing.Point(49, 150);
            step4Label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            step4Label.Name = "step4Label";
            step4Label.Size = new System.Drawing.Size(173, 26);
            step4Label.TabIndex = 7;
            step4Label.Text = "Validate integrity";
            // 
            // step3Label
            // 
            step3Label.AutoSize = true;
            step3Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            step3Label.Location = new System.Drawing.Point(49, 108);
            step3Label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            step3Label.Name = "step3Label";
            step3Label.Size = new System.Drawing.Size(226, 26);
            step3Label.TabIndex = 6;
            step3Label.Text = "Decrypt and copy files";
            // 
            // step2Label
            // 
            step2Label.AutoSize = true;
            step2Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            step2Label.Location = new System.Drawing.Point(49, 67);
            step2Label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            step2Label.Name = "step2Label";
            step2Label.Size = new System.Drawing.Size(192, 26);
            step2Label.TabIndex = 5;
            step2Label.Text = "Select disc key file";
            // 
            // step1Label
            // 
            step1Label.AutoSize = true;
            step1Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            step1Label.Location = new System.Drawing.Point(49, 25);
            step1Label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            step1Label.Name = "step1Label";
            step1Label.Size = new System.Drawing.Size(220, 26);
            step1Label.TabIndex = 4;
            step1Label.Text = "Insert PS3 game disc";
            // 
            // step4StatusLabel
            // 
            step4StatusLabel.AutoSize = true;
            step4StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            step4StatusLabel.Location = new System.Drawing.Point(7, 150);
            step4StatusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            step4StatusLabel.Name = "step4StatusLabel";
            step4StatusLabel.Size = new System.Drawing.Size(37, 26);
            step4StatusLabel.TabIndex = 3;
            step4StatusLabel.Text = "⏳";
            // 
            // step3StatusLabel
            // 
            step3StatusLabel.AutoSize = true;
            step3StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            step3StatusLabel.Location = new System.Drawing.Point(7, 108);
            step3StatusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            step3StatusLabel.Name = "step3StatusLabel";
            step3StatusLabel.Size = new System.Drawing.Size(37, 26);
            step3StatusLabel.TabIndex = 2;
            step3StatusLabel.Text = "⏳";
            // 
            // step2StatusLabel
            // 
            step2StatusLabel.AutoSize = true;
            step2StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            step2StatusLabel.Location = new System.Drawing.Point(7, 67);
            step2StatusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            step2StatusLabel.Name = "step2StatusLabel";
            step2StatusLabel.Size = new System.Drawing.Size(37, 26);
            step2StatusLabel.TabIndex = 1;
            step2StatusLabel.Text = "⏳";
            // 
            // step1StatusLabel
            // 
            step1StatusLabel.AutoSize = true;
            step1StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            step1StatusLabel.Location = new System.Drawing.Point(7, 25);
            step1StatusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            step1StatusLabel.Name = "step1StatusLabel";
            step1StatusLabel.Size = new System.Drawing.Size(37, 26);
            step1StatusLabel.TabIndex = 0;
            step1StatusLabel.Text = "⏳";
            // 
            // selectIrdButton
            // 
            selectIrdButton.Location = new System.Drawing.Point(449, 148);
            selectIrdButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            selectIrdButton.Name = "selectIrdButton";
            selectIrdButton.Size = new System.Drawing.Size(458, 62);
            selectIrdButton.TabIndex = 3;
            selectIrdButton.Text = "Select disc key file...";
            selectIrdButton.UseVisualStyleBackColor = true;
            selectIrdButton.Click += selectIrdButton_Click;
            // 
            // rescanDiscsButton
            // 
            rescanDiscsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            rescanDiscsButton.Location = new System.Drawing.Point(449, 148);
            rescanDiscsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rescanDiscsButton.Name = "rescanDiscsButton";
            rescanDiscsButton.Size = new System.Drawing.Size(458, 62);
            rescanDiscsButton.TabIndex = 2;
            rescanDiscsButton.Text = "Re-scan discs";
            rescanDiscsButton.UseVisualStyleBackColor = true;
            rescanDiscsButton.Click += rescanDiscsButton_Click;
            // 
            // startDumpingButton
            // 
            startDumpingButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            startDumpingButton.Location = new System.Drawing.Point(449, 148);
            startDumpingButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            startDumpingButton.Name = "startDumpingButton";
            startDumpingButton.Size = new System.Drawing.Size(458, 62);
            startDumpingButton.TabIndex = 3;
            startDumpingButton.Text = "Start";
            startDumpingButton.UseVisualStyleBackColor = true;
            startDumpingButton.Click += startDumpingButton_Click;
            // 
            // cancelDiscDumpButton
            // 
            cancelDiscDumpButton.Location = new System.Drawing.Point(820, 183);
            cancelDiscDumpButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cancelDiscDumpButton.Name = "cancelDiscDumpButton";
            cancelDiscDumpButton.Size = new System.Drawing.Size(88, 27);
            cancelDiscDumpButton.TabIndex = 4;
            cancelDiscDumpButton.Text = "Cancel";
            cancelDiscDumpButton.UseVisualStyleBackColor = true;
            cancelDiscDumpButton.Click += cancelDiscDumpButton_Click;
            // 
            // dumpingProgressLabel
            // 
            dumpingProgressLabel.Location = new System.Drawing.Point(455, 183);
            dumpingProgressLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            dumpingProgressLabel.Name = "dumpingProgressLabel";
            dumpingProgressLabel.Size = new System.Drawing.Size(358, 27);
            dumpingProgressLabel.TabIndex = 11;
            dumpingProgressLabel.Text = "File 1 of 13";
            dumpingProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(919, 222);
            Controls.Add(selectIrdButton);
            Controls.Add(dumpingProgressLabel);
            Controls.Add(dumpingProgressBar);
            Controls.Add(cancelDiscDumpButton);
            Controls.Add(startDumpingButton);
            Controls.Add(rescanDiscsButton);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "PS3 Disc Dumper";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label irdMatchLabel;
        private System.Windows.Forms.Label gameTitleLabel;
        private System.Windows.Forms.Label productCodeLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label step4Label;
        private System.Windows.Forms.Label step3Label;
        private System.Windows.Forms.Label step2Label;
        private System.Windows.Forms.Label step1Label;
        private System.Windows.Forms.Label step4StatusLabel;
        private System.Windows.Forms.Label step3StatusLabel;
        private System.Windows.Forms.Label step2StatusLabel;
        private System.Windows.Forms.Label step1StatusLabel;
        private System.Windows.Forms.Label discSizeLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button rescanDiscsButton;
        private System.Windows.Forms.Button selectIrdButton;
        private System.Windows.Forms.Button startDumpingButton;
        private System.Windows.Forms.Button cancelDiscDumpButton;
        private System.Windows.Forms.ProgressBar dumpingProgressBar;
        private System.Windows.Forms.Label dumpingProgressLabel;
        private System.Windows.Forms.Button settingsButton;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button updateButton;
    }
}

