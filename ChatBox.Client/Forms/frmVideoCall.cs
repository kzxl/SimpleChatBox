using System;
using System.Drawing;
using System.Windows.Forms;
using ChatBox.Client.Helpers;
using ChatBox.Client.Services;

namespace ChatBox.Client.Forms
{
    /// <summary>
    /// Form gọi video - hiển thị video từ xa và local camera.
    /// Skeleton - sẽ tích hợp AForge camera capture ở Phase 4.
    /// </summary>
    public partial class frmVideoCall : Form
    {
        private readonly VideoCallService _videoCallService;
        private readonly VideoRecorder _recorder;
        private bool _isRecording;

        public frmVideoCall(VideoCallService videoCallService)
        {
            InitializeComponent();
            _videoCallService = videoCallService;
            _recorder = new VideoRecorder();

            _videoCallService.OnVideoFrameReceived += DisplayRemoteFrame;
            _videoCallService.OnCallEnded += HandleCallEnded;
        }

        private void DisplayRemoteFrame(byte[] frameData)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<byte[]>(DisplayRemoteFrame), frameData);
                return;
            }

            try
            {
                using (var ms = new System.IO.MemoryStream(frameData))
                {
                    var image = Image.FromStream(ms);
                    pnlRemoteVideo.Image?.Dispose();
                    pnlRemoteVideo.Image = new Bitmap(image);

                    // Ghi lại nếu đang recording
                    if (_isRecording)
                    {
                        _recorder.AddFrame(frameData);
                    }
                }
            }
            catch { }
        }

        private void HandleCallEnded()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HandleCallEnded));
                return;
            }

            if (_isRecording)
            {
                _recorder.StopRecording();
                _isRecording = false;
            }

            lblStatus.Text = "📹 Cuộc gọi đã kết thúc";
            MessageBox.Show("Cuộc gọi đã kết thúc", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }

        private void btnEndCall_Click(object sender, EventArgs e)
        {
            _videoCallService.EndCall();

            if (_isRecording)
            {
                _recorder.StopRecording();
                _isRecording = false;
            }

            this.Close();
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (!_isRecording)
            {
                var outputPath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Recordings",
                    $"call_{DateTime.Now:yyyyMMdd_HHmmss}.avi");

                var dir = System.IO.Path.GetDirectoryName(outputPath);
                if (!System.IO.Directory.Exists(dir))
                    System.IO.Directory.CreateDirectory(dir);

                _recorder.StartRecording(outputPath);
                _isRecording = true;
                btnRecord.Text = "⏹ Dừng ghi";
                btnRecord.BackColor = Color.FromArgb(200, 50, 50);
                lblStatus.Text = "🔴 Đang ghi hình...";
            }
            else
            {
                _recorder.StopRecording();
                _isRecording = false;
                btnRecord.Text = "⏺ Ghi hình";
                btnRecord.BackColor = Color.FromArgb(70, 70, 75);
                lblStatus.Text = "📹 Đang gọi...";
            }
        }
    }
}
