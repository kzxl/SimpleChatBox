# 💬 SimpleChatBox

Ứng dụng Chat thời gian thực với **Server - Multi Client** trên nền Windows Forms C# (.NET Framework 4.8).  
Hỗ trợ **P2P Video Call** qua internet với STUN NAT traversal.

## ✨ Tính năng

- **Đăng ký / Đăng nhập** — Tài khoản lưu trên server (SHA256 password hash)
- **Chat 1-1** — Nhắn tin trực tiếp giữa 2 user
- **Mã hoá End-to-End** — Diffie-Hellman key exchange + AES-256-CBC
- **Truyền file** — Chunk-based, lưu vào `Downloads/`, hỏi mở file sau khi nhận
- **P2P Video Call** — STUN NAT traversal + UDP hole punching, fallback server relay
- **Ghi hình cuộc gọi** — VideoRecorder (skeleton, cần AForge FFMPEG)
- **Lịch sử tin nhắn** — Lưu local trên client (JSON per conversation)
- **Danh sách Online** — Tự động cập nhật khi user login/logout
- **Disconnect Detection** — Tự kết thúc video call khi đối phương offline

## 🏗️ Kiến trúc

```
ChatBoxSimple.sln
├── ChatBox.Shared        # Class Library — DTO, Protocol, Crypto, Network
│   ├── Constants/        # AppConstants (ports, buffer sizes)
│   ├── DTOs/             # LoginRequest, LoginResponse, Message, FileTransfer, VideoSignal
│   ├── Protocol/         # PacketType, Packet, PacketSerializer (length-prefixed TCP)
│   ├── Crypto/           # AesHelper (AES-256-CBC), DiffieHellmanHelper (ECDH)
│   └── Network/          # StunClient (RFC 5389 NAT traversal)
│
├── ChatBox.Server        # WinForms App — TCP Server
│   ├── Data/             # UserStore (JSON file storage)
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

## 📹 Video Call — P2P Architecture

```
┌─────────────────────────────────────────────────────┐
│                    Call Flow                         │
│                                                     │
│  Client A          Server          Client B         │
│     │                │                │              │
│     ├──STUN──→ Google STUN Server                   │
│     │    (discover public IP:Port)                   │
│     │                                               │
│     ├──Request──→│                │                  │
│     │            ├──Forward──→    │                  │
│     │            │          ←──Accept──┤             │
│     │   ←──Forward──┤                │              │
│     │                                               │
│     ├═══════ UDP Hole Punching ═══════┤             │
│     │    (try public + local endpoint)  │            │
│     │                                               │
│     │  ✅ Success → P2P UDP Streaming                │
│     │  ❌ Fail    → Fallback TCP Relay               │
└─────────────────────────────────────────────────────┘
```

| Scenario | Connection Type | Latency |
|----------|----------------|---------|
| Cùng LAN | UDP P2P (local IP) | ~1ms |
| Khác mạng (NAT) | UDP P2P (STUN hole punch) | ~20-50ms |
| Symmetric NAT / Firewall block | TCP Server Relay (fallback) | ~50-100ms |

## 🔒 Bảo mật

| Layer | Công nghệ |
|-------|-----------|
| Password | SHA-256 hash |
| Key Exchange | ECDH (ECDiffieHellmanCng) |
| Message Encryption | AES-256-CBC (IV tự động prepend) |
| NAT Discovery | STUN RFC 5389 (Google STUN servers) |

## 📡 Protocol

Giao thức TCP tự xây dựng, **length-prefixed JSON**:

```
[4 bytes: payload length (Big Endian)] [N bytes: JSON payload]
```

Mỗi packet chứa: `Type`, `SenderId`, `ReceiverId`, `Data`, `Timestamp`.

## 🚀 Hướng dẫn chạy

### Yêu cầu
- Windows 10+
- .NET Framework 4.8
- Visual Studio 2022 (hoặc MSBuild)

### Build
```bash
# MSBuild
MSBuild.exe ChatBoxSimple.sln /p:Configuration=Debug

# Hoặc mở ChatBoxSimple.sln trong Visual Studio → Build Solution
```

### Chạy
1. **Khởi động Server**: Chạy `ChatBox.Server.exe` → Bấm **▶ Khởi động** (port mặc định: 9000)
2. **Khởi động Client**: Chạy `ChatBox.Client.exe` (có thể chạy nhiều instance)
3. **Đăng ký** tài khoản mới → **Đăng nhập**
4. Chọn user trong danh sách Online → **Chat / Gửi file / Video Call**

### Chạy qua Internet
1. Server cần **mở port 9000** trên router (port forwarding)
2. Client nhập **IP public** của server khi đăng nhập
3. Video call tự động dùng **STUN** để kết nối P2P qua NAT

## 📁 Cấu trúc dữ liệu

| File | Vị trí | Mô tả |
|------|--------|-------|
| `users.json` | Server `bin/` | Danh sách tài khoản |
| `ChatHistory/` | Client `bin/` | Lịch sử tin nhắn (per conversation) |
| `Downloads/` | Client `bin/` | File nhận được từ người khác |
| `Recordings/` | Client `bin/` | Video ghi hình cuộc gọi |

## 🛠️ Công nghệ

- **Framework**: .NET Framework 4.8
- **UI**: Windows Forms
- **Network**: `System.Net.Sockets` (TCP + UDP)
- **NAT Traversal**: STUN (RFC 5389) + UDP Hole Punching
- **Crypto**: `System.Security.Cryptography` (AES, ECDH, SHA256)
- **Pattern**: Interface-first Services, DTO-based communication

## 📋 Roadmap

- [ ] Tích hợp AForge.Video.DirectShow cho camera capture
- [ ] NAudio cho audio capture
- [ ] Group chat
- [ ] Emoji & rich text
- [ ] UI polish & themes

## 📄 License

MIT — Xem file [LICENSE](LICENSE)
