namespace ToolLog
{
    partial class ToolLog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ToolLog));
            this.StartBtn = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.AlwaysHideCheckBox = new System.Windows.Forms.CheckBox();
            this.HideCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.FilePathLabel = new System.Windows.Forms.Label();
            this.FixedCheckBox = new System.Windows.Forms.CheckBox();
            this.Browser = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.FindBox = new System.Windows.Forms.TextBox();
            this.CommandLabel = new System.Windows.Forms.Label();
            this.TipsLabel = new System.Windows.Forms.Label();
            this.richTextBox1 = new Custom.CustomRichTextBox();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartBtn
            // 
            this.StartBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.StartBtn.BackColor = System.Drawing.SystemColors.Control;
            this.StartBtn.Location = new System.Drawing.Point(708, 461);
            this.StartBtn.Name = "StartBtn";
            this.StartBtn.Size = new System.Drawing.Size(75, 23);
            this.StartBtn.TabIndex = 1;
            this.StartBtn.Text = "Start";
            this.StartBtn.UseVisualStyleBackColor = false;
            this.StartBtn.Click += new System.EventHandler(this.StartBtn_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBox
            // 
            this.groupBox.BackColor = System.Drawing.Color.Transparent;
            this.groupBox.Controls.Add(this.AlwaysHideCheckBox);
            this.groupBox.Controls.Add(this.HideCheckBox);
            this.groupBox.Controls.Add(this.label1);
            this.groupBox.Controls.Add(this.FilePathLabel);
            this.groupBox.Controls.Add(this.FixedCheckBox);
            this.groupBox.Controls.Add(this.Browser);
            this.groupBox.Location = new System.Drawing.Point(12, 12);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(512, 98);
            this.groupBox.TabIndex = 3;
            this.groupBox.TabStop = false;
            // 
            // AlwaysHideCheckBox
            // 
            this.AlwaysHideCheckBox.AutoSize = true;
            this.AlwaysHideCheckBox.Location = new System.Drawing.Point(78, 66);
            this.AlwaysHideCheckBox.Name = "AlwaysHideCheckBox";
            this.AlwaysHideCheckBox.Size = new System.Drawing.Size(90, 16);
            this.AlwaysHideCheckBox.TabIndex = 13;
            this.AlwaysHideCheckBox.Text = "Always Hide";
            this.AlwaysHideCheckBox.UseVisualStyleBackColor = true;
            this.AlwaysHideCheckBox.CheckedChanged += new System.EventHandler(this.AlwaysHideCheckBox_CheckedChanged);
            // 
            // HideCheckBox
            // 
            this.HideCheckBox.AutoSize = true;
            this.HideCheckBox.Location = new System.Drawing.Point(24, 66);
            this.HideCheckBox.Name = "HideCheckBox";
            this.HideCheckBox.Size = new System.Drawing.Size(48, 16);
            this.HideCheckBox.TabIndex = 12;
            this.HideCheckBox.Text = "Hide";
            this.HideCheckBox.UseVisualStyleBackColor = true;
            this.HideCheckBox.CheckedChanged += new System.EventHandler(this.HideCheckBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.Control;
            this.label1.Location = new System.Drawing.Point(24, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 11;
            this.label1.Text = "Config";
            // 
            // FilePathLabel
            // 
            this.FilePathLabel.AutoSize = true;
            this.FilePathLabel.BackColor = System.Drawing.SystemColors.Window;
            this.FilePathLabel.Enabled = false;
            this.FilePathLabel.Location = new System.Drawing.Point(102, 30);
            this.FilePathLabel.MaximumSize = new System.Drawing.Size(300, 20);
            this.FilePathLabel.MinimumSize = new System.Drawing.Size(300, 20);
            this.FilePathLabel.Name = "FilePathLabel";
            this.FilePathLabel.Size = new System.Drawing.Size(300, 20);
            this.FilePathLabel.TabIndex = 10;
            this.FilePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FixedCheckBox
            // 
            this.FixedCheckBox.AutoSize = true;
            this.FixedCheckBox.Location = new System.Drawing.Point(24, 34);
            this.FixedCheckBox.Name = "FixedCheckBox";
            this.FixedCheckBox.Size = new System.Drawing.Size(72, 16);
            this.FixedCheckBox.TabIndex = 8;
            this.FixedCheckBox.Text = "FilePath";
            this.FixedCheckBox.UseVisualStyleBackColor = true;
            this.FixedCheckBox.CheckedChanged += new System.EventHandler(this.FixedCheckBox_CheckedChanged);
            // 
            // Browser
            // 
            this.Browser.Enabled = false;
            this.Browser.Location = new System.Drawing.Point(408, 27);
            this.Browser.Name = "Browser";
            this.Browser.Size = new System.Drawing.Size(75, 23);
            this.Browser.TabIndex = 3;
            this.Browser.Text = "Browser";
            this.Browser.UseVisualStyleBackColor = true;
            this.Browser.Click += new System.EventHandler(this.Browser_Click);
            // 
            // FindBox
            // 
            this.FindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FindBox.BackColor = System.Drawing.SystemColors.Window;
            this.FindBox.Location = new System.Drawing.Point(-1, 436);
            this.FindBox.Name = "FindBox";
            this.FindBox.Size = new System.Drawing.Size(795, 21);
            this.FindBox.TabIndex = 4;
            this.FindBox.Visible = false;
            this.FindBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FindBox_KeyDown);
            // 
            // CommandLabel
            // 
            this.CommandLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandLabel.AutoSize = true;
            this.CommandLabel.BackColor = System.Drawing.SystemColors.Control;
            this.CommandLabel.Location = new System.Drawing.Point(482, 467);
            this.CommandLabel.MaximumSize = new System.Drawing.Size(220, 0);
            this.CommandLabel.MinimumSize = new System.Drawing.Size(220, 0);
            this.CommandLabel.Name = "CommandLabel";
            this.CommandLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.CommandLabel.Size = new System.Drawing.Size(220, 12);
            this.CommandLabel.TabIndex = 11;
            this.CommandLabel.Text = "hfg";
            this.CommandLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TipsLabel
            // 
            this.TipsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TipsLabel.AutoSize = true;
            this.TipsLabel.BackColor = System.Drawing.SystemColors.Control;
            this.TipsLabel.ForeColor = System.Drawing.Color.Red;
            this.TipsLabel.Location = new System.Drawing.Point(2, 466);
            this.TipsLabel.Name = "TipsLabel";
            this.TipsLabel.Size = new System.Drawing.Size(41, 12);
            this.TipsLabel.TabIndex = 10;
            this.TipsLabel.Text = "label2";
            this.TipsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TipsLabel.Visible = false;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox1.IsFind = false;
            this.richTextBox1.Location = new System.Drawing.Point(-1, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(787, 457);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox1_KeyDown);
            this.richTextBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.richTextBox1_MouseDoubleClick);
            // 
            // ToolLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(785, 488);
            this.Controls.Add(this.TipsLabel);
            this.Controls.Add(this.CommandLabel);
            this.Controls.Add(this.FindBox);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.StartBtn);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ToolLog";
            this.Text = "ToolLog";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ToolLog_FormClosed);
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Custom.CustomRichTextBox richTextBox1;
        private System.Windows.Forms.Button StartBtn;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Button Browser;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox FindBox;
        private System.Windows.Forms.Label FilePathLabel;
        private System.Windows.Forms.CheckBox FixedCheckBox;
        private System.Windows.Forms.Label CommandLabel;
        private System.Windows.Forms.CheckBox AlwaysHideCheckBox;
        private System.Windows.Forms.CheckBox HideCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label TipsLabel;
    }
}

