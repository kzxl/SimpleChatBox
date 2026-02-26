using ChatBox.Shared.Protocol;
using ChatBox.Server.Models;
using System.Collections.Generic;

namespace ChatBox.Server.Services
{
    /// <summary>
    /// Interface route tin nhắn đến đúng client
    /// </summary>
    public interface IMessageRouter
    {
        /// <summary>Gửi packet đến 1 client cụ thể</summary>
        void SendToClient(string userId, Packet packet);

        /// <summary>Gửi packet đến tất cả client (trừ sender)</summary>
        void Broadcast(Packet packet, string excludeUserId = null);

        /// <summary>Gửi danh sách user online cho tất cả client</summary>
        void BroadcastUserList();
    }
}
