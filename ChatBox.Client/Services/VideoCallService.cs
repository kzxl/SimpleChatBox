using System;
using ChatBox.Shared.Protocol;

namespace ChatBox.Client.Services
{
    /// <summary>
    /// Quản lý gọi video: signaling qua TCP, streaming qua UDP.
    /// Sử dụng AForge hoặc DirectShow cho camera capture.
    /// (Skeleton - sẽ implement chi tiết ở Phase 4)
    /// </summary>
    public class VideoCallService : IVideoCallService
    {
        private readonly TcpClientService _tcpService;
        private readonly ChatService _chatService;

        public bool IsInCall { get; private set; }
        public string CurrentCallPartner { get; private set; }

        public event Action<string> OnIncomingCall;    // callerUserId
        public event Action OnCallAccepted;
        public event Action<string> OnCallRejected;    // reason
        public event Action OnCallEnded;
        public event Action<byte[]> OnVideoFrameReceived;

        public VideoCallService(TcpClientService tcpService, ChatService chatService)
        {
            _tcpService = tcpService;
            _chatService = chatService;
        }

        /// <summary>
        /// Gửi yêu cầu gọi video
        /// </summary>
        public void StartCall(string targetUserId)
        {
            if (IsInCall) return;

            var data = string.Format(
                "{{\"SignalType\":\"Request\",\"CallerUdpPort\":0}}");

            var packet = new Packet(PacketType.VideoCallRequest, _chatService.CurrentUserId, targetUserId, data);
            _tcpService.SendPacket(packet);

            CurrentCallPartner = targetUserId;
        }

        /// <summary>
        /// Chấp nhận cuộc gọi
        /// </summary>
        public void AcceptCall(string callerUserId)
        {
            var data = "{\"SignalType\":\"Accept\"}";
            var packet = new Packet(PacketType.VideoCallAccept, _chatService.CurrentUserId, callerUserId, data);
            _tcpService.SendPacket(packet);

            IsInCall = true;
            CurrentCallPartner = callerUserId;
            OnCallAccepted?.Invoke();
        }

        /// <summary>
        /// Từ chối cuộc gọi
        /// </summary>
        public void RejectCall(string callerUserId)
        {
            var data = "{\"SignalType\":\"Reject\"}";
            var packet = new Packet(PacketType.VideoCallReject, _chatService.CurrentUserId, callerUserId, data);
            _tcpService.SendPacket(packet);
        }

        /// <summary>
        /// Kết thúc cuộc gọi
        /// </summary>
        public void EndCall()
        {
            if (!IsInCall) return;

            var data = "{\"SignalType\":\"End\"}";
            var packet = new Packet(PacketType.VideoCallEnd, _chatService.CurrentUserId, CurrentCallPartner, data);
            _tcpService.SendPacket(packet);

            IsInCall = false;
            CurrentCallPartner = null;
            OnCallEnded?.Invoke();
        }

        /// <summary>
        /// Xử lý signal nhận được từ server
        /// </summary>
        public void HandleVideoSignal(Packet packet)
        {
            switch (packet.Type)
            {
                case PacketType.VideoCallRequest:
                    OnIncomingCall?.Invoke(packet.SenderId);
                    break;

                case PacketType.VideoCallAccept:
                    IsInCall = true;
                    CurrentCallPartner = packet.SenderId;
                    OnCallAccepted?.Invoke();
                    break;

                case PacketType.VideoCallReject:
                    OnCallRejected?.Invoke("Cuộc gọi bị từ chối");
                    CurrentCallPartner = null;
                    break;

                case PacketType.VideoCallEnd:
                    IsInCall = false;
                    CurrentCallPartner = null;
                    OnCallEnded?.Invoke();
                    break;

                case PacketType.VideoFrame:
                    if (!string.IsNullOrEmpty(packet.Data))
                    {
                        try
                        {
                            var frameData = Convert.FromBase64String(packet.Data);
                            OnVideoFrameReceived?.Invoke(frameData);
                        }
                        catch { }
                    }
                    break;
            }
        }
    }
}
