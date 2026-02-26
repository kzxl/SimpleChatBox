using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChatBox.Client.Services;
using ChatBox.Shared.Protocol;

namespace ChatBox.Client.Forms
{
    /// <summary>
    /// Form đăng nhập / đăng ký
    /// </summary>
    public partial class frmLogin : Form
    {
        private TcpClientService _tcpService;

        public string LoggedInUserId { get; private set; }
        public string LoggedInDisplayName { get; private set; }
        public TcpClientService TcpService => _tcpService;

        public frmLogin()
        {
            InitializeComponent();
            _tcpService = new TcpClientService();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            await DoAuth(PacketType.Login);
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            await DoAuth(PacketType.Register);
        }

        private async Task DoAuth(PacketType authType)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblStatus.Text = "Vui lòng nhập username và password";
                lblStatus.ForeColor = System.Drawing.Color.Orange;
                return;
            }

            btnLogin.Enabled = false;
            btnRegister.Enabled = false;
            lblStatus.Text = "Đang kết nối...";
            lblStatus.ForeColor = System.Drawing.Color.Gray;

            try
            {
                // 1. Kết nối TCP
                if (!_tcpService.IsConnected)
                {
                    var connected = await _tcpService.ConnectAsync(txtServer.Text, (int)nudPort.Value);
                    if (!connected)
                    {
                        lblStatus.Text = "Không thể kết nối đến server";
                        lblStatus.ForeColor = System.Drawing.Color.Red;
                        return;
                    }
                }

                // 2. Hash password
                string passwordHash;
                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(txtPassword.Text));
                    passwordHash = Convert.ToBase64String(bytes);
                }

                // 3. Gửi packet Login/Register
                var data = string.Format(
                    "{{\"Username\":\"{0}\",\"PasswordHash\":\"{1}\"}}",
                    txtUsername.Text, passwordHash);

                var packet = new Packet(authType, null, null, data);

                // Subscribe nhận response 1 lần
                Action<Packet> handler = null;
                var tcs = new TaskCompletionSource<Packet>();
                handler = p =>
                {
                    if (p.Type == PacketType.LoginResponse || p.Type == PacketType.RegisterResponse)
                    {
                        _tcpService.OnPacketReceived -= handler;
                        tcs.TrySetResult(p);
                    }
                };
                _tcpService.OnPacketReceived += handler;

                _tcpService.SendPacket(packet);
                lblStatus.Text = authType == PacketType.Login ? "Đang đăng nhập..." : "Đang đăng ký...";

                // 4. Chờ response (timeout 10s)
                var timeoutTask = Task.Delay(10000);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    _tcpService.OnPacketReceived -= handler;
                    lblStatus.Text = "Timeout - server không phản hồi";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                var response = tcs.Task.Result;

                // 5. Parse response
                var success = GetJsonField(response.Data, "Success") == "true";
                var message = GetJsonField(response.Data, "Message");
                var userId = GetJsonField(response.Data, "UserId");
                var displayName = GetJsonField(response.Data, "DisplayName");

                if (success)
                {
                    LoggedInUserId = userId;
                    LoggedInDisplayName = displayName;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblStatus.Text = message ?? "Đăng nhập thất bại";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Lỗi: {ex.Message}";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
            finally
            {
                btnLogin.Enabled = true;
                btnRegister.Enabled = true;
            }
        }

        private string GetJsonField(string json, string field)
        {
            if (string.IsNullOrEmpty(json)) return null;

            // Handle boolean fields
            var searchBool = "\"" + field + "\":";
            int idx = json.IndexOf(searchBool, StringComparison.Ordinal);
            if (idx < 0) return null;

            idx += searchBool.Length;
            while (idx < json.Length && json[idx] == ' ') idx++;

            if (idx >= json.Length) return null;

            if (json[idx] == '"')
            {
                idx++;
                int end = json.IndexOf('"', idx);
                return end < 0 ? null : json.Substring(idx, end - idx);
            }
            else
            {
                // Boolean or number
                var sb = new StringBuilder();
                while (idx < json.Length && json[idx] != ',' && json[idx] != '}')
                {
                    sb.Append(json[idx]);
                    idx++;
                }
                return sb.ToString().Trim();
            }
        }
    }
}
