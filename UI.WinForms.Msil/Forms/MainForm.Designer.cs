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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.settingsButton = new System.Windows.Forms.Button();
            this.discSizeLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.irdMatchLabel = new System.Windows.Forms.Label();
            this.gameTitleLabel = new System.Windows.Forms.Label();
            this.productCodeLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dumpingProgressBar = new System.Windows.Forms.ProgressBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.step4Label = new System.Windows.Forms.Label();
            this.step3Label = new System.Windows.Forms.Label();
            this.step2Label = new System.Windows.Forms.Label();
            this.step1Label = new System.Windows.Forms.Label();
            this.step4StatusLabel = new System.Windows.Forms.Label();
            this.step3StatusLabel = new System.Windows.Forms.Label();
            this.step2StatusLabel = new System.Windows.Forms.Label();
            this.step1StatusLabel = new System.Windows.Forms.Label();
            this.selectIrdButton = new System.Windows.Forms.Button();
            this.rescanDiscsButton = new System.Windows.Forms.Button();
            this.startDumpingButton = new System.Windows.Forms.Button();
            this.cancelDiscDumpButton = new System.Windows.Forms.Button();
            this.dumpingProgressLabel = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.settingsButton);
            this.groupBox1.Controls.Add(this.discSizeLabel);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.irdMatchLabel);
            this.groupBox1.Controls.Add(this.gameTitleLabel);
            this.groupBox1.Controls.Add(this.productCodeLabel);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(384, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(394, 109);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Disc Information";
            // 
            // settingsButton
            // 
            this.settingsButton.Location = new System.Drawing.Point(365, 10);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(23, 23);
            this.settingsButton.TabIndex = 10;
            this.settingsButton.Text = "⚙";
            this.toolTip1.SetToolTip(this.settingsButton, "View or change program settings");
            this.settingsButton.UseVisualStyleBackColor = true;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // discSizeLabel
            // 
            this.discSizeLabel.AutoSize = true;
            this.discSizeLabel.Location = new System.Drawing.Point(87, 66);
            this.discSizeLabel.Name = "discSizeLabel";
            this.discSizeLabel.Size = new System.Drawing.Size(46, 13);
            this.discSizeLabel.TabIndex = 9;
            this.discSizeLabel.Text = "45.7 GB";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(54, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Disc Size:";
            // 
            // irdMatchLabel
            // 
            this.irdMatchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.irdMatchLabel.Location = new System.Drawing.Point(87, 89);
            this.irdMatchLabel.Name = "irdMatchLabel";
            this.irdMatchLabel.Size = new System.Drawing.Size(301, 13);
            this.irdMatchLabel.TabIndex = 5;
            this.irdMatchLabel.Text = "BCES00802-v2-0338EC334F5ADEF4DE4E4007D7E1D80B";
            // 
            // gameTitleLabel
            // 
            this.gameTitleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameTitleLabel.Location = new System.Drawing.Point(87, 43);
            this.gameTitleLabel.Name = "gameTitleLabel";
            this.gameTitleLabel.Size = new System.Drawing.Size(301, 13);
            this.gameTitleLabel.TabIndex = 4;
            this.gameTitleLabel.Text = "Weebs in Space";
            // 
            // productCodeLabel
            // 
            this.productCodeLabel.AutoSize = true;
            this.productCodeLabel.Location = new System.Drawing.Point(87, 20);
            this.productCodeLabel.Name = "productCodeLabel";
            this.productCodeLabel.Size = new System.Drawing.Size(65, 13);
            this.productCodeLabel.TabIndex = 3;
            this.productCodeLabel.Text = "BLUS12345";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Matching key:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Game Title:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Product Code:";
            // 
            // dumpingProgressBar
            // 
            this.dumpingProgressBar.Location = new System.Drawing.Point(384, 128);
            this.dumpingProgressBar.Maximum = 10000;
            this.dumpingProgressBar.Name = "dumpingProgressBar";
            this.dumpingProgressBar.Size = new System.Drawing.Size(394, 23);
            this.dumpingProgressBar.Step = 1;
            this.dumpingProgressBar.TabIndex = 10;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.step4Label);
            this.groupBox2.Controls.Add(this.step3Label);
            this.groupBox2.Controls.Add(this.step2Label);
            this.groupBox2.Controls.Add(this.step1Label);
            this.groupBox2.Controls.Add(this.step4StatusLabel);
            this.groupBox2.Controls.Add(this.step3StatusLabel);
            this.groupBox2.Controls.Add(this.step2StatusLabel);
            this.groupBox2.Controls.Add(this.step1StatusLabel);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(366, 170);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Steps";
            // 
            // step4Label
            // 
            this.step4Label.AutoSize = true;
            this.step4Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.step4Label.Location = new System.Drawing.Point(42, 130);
            this.step4Label.Name = "step4Label";
            this.step4Label.Size = new System.Drawing.Size(173, 26);
            this.step4Label.TabIndex = 7;
            this.step4Label.Text = "Validate integrity";
            // 
            // step3Label
            // 
            this.step3Label.AutoSize = true;
            this.step3Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.step3Label.Location = new System.Drawing.Point(42, 94);
            this.step3Label.Name = "step3Label";
            this.step3Label.Size = new System.Drawing.Size(226, 26);
            this.step3Label.TabIndex = 6;
            this.step3Label.Text = "Decrypt and copy files";
            // 
            // step2Label
            // 
            this.step2Label.AutoSize = true;
            this.step2Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.step2Label.Location = new System.Drawing.Point(42, 58);
            this.step2Label.Name = "step2Label";
            this.step2Label.Size = new System.Drawing.Size(192, 26);
            this.step2Label.TabIndex = 5;
            this.step2Label.Text = "Select disc key file";
            // 
            // step1Label
            // 
            this.step1Label.AutoSize = true;
            this.step1Label.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.step1Label.Location = new System.Drawing.Point(42, 22);
            this.step1Label.Name = "step1Label";
            this.step1Label.Size = new System.Drawing.Size(220, 26);
            this.step1Label.TabIndex = 4;
            this.step1Label.Text = "Insert PS3 game disc";
            // 
            // step4StatusLabel
            // 
            this.step4StatusLabel.AutoSize = true;
            this.step4StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.step4StatusLabel.Location = new System.Drawing.Point(6, 130);
            this.step4StatusLabel.Name = "step4StatusLabel";
            this.step4StatusLabel.Size = new System.Drawing.Size(30, 26);
            this.step4StatusLabel.TabIndex = 3;
            this.step4StatusLabel.Text = "⏳";
            // 
            // step3StatusLabel
            // 
            this.step3StatusLabel.AutoSize = true;
            this.step3StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.step3StatusLabel.Location = new System.Drawing.Point(6, 94);
            this.step3StatusLabel.Name = "step3StatusLabel";
            this.step3StatusLabel.Size = new System.Drawing.Size(30, 26);
            this.step3StatusLabel.TabIndex = 2;
            this.step3StatusLabel.Text = "⏳";
            // 
            // step2StatusLabel
            // 
            this.step2StatusLabel.AutoSize = true;
            this.step2StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.step2StatusLabel.Location = new System.Drawing.Point(6, 58);
            this.step2StatusLabel.Name = "step2StatusLabel";
            this.step2StatusLabel.Size = new System.Drawing.Size(30, 26);
            this.step2StatusLabel.TabIndex = 1;
            this.step2StatusLabel.Text = "⏳";
            // 
            // step1StatusLabel
            // 
            this.step1StatusLabel.AutoSize = true;
            this.step1StatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.step1StatusLabel.Location = new System.Drawing.Point(6, 22);
            this.step1StatusLabel.Name = "step1StatusLabel";
            this.step1StatusLabel.Size = new System.Drawing.Size(30, 26);
            this.step1StatusLabel.TabIndex = 0;
            this.step1StatusLabel.Text = "⏳";
            // 
            // selectIrdButton
            // 
            this.selectIrdButton.Location = new System.Drawing.Point(385, 128);
            this.selectIrdButton.Name = "selectIrdButton";
            this.selectIrdButton.Size = new System.Drawing.Size(393, 54);
            this.selectIrdButton.TabIndex = 3;
            this.selectIrdButton.Text = "Select disc key file...";
            this.selectIrdButton.UseVisualStyleBackColor = true;
            this.selectIrdButton.Click += new System.EventHandler(this.selectIrdButton_Click);
            // 
            // rescanDiscsButton
            // 
            this.rescanDiscsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rescanDiscsButton.Location = new System.Drawing.Point(385, 128);
            this.rescanDiscsButton.Name = "rescanDiscsButton";
            this.rescanDiscsButton.Size = new System.Drawing.Size(393, 54);
            this.rescanDiscsButton.TabIndex = 2;
            this.rescanDiscsButton.Text = "Re-scan discs";
            this.rescanDiscsButton.UseVisualStyleBackColor = true;
            this.rescanDiscsButton.Click += new System.EventHandler(this.rescanDiscsButton_Click);
            // 
            // startDumpingButton
            // 
            this.startDumpingButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startDumpingButton.Location = new System.Drawing.Point(385, 128);
            this.startDumpingButton.Name = "startDumpingButton";
            this.startDumpingButton.Size = new System.Drawing.Size(393, 54);
            this.startDumpingButton.TabIndex = 3;
            this.startDumpingButton.Text = "Start";
            this.startDumpingButton.UseVisualStyleBackColor = true;
            this.startDumpingButton.Click += new System.EventHandler(this.startDumpingButton_Click);
            // 
            // cancelDiscDumpButton
            // 
            this.cancelDiscDumpButton.Location = new System.Drawing.Point(703, 159);
            this.cancelDiscDumpButton.Name = "cancelDiscDumpButton";
            this.cancelDiscDumpButton.Size = new System.Drawing.Size(75, 23);
            this.cancelDiscDumpButton.TabIndex = 4;
            this.cancelDiscDumpButton.Text = "Cancel";
            this.cancelDiscDumpButton.UseVisualStyleBackColor = true;
            this.cancelDiscDumpButton.Click += new System.EventHandler(this.cancelDiscDumpButton_Click);
            // 
            // dumpingProgressLabel
            // 
            this.dumpingProgressLabel.Location = new System.Drawing.Point(390, 159);
            this.dumpingProgressLabel.Name = "dumpingProgressLabel";
            this.dumpingProgressLabel.Size = new System.Drawing.Size(307, 23);
            this.dumpingProgressLabel.TabIndex = 11;
            this.dumpingProgressLabel.Text = "File 1 of 13";
            this.dumpingProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 192);
            this.Controls.Add(this.selectIrdButton);
            this.Controls.Add(this.dumpingProgressLabel);
            this.Controls.Add(this.dumpingProgressBar);
            this.Controls.Add(this.cancelDiscDumpButton);
            this.Controls.Add(this.startDumpingButton);
            this.Controls.Add(this.rescanDiscsButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PS3 Disc Dumper v3.0 beta 1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

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
    }
}

