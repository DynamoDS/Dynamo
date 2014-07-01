namespace CSharpAnalytics.Sample.WinForms
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
            this.TrackScreenButton = new System.Windows.Forms.Button();
            this.TrackEventButton = new System.Windows.Forms.Button();
            this.AllowUsageDataCollectionCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // TrackScreenButton
            // 
            this.TrackScreenButton.Location = new System.Drawing.Point(12, 12);
            this.TrackScreenButton.Name = "TrackScreenButton";
            this.TrackScreenButton.Size = new System.Drawing.Size(125, 23);
            this.TrackScreenButton.TabIndex = 0;
            this.TrackScreenButton.Text = "Track Screen";
            this.TrackScreenButton.UseVisualStyleBackColor = true;
            this.TrackScreenButton.Click += new System.EventHandler(this.TrackScreenButtonClick);
            // 
            // TrackEventButton
            // 
            this.TrackEventButton.Location = new System.Drawing.Point(12, 41);
            this.TrackEventButton.Name = "TrackEventButton";
            this.TrackEventButton.Size = new System.Drawing.Size(125, 23);
            this.TrackEventButton.TabIndex = 1;
            this.TrackEventButton.Text = "Track Event";
            this.TrackEventButton.UseVisualStyleBackColor = true;
            this.TrackEventButton.Click += new System.EventHandler(this.TrackEventButtonClick);
            // 
            // AllowUsageDataCollectionCheckBox
            // 
            this.AllowUsageDataCollectionCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AllowUsageDataCollectionCheckBox.AutoSize = true;
            this.AllowUsageDataCollectionCheckBox.Location = new System.Drawing.Point(12, 332);
            this.AllowUsageDataCollectionCheckBox.Name = "AllowUsageDataCollectionCheckBox";
            this.AllowUsageDataCollectionCheckBox.Size = new System.Drawing.Size(160, 17);
            this.AllowUsageDataCollectionCheckBox.TabIndex = 2;
            this.AllowUsageDataCollectionCheckBox.Text = "Allow Usage Data Collection";
            this.AllowUsageDataCollectionCheckBox.UseVisualStyleBackColor = true;
            this.AllowUsageDataCollectionCheckBox.CheckedChanged += new System.EventHandler(this.AllowUsageDataCollectionCheckBox_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.AllowUsageDataCollectionCheckBox);
            this.Controls.Add(this.TrackEventButton);
            this.Controls.Add(this.TrackScreenButton);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CSharpAnalytics Sample WinForms App";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button TrackScreenButton;
        private System.Windows.Forms.Button TrackEventButton;
        private System.Windows.Forms.CheckBox AllowUsageDataCollectionCheckBox;
    }
}