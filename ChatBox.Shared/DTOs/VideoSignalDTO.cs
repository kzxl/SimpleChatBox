using System;

namespace ChatBox.Shared.DTOs
{
    /// <summary>
    /// DTO cho tín hiệu video call (signaling)
    /// </summary>
    [Serializable]
    public class VideoSignalDTO
    {
        /// <summary>Loại tín hiệu: Request, Accept, Reject, End</summary>
        public string SignalType { get; set; }

        /// <summary>Địa chỉ IP của caller (để kết nối UDP trực tiếp)</summary>
        public string CallerIp { get; set; }

        /// <summary>Port UDP của caller</summary>
        public int CallerUdpPort { get; set; }

        /// <summary>Thời gian bắt đầu cuộc gọi</summary>
        public DateTime CallStartTime { get; set; }
    }
}
