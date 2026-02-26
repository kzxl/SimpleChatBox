namespace ChatBox.Client.Forms
{
    partial class frmVideoCall
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlRemoteVideo = new System.Windows.Forms.PictureBox();
            this.pnlLocalVideo = new System.Windows.Forms.PictureBox();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.btnEndCall = new System.Windows.Forms.Button();
            this.btnRecord = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pnlRemoteVideo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlLocalVideo)).BeginInit();
            this.pnlControls.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlRemoteVideo
            // 
            this.pnlRemoteVideo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.pnlRemoteVideo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRemoteVideo.Location = new System.Drawing.Point(0, 0);
            this.pnlRemoteVideo.Name = "pnlRemoteVideo";
            this.pnlRemoteVideo.Size = new System.Drawing.Size(640, 430);
            this.pnlRemoteVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pnlRemoteVideo.TabIndex = 0;
            this.pnlRemoteVideo.TabStop = false;
            // 
            // pnlLocalVideo
            // 
            this.pnlLocalVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlLocalVideo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.pnlLocalVideo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLocalVideo.Location = new System.Drawing.Point(480, 310);
            this.pnlLocalVideo.Name = "pnlLocalVideo";
            this.pnlLocalVideo.Size = new System.Drawing.Size(150, 110);
            this.pnlLocalVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pnlLocalVideo.TabIndex = 1;
            this.pnlLocalVideo.TabStop = false;
            // 
            // pnlControls
            // 
            this.pnlControls.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(40)))));
            this.pnlControls.Controls.Add(this.lblStatus);
            this.pnlControls.Controls.Add(this.btnRecord);
            this.pnlControls.Controls.Add(this.btnEndCall);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlControls.Location = new System.Drawing.Point(0, 430);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Padding = new System.Windows.Forms.Padding(10);
            this.pnlControls.Size = new System.Drawing.Size(640, 50);
            this.pnlControls.TabIndex = 2;
            // 
            // btnEndCall
            // 
            this.btnEndCall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.btnEndCall.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnEndCall.FlatAppearance.BorderSize = 0;
            this.btnEndCall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEndCall.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEndCall.ForeColor = System.Drawing.Color.White;
            this.btnEndCall.Location = new System.Drawing.Point(520, 10);
            this.btnEndCall.Name = "btnEndCall";
            this.btnEndCall.Size = new System.Drawing.Size(110, 30);
            this.btnEndCall.TabIndex = 0;
            this.btnEndCall.Text = "📞 Kết thúc";
            this.btnEndCall.UseVisualStyleBackColor = false;
            this.btnEndCall.Click += new System.EventHandler(this.btnEndCall_Click);
            // 
            // btnRecord
            // 
            this.btnRecord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(75)))));
            this.btnRecord.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnRecord.FlatAppearance.BorderSize = 0;
            this.btnRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRecord.ForeColor = System.Drawing.Color.White;
            this.btnRecord.Location = new System.Drawing.Point(410, 10);
            this.btnRecord.Name = "btnRecord";
            this.btnRecord.Size = new System.Drawing.Size(110, 30);
            this.btnRecord.TabIndex = 1;
            this.btnRecord.Text = "⏺ Ghi hình";
            this.btnRecord.UseVisualStyleBackColor = false;
            this.btnRecord.Click += new System.EventHandler(this.btnRecord_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Location = new System.Drawing.Point(10, 10);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(400, 30);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.Text = "📹 Đang gọi...";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // frmVideoCall
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(640, 480);
            this.Controls.Add(this.pnlLocalVideo);
            this.Controls.Add(this.pnlRemoteVideo);
            this.Controls.Add(this.pnlControls);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(480, 400);
            this.Name = "frmVideoCall";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Video Call";
            ((System.ComponentModel.ISupportInitialize)(this.pnlRemoteVideo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlLocalVideo)).EndInit();
            this.pnlControls.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pnlRemoteVideo;
        private System.Windows.Forms.PictureBox pnlLocalVideo;
        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Button btnEndCall;
        private System.Windows.Forms.Button btnRecord;
        private System.Windows.Forms.Label lblStatus;
    }
}
