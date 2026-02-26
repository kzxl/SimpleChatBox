namespace ChatBox.Client.Services
{
    /// <summary>
    /// Interface cho dịch vụ gọi video
    /// </summary>
    public interface IVideoCallService
    {
        /// <summary>Bắt đầu gọi video đến 1 user</summary>
        void StartCall(string targetUserId);

        /// <summary>Chấp nhận cuộc gọi</summary>
        void AcceptCall(string callerUserId);

        /// <summary>Từ chối cuộc gọi</summary>
        void RejectCall(string callerUserId);

        /// <summary>Kết thúc cuộc gọi</summary>
        void EndCall();

        /// <summary>Có đang trong cuộc gọi không</summary>
        bool IsInCall { get; }
    }
}
