using System;
using System.Windows.Forms;
using ChatBox.Server.Data;
using ChatBox.Server.Services;

namespace ChatBox.Server.Forms
{
    /// <summary>
    /// Server dashboard - hiển thị trạng thái, danh sách client, log
    /// </summary>
    public partial class frmServer : Form
    {
        private TcpServerService _serverService;
        private UserStore _userStore;

        public frmServer()
        {
            InitializeComponent();
            _userStore = new UserStore();
            _serverService = new TcpServerService(_userStore);

            // Subscribe events
            _serverService.OnLog += AppendLog;
            _serverService.OnClientListChanged += RefreshClientList;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                int port = (int)nudPort.Value;
                _serverService.Start(port);

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                nudPort.Enabled = false;
                lblStatus.Text = "● Server đang chạy";
                lblStatus.ForeColor = System.Drawing.Color.LimeGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể khởi động server: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _serverService.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            nudPort.Enabled = true;
            lblStatus.Text = "● Server dừng";
            lblStatus.ForeColor = System.Drawing.Color.Gray;
        }

        private void AppendLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            txtLog.AppendText(message + Environment.NewLine);
            txtLog.ScrollToCaret();
        }

        private void RefreshClientList()
        {
            if (lstClients.InvokeRequired)
            {
                lstClients.BeginInvoke(new Action(RefreshClientList));
                return;
            }

            lstClients.Items.Clear();
            // TODO: Expose client list từ TcpServerService
            lblClients.Text = $"📡 Clients Online ({_serverService.ConnectedCount})";
        }

        private void frmServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_serverService.IsRunning)
            {
                var result = MessageBox.Show(
                    "Server đang chạy. Bạn có muốn dừng và thoát?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _serverService.Stop();
            }
        }
    }
}
