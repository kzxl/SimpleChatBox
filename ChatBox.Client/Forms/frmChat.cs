using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ChatBox.Client.Services;
using ChatBox.Shared.Crypto;
using ChatBox.Shared.Protocol;

namespace ChatBox.Client.Forms
{
    /// <summary>
    /// Giao diện chat chính - hiển thị user online, gửi/nhận tin nhắn, file, video call
    /// </summary>
    public partial class frmChat : Form
    {
        private readonly TcpClientService _tcpService;
        private readonly ChatService _chatService;
        private readonly FileTransferService _fileTransferService;
        private readonly VideoCallService _videoCallService;
        private readonly MessageHistoryService _historyService;

        private string _currentUserId;
        private string _currentDisplayName;
        private string _selectedUserId;
        private string _selectedUserName;

        /// <summary>userId → displayName</summary>
        private readonly Dictionary<string, string> _onlineUsers = new Dictionary<string, string>();

        /// <summary>DH key exchange instances per user</summary>
        private readonly Dictionary<string, DiffieHellmanHelper> _dhInstances = new Dictionary<string, DiffieHellmanHelper>();

        public frmChat(TcpClientService tcpService, string userId, string displayName)
        {
            InitializeComponent();

            _tcpService = tcpService;
            _currentUserId = userId;
            _currentDisplayName = displayName;

            _chatService = new ChatService(tcpService);
            _chatService.CurrentUserId = userId;

            _fileTransferService = new FileTransferService(tcpService, _chatService);
            _videoCallService = new VideoCallService(tcpService, _chatService);
            _historyService = new MessageHistoryService();

            lblCurrentUser.Text = $"💬 ChatBox - Đăng nhập: {displayName}";
            this.Text = $"ChatBox - {displayName}";

            // Subscribe packet handler
            _tcpService.OnPacketReceived += HandlePacket;
            _tcpService.OnDisconnected += HandleDisconnected;

            // Video call events
            _videoCallService.OnIncomingCall += HandleIncomingCall;
        }

        #region Packet Handling

        private void HandlePacket(Packet packet)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<Packet>(HandlePacket), packet);
                return;
            }

            switch (packet.Type)
            {
                case PacketType.UserList:
                    HandleUserList(packet);
                    break;

                case PacketType.Message:
                    HandleMessage(packet);
                    break;

                case PacketType.KeyExchange:
                    HandleKeyExchange(packet);
                    break;

                case PacketType.KeyExchangeResponse:
                    HandleKeyExchangeResponse(packet);
                    break;

                case PacketType.FileHeader:
                    HandleFileHeader(packet);
                    break;

                case PacketType.FileChunk:
                    // TODO: Accumulate chunks
                    break;

                case PacketType.FileComplete:
                    HandleFileComplete(packet);
                    break;

                case PacketType.VideoCallRequest:
                case PacketType.VideoCallAccept:
                case PacketType.VideoCallReject:
                case PacketType.VideoCallEnd:
                case PacketType.VideoFrame:
                    _videoCallService.HandleVideoSignal(packet);
                    break;
            }
        }

        private void HandleUserList(Packet packet)
        {
            _onlineUsers.Clear();
            lstUsers.Items.Clear();

            if (!string.IsNullOrEmpty(packet.Data))
            {
                var users = packet.Data.Split(',');
                foreach (var user in users)
                {
                    var parts = user.Split('|');
                    if (parts.Length >= 2)
                    {
                        string uid = parts[0];
                        string name = parts[1];

                        if (uid == _currentUserId) continue; // Bỏ qua chính mình

                        _onlineUsers[uid] = name;
                        lstUsers.Items.Add($"🟢 {name}");
                    }
                }
            }

            lblUsers.Text = $"👥 Online ({_onlineUsers.Count})";
        }

        private void HandleMessage(Packet packet)
        {
            string content = GetJsonField(packet.Data, "Content");
            bool isEncrypted = GetJsonField(packet.Data, "IsEncrypted") == "true";

            // Giải mã nếu cần
            string displayContent = _chatService.DecryptMessage(packet.SenderId, content, isEncrypted);

            string senderName = "Unknown";
            if (_onlineUsers.ContainsKey(packet.SenderId))
                senderName = _onlineUsers[packet.SenderId];

            AppendChat(senderName, displayContent, Color.FromArgb(100, 200, 255));

            // Lưu lịch sử
            _historyService.SaveMessage(packet.SenderId, packet.SenderId, senderName, displayContent, false);
        }

        private void HandleKeyExchange(Packet packet)
        {
            // Nhận public key từ người khác → tạo DH instance, derive shared secret, gửi lại public key
            var otherPublicKey = packet.Data;
            var dh = new DiffieHellmanHelper();
            var sharedSecret = dh.DeriveSharedSecret(otherPublicKey);
            var myPublicKey = dh.GetPublicKey();

            _chatService.SetSharedKey(packet.SenderId, sharedSecret);
            _dhInstances[packet.SenderId] = dh;

            // Gửi public key response
            var responsePacket = new Packet(PacketType.KeyExchangeResponse, _currentUserId, packet.SenderId, myPublicKey);
            _tcpService.SendPacket(responsePacket);

            AppendSystem($"🔐 Đã thiết lập kênh mã hoá với {GetUserName(packet.SenderId)}");
        }

        private void HandleKeyExchangeResponse(Packet packet)
        {
            // Nhận public key response → derive shared secret
            DiffieHellmanHelper dh;
            if (_dhInstances.TryGetValue(packet.SenderId, out dh))
            {
                var sharedSecret = dh.DeriveSharedSecret(packet.Data);
                _chatService.SetSharedKey(packet.SenderId, sharedSecret);

                AppendSystem($"🔐 Đã thiết lập kênh mã hoá với {GetUserName(packet.SenderId)}");
            }
        }

        private void HandleFileHeader(Packet packet)
        {
            string fileName = GetJsonField(packet.Data, "FileName");
            string senderName = GetUserName(packet.SenderId);
            AppendSystem($"📎 {senderName} đang gửi file: {fileName}");
        }

        private void HandleFileComplete(Packet packet)
        {
            string senderName = GetUserName(packet.SenderId);
            AppendSystem($"✅ Đã nhận file từ {senderName}");
        }

        private void HandleIncomingCall(string callerUserId)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(HandleIncomingCall), callerUserId);
                return;
            }

            string callerName = GetUserName(callerUserId);
            var result = MessageBox.Show(
                $"📹 {callerName} đang gọi video cho bạn.\nBạn có muốn trả lời?",
                "Cuộc gọi đến", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _videoCallService.AcceptCall(callerUserId);
                AppendSystem($"📹 Đang gọi video với {callerName}");
            }
            else
            {
                _videoCallService.RejectCall(callerUserId);
            }
        }

        private void HandleDisconnected()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HandleDisconnected));
                return;
            }

            AppendSystem("⚠️ Mất kết nối đến server!");
            MessageBox.Show("Mất kết nối đến server!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region UI Events

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedIndex < 0) return;

            int idx = 0;
            foreach (var kvp in _onlineUsers)
            {
                if (idx == lstUsers.SelectedIndex)
                {
                    _selectedUserId = kvp.Key;
                    _selectedUserName = kvp.Value;
                    lblChatWith.Text = $"💬 Chat với {kvp.Value}";

                    // Load lịch sử chat
                    rtbChat.Clear();
                    var history = _historyService.GetHistory(_selectedUserId);
                    foreach (var msg in history)
                    {
                        bool isMine = msg.SenderId == _currentUserId;
                        var color = isMine ? Color.LimeGreen : Color.FromArgb(100, 200, 255);
                        var name = isMine ? _currentDisplayName : msg.SenderName;
                        AppendChat(name, msg.Content, color);
                    }

                    // Khởi tạo DH key exchange nếu chưa có
                    if (!_chatService.HasSharedKey(_selectedUserId))
                    {
                        InitiateKeyExchange(_selectedUserId);
                    }

                    break;
                }
                idx++;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        private void btnSendFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId))
            {
                MessageBox.Show("Vui lòng chọn user để gửi file");
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn file để gửi";
                ofd.Filter = "All Files|*.*|Images|*.jpg;*.png;*.gif;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _fileTransferService.SendFile(_selectedUserId, ofd.FileName);
                    AppendSystem($"📎 Đang gửi file: {System.IO.Path.GetFileName(ofd.FileName)}");
                }
            }
        }

        private void btnVideoCall_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedUserId))
            {
                MessageBox.Show("Vui lòng chọn user để gọi video");
                return;
            }

            _videoCallService.StartCall(_selectedUserId);
            AppendSystem($"📹 Đang gọi video cho {_selectedUserName}...");
        }

        private void frmChat_FormClosing(object sender, FormClosingEventArgs e)
        {
            _tcpService.OnPacketReceived -= HandlePacket;
            _tcpService.OnDisconnected -= HandleDisconnected;

            // Gửi disconnect packet
            var packet = new Packet(PacketType.Disconnect, _currentUserId, null, null);
            _tcpService.SendPacket(packet);
            _tcpService.Disconnect();

            // Dispose DH instances
            foreach (var dh in _dhInstances.Values)
            {
                dh.Dispose();
            }
        }

        #endregion

        #region Helpers

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(txtMessage.Text) || string.IsNullOrEmpty(_selectedUserId))
                return;

            string message = txtMessage.Text.Trim();
            _chatService.SendMessage(_selectedUserId, message);

            // Hiển thị tin nhắn của mình
            AppendChat(_currentDisplayName, message, Color.LimeGreen);

            // Lưu lịch sử
            _historyService.SaveMessage(_selectedUserId, _currentUserId, _currentDisplayName, message, false);

            txtMessage.Clear();
            txtMessage.Focus();
        }

        private void InitiateKeyExchange(string targetUserId)
        {
            var dh = new DiffieHellmanHelper();
            _dhInstances[targetUserId] = dh;

            var publicKey = dh.GetPublicKey();
            var packet = new Packet(PacketType.KeyExchange, _currentUserId, targetUserId, publicKey);
            _tcpService.SendPacket(packet);

            AppendSystem($"🔑 Đang trao đổi khoá mã hoá với {GetUserName(targetUserId)}...");
        }

        private void AppendChat(string sender, string message, Color color)
        {
            var timestamp = DateTime.Now.ToString("HH:mm");

            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;

            // Timestamp
            rtbChat.SelectionColor = Color.Gray;
            rtbChat.AppendText($"[{timestamp}] ");

            // Sender name
            rtbChat.SelectionColor = color;
            rtbChat.AppendText($"{sender}: ");

            // Message
            rtbChat.SelectionColor = Color.White;
            rtbChat.AppendText($"{message}\n");

            rtbChat.ScrollToCaret();
        }

        private void AppendSystem(string message)
        {
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.SelectionColor = Color.FromArgb(255, 200, 100);
            rtbChat.AppendText($"  {message}\n");
            rtbChat.ScrollToCaret();
        }

        private string GetUserName(string userId)
        {
            string name;
            return _onlineUsers.TryGetValue(userId, out name) ? name : userId;
        }

        private string GetJsonField(string json, string field)
        {
            if (string.IsNullOrEmpty(json)) return null;

            var search = "\"" + field + "\":";
            int idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;

            idx += search.Length;
            while (idx < json.Length && json[idx] == ' ') idx++;

            if (idx >= json.Length) return null;

            if (json[idx] == '"')
            {
                idx++;
                var sb = new StringBuilder();
                bool escaped = false;
                while (idx < json.Length)
                {
                    char c = json[idx];
                    if (escaped) { sb.Append(c); escaped = false; }
                    else if (c == '\\') { escaped = true; }
                    else if (c == '"') { break; }
                    else { sb.Append(c); }
                    idx++;
                }
                return sb.ToString();
            }
            else
            {
                var sb = new StringBuilder();
                while (idx < json.Length && json[idx] != ',' && json[idx] != '}')
                {
                    sb.Append(json[idx]);
                    idx++;
                }
                return sb.ToString().Trim();
            }
        }

        #endregion
    }
}
