# 💬 SimpleChatBox

Ứng dụng Chat thời gian thực với **Server - Multi Client** trên nền Windows Forms C# (.NET Framework 4.8).  
Hỗ trợ **P2P Video Call** qua internet với STUN NAT traversal.

## ✨ Tính năng

### 💬 Chat
- **Chat 1-1** — Nhắn tin trực tiếp giữa 2 user, mã hoá End-to-End
- **Chat nhóm** — Broadcast tin nhắn cho tất cả user online
- **Emoji picker** — 24 emoji phổ biến, bấm 😀 để chọn
- **Typing indicator** — Hiển thị "đang nhập..." khi đối phương gõ
- **Lịch sử chat** — Lưu trên **server**, tự động load khi chọn user
- **Unread badges** — Đếm tin nhắn chưa đọc cạnh tên user
- **Sound notification** — Âm thanh khi nhận tin nhắn mới

### 📎 File Transfer
- **Gửi file** — Chunk-based, hỗ trợ mọi loại file
- **Nhận file** — Tự động lưu vào `Downloads/`, hỏi mở sau khi nhận

### 📹 Video Call (P2P)
- **STUN NAT traversal** — Hoạt động qua internet
- **UDP Hole Punching** — Kết nối P2P trực tiếp
- **Fallback relay** — Tự chuyển qua server nếu P2P thất bại
- **Popup cửa sổ** — frmVideoCall hiện khi gọi/nhận

### 🔐 Bảo mật
- **Đăng ký / Đăng nhập** — SHA256 password hash
- **Diffie-Hellman** — ECDH key exchange
- **AES-256-CBC** — Mã hoá tin nhắn End-to-End

### ⚡ Hệ thống
- **Online/Offline detection** — Tự động cập nhật danh sách user
- **Disconnect handling** — Tự kết thúc video call khi đối phương offline
- **Heartbeat** — Đảm bảo user list luôn đồng bộ

## 🏗️ Kiến trúc

```
ChatBoxSimple.sln
├── ChatBox.Shared        # Class Library — DTO, Protocol, Crypto, Network
│   ├── Constants/        # AppConstants (ports, buffer sizes)
│   ├── DTOs/             # Login, Message, FileTransfer, VideoSignal
│   ├── Protocol/         # PacketType, Packet, PacketSerializer
│   ├── Crypto/           # AesHelper (AES-256-CBC), DiffieHellmanHelper (ECDH)
│   └── Network/          # StunClient (RFC 5389 NAT traversal)
│
├── ChatBox.Server        # WinForms App — TCP Server
│   ├── Data/             # UserStore (accounts), MessageStore (chat history)
│   ├── Models/           # UserAccount, ConnectedClient
│   ├── Services/         # AuthService, TcpServerService, MessageRouter
│   └── Forms/            # frmServer (dashboard)
│
└── ChatBox.Client        # WinForms App — TCP Client
    ├── Services/         # TcpClient, Chat, FileTransfer, FileReceive,
    │                     # VideoCall, UdpPeer, MessageHistory
    ├── Helpers/          # VideoRecorder
    └── Forms/            # frmLogin, frmChat, frmVideoCall
```

## 📹 P2P Video Call Flow

```
┌─────────────────────────────────────────────────────┐
│  Client A          Server          Client B         │
│     │                │                │              │
│     ├──STUN──→ Google STUN Server                   │
│     │    (discover public IP:Port)                   │
│     │                                               │
│     ├──Request──→│──Forward──→│                      │
│     │            │     ←──Accept──┤                  │
│     │   ←──Forward──┤                │              │
│     │                                               │
│     ├═══════ UDP Hole Punching ═══════┤             │
│     │                                               │
│     │  ✅ Success → P2P UDP Streaming                │
│     │  ❌ Fail    → Fallback TCP Relay               │
└─────────────────────────────────────────────────────┘
```

## 🚀 Hướng dẫn sử dụng

### Yêu cầu
- Windows 10+
- .NET Framework 4.8
- Visual Studio 2022 (hoặc MSBuild)

### Build
```bash
MSBuild.exe ChatBoxSimple.sln /p:Configuration=Debug
# Hoặc: Visual Studio → Build Solution (Ctrl+Shift+B)
```

### Bước 1: Khởi động Server
1. Chạy `ChatBox.Server/bin/Debug/ChatBox.Server.exe`
2. Chọn port (mặc định: **9000**)
3. Bấm **▶ Khởi động**
4. Server sẵn sàng, hiển thị log kết nối

### Bước 2: Khởi động Client
1. Chạy `ChatBox.Client/bin/Debug/ChatBox.Client.exe` (có thể chạy **nhiều instance**)
2. Nhập **Server**: `127.0.0.1` (LAN) hoặc IP public (internet)
3. Nhập **Port**: `9000`
4. **Đăng ký** tài khoản mới hoặc **Đăng nhập** tài khoản có sẵn

### Bước 3: Sử dụng
| Tính năng | Cách dùng |
|-----------|-----------|
| **Chat 1-1** | Chọn user trong danh sách → Gõ tin nhắn → Enter hoặc "Gửi ➤" |
| **Chat nhóm** | Bấm "📢 Chat nhóm" → Gõ tin nhắn (gửi cho tất cả) |
| **Emoji** | Bấm 😀 trên thanh chat → Chọn emoji |
| **Gửi file** | Bấm 📎 → Chọn file → File tự động gửi |
| **Video call** | Chọn user → Bấm 📹 → Đợi đối phương chấp nhận |
| **Lịch sử** | Chọn user → Lịch sử tự động load từ server |

### Chạy qua Internet
1. Server: **mở port 9000** trên router (port forwarding)
2. Client: nhập **IP public** của server khi đăng nhập
3. Video call tự động dùng **STUN** để kết nối P2P qua NAT

## 📁 Dữ liệu Server

| File/Folder | Mô tả |
|-------------|-------|
| `users.json` | Danh sách tài khoản đã đăng ký |
| `ChatData/` | Lịch sử chat (JSON per conversation, max 500 tin/conversation) |

## 📁 Dữ liệu Client

| File/Folder | Mô tả |
|-------------|-------|
| `Downloads/` | File nhận được từ người khác |
| `Recordings/` | Video ghi hình cuộc gọi |

## 🛠️ Công nghệ

- **Framework**: .NET Framework 4.8
- **UI**: Windows Forms (dark theme)
- **Network**: `System.Net.Sockets` (TCP + UDP)
- **NAT Traversal**: STUN (RFC 5389) + UDP Hole Punching
- **Crypto**: `System.Security.Cryptography` (AES, ECDH, SHA256)
- **Pattern**: Interface-first Services, DTO-based communication

## 📋 Roadmap

- [ ] Tích hợp AForge.Video.DirectShow cho camera capture
- [ ] NAudio cho audio capture
- [ ] UI themes (light/dark switch)

## 📄 License

MIT — Xem file [LICENSE](LICENSE)
