namespace CommonUtils
{
    partial class Download
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
            if (disposing && (components != null)) {
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
            this.UpdatePercent = new System.Windows.Forms.TableLayoutPanel();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lbStatus = new System.Windows.Forms.Label();
            this.UpdatePercent.SuspendLayout();
            this.SuspendLayout();
            // 
            // UpdatePercent
            // 
            this.UpdatePercent.ColumnCount = 1;
            this.UpdatePercent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.UpdatePercent.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.UpdatePercent.Controls.Add(this.progressBar, 0, 0);
            this.UpdatePercent.Controls.Add(this.lbStatus, 0, 1);
            this.UpdatePercent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UpdatePercent.Location = new System.Drawing.Point(0, 0);
            this.UpdatePercent.Name = "UpdatePercent";
            this.UpdatePercent.RowCount = 2;
            this.UpdatePercent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.UpdatePercent.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.UpdatePercent.Size = new System.Drawing.Size(464, 49);
            this.UpdatePercent.TabIndex = 0;
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(3, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(458, 18);
            this.progressBar.TabIndex = 0;
            // 
            // lbStatus
            // 
            this.lbStatus.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbStatus.AutoSize = true;
            this.lbStatus.Location = new System.Drawing.Point(193, 30);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(78, 13);
            this.lbStatus.TabIndex = 1;
            this.lbStatus.Text = "Dowloaded 0%";
            // 
            // Download
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 49);
            this.Controls.Add(this.UpdatePercent);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(480, 88);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(480, 88);
            this.Name = "Download";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Download Update";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Download_FormClosing);
            this.Load += new System.EventHandler(this.Download_Load);
            this.UpdatePercent.ResumeLayout(false);
            this.UpdatePercent.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel UpdatePercent;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lbStatus;
    }
}