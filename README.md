# 💬 SimpleChatBox

Ứng dụng Chat thời gian thực với **Server - Multi Client** trên nền Windows Forms C# (.NET Framework 4.8).

## ✨ Tính năng

- **Đăng ký / Đăng nhập** — Tài khoản lưu trên server (SHA256 password hash)
- **Chat 1-1** — Nhắn tin trực tiếp giữa 2 user
- **Mã hoá End-to-End** — Diffie-Hellman key exchange + AES-256-CBC
- **Truyền file** — Chunk-based, hỗ trợ mở file sau khi nhận
- **Video Call** — Signaling qua TCP, popup cửa sổ video (skeleton cho camera capture)
- **Ghi hình cuộc gọi** — VideoRecorder skeleton (cần tích hợp AForge FFMPEG)
- **Lịch sử tin nhắn** — Lưu local trên client (JSON per conversation)
- **Danh sách Online** — Tự động cập nhật khi user login/logout
- **Disconnect Detection** — Tự kết thúc video call khi đối phương offline

## 🏗️ Kiến trúc

```
ChatBoxSimple.sln
├── ChatBox.Shared        # Class Library — DTO, Protocol, Crypto, Constants
│   ├── Constants/        # AppConstants (ports, buffer sizes)
│   ├── DTOs/             # LoginRequest, LoginResponse, Message, FileTransfer, VideoSignal
│   ├── Protocol/         # PacketType, Packet, PacketSerializer (length-prefixed TCP)
│   └── Crypto/           # AesHelper (AES-256-CBC), DiffieHellmanHelper (ECDH)
│
├── ChatBox.Server        # WinForms App — TCP Server
│   ├── Data/             # UserStore (JSON file storage)
│   ├── Models/           # UserAccount, ConnectedClient
│   ├── Services/         # AuthService, TcpServerService, MessageRouter
│   └── Forms/            # frmServer (dashboard)
│
└── ChatBox.Client        # WinForms App — TCP Client
    ├── Services/         # TcpClient, Chat, FileTransfer, FileReceive, VideoCall, MessageHistory
    ├── Helpers/          # VideoRecorder
    └── Forms/            # frmLogin, frmChat, frmVideoCall
```

## 🔒 Bảo mật

| Layer | Công nghệ |
|-------|-----------|
| Password | SHA-256 hash |
| Key Exchange | ECDH (ECDiffieHellmanCng) |
| Message Encryption | AES-256-CBC (IV tự động prepend) |

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
- **Network**: `System.Net.Sockets` (TCP)
- **Crypto**: `System.Security.Cryptography` (AES, ECDH, SHA256)
- **Pattern**: Interface-first Services, DTO-based communication

## 📋 Roadmap

- [ ] Tích hợp AForge.Video.DirectShow cho camera capture
- [ ] UDP video/audio streaming
- [ ] NAudio cho audio capture
- [ ] Group chat
- [ ] Emoji & rich text
- [ ] UI polish & themes

## 📄 License

MIT
