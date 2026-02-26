using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using ChatBox.Server.Models;
using ChatBox.Shared.Protocol;

namespace ChatBox.Server.Services
{
    /// <summary>
    /// Route tin nhắn đến đúng client hoặc broadcast
    /// </summary>
    public class MessageRouter : IMessageRouter
    {
        private readonly ConcurrentDictionary<string, ConnectedClient> _clients;
        private readonly Action<string> _log;

        public MessageRouter(ConcurrentDictionary<string, ConnectedClient> clients, Action<string> log)
        {
            _clients = clients;
            _log = log;
        }

        /// <summary>
        /// Gửi packet đến 1 client cụ thể
        /// </summary>
        public void SendToClient(string userId, Packet packet)
        {
            ConnectedClient client;
            if (_clients.TryGetValue(userId, out client) && client.IsAuthenticated)
            {
                try
                {
                    PacketSerializer.SendPacket(client.Stream, packet);
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"[ERROR] Gửi packet đến {userId} thất bại: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gửi packet đến tất cả client (trừ sender)
        /// </summary>
        public void Broadcast(Packet packet, string excludeUserId = null)
        {
            foreach (var kvp in _clients)
            {
                if (kvp.Key == excludeUserId) continue;
                if (!kvp.Value.IsAuthenticated) continue;

                try
                {
                    PacketSerializer.SendPacket(kvp.Value.Stream, packet);
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"[ERROR] Broadcast đến {kvp.Key} thất bại: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gửi danh sách user online cho tất cả client.
        /// Data = "user1|DisplayName1,user2|DisplayName2,..."
        /// </summary>
        public void BroadcastUserList()
        {
            var onlineUsers = _clients.Values
                .Where(c => c.IsAuthenticated)
                .Select(c => c.UserId + "|" + c.DisplayName);

            var userListData = string.Join(",", onlineUsers);
            var packet = new Packet(PacketType.UserList, "server", null, userListData);

            Broadcast(packet);
        }
    }
}
