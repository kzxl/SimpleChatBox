using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ChatBox.Shared.Protocol
{
    /// <summary>
    /// Serialize/Deserialize Packet qua NetworkStream.
    /// Format: [4 bytes - payload length (BigEndian)][N bytes - JSON payload]
    /// </summary>
    public static class PacketSerializer
    {
        /// <summary>
        /// Serialize packet thành JSON string
        /// </summary>
        public static string Serialize(Packet packet)
        {
            // Simple JSON serialization without Newtonsoft dependency
            // Using basic string building for .NET Framework compatibility
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"Type\":{0}", (int)packet.Type);
            sb.AppendFormat(",\"SenderId\":{0}", JsonEscape(packet.SenderId));
            sb.AppendFormat(",\"ReceiverId\":{0}", JsonEscape(packet.ReceiverId));
            sb.AppendFormat(",\"Data\":{0}", JsonEscape(packet.Data));
            sb.AppendFormat(",\"Timestamp\":\"{0:O}\"", packet.Timestamp);
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Deserialize JSON string thành Packet
        /// </summary>
        public static Packet Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            var packet = new Packet();
            // Simple JSON parsing
            packet.Type = (PacketType)GetIntValue(json, "Type");
            packet.SenderId = GetStringValue(json, "SenderId");
            packet.ReceiverId = GetStringValue(json, "ReceiverId");
            packet.Data = GetStringValue(json, "Data");

            var timestampStr = GetStringValue(json, "Timestamp");
            if (!string.IsNullOrEmpty(timestampStr))
            {
                DateTime ts;
                if (DateTime.TryParse(timestampStr, out ts))
                    packet.Timestamp = ts;
            }

            return packet;
        }

        /// <summary>
        /// Gửi packet qua NetworkStream (length-prefixed)
        /// </summary>
        public static void SendPacket(NetworkStream stream, Packet packet)
        {
            var json = Serialize(packet);
            var payload = Encoding.UTF8.GetBytes(json);
            var lengthBytes = BitConverter.GetBytes(payload.Length);

            // Ensure BigEndian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);

            stream.Write(lengthBytes, 0, 4);
            stream.Write(payload, 0, payload.Length);
            stream.Flush();
        }

        /// <summary>
        /// Đọc packet từ NetworkStream (length-prefixed)
        /// </summary>
        public static Packet ReceivePacket(NetworkStream stream)
        {
            // Read length prefix (4 bytes)
            var lengthBytes = ReadExact(stream, 4);
            if (lengthBytes == null)
                return null;

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lengthBytes);

            int length = BitConverter.ToInt32(lengthBytes, 0);

            if (length <= 0 || length > Constants.AppConstants.MaxPacketSize)
                return null;

            // Read payload
            var payloadBytes = ReadExact(stream, length);
            if (payloadBytes == null)
                return null;

            var json = Encoding.UTF8.GetString(payloadBytes);
            return Deserialize(json);
        }

        /// <summary>
        /// Đọc đúng N bytes từ stream
        /// </summary>
        private static byte[] ReadExact(NetworkStream stream, int count)
        {
            var buffer = new byte[count];
            int totalRead = 0;

            while (totalRead < count)
            {
                int bytesRead = stream.Read(buffer, totalRead, count - totalRead);
                if (bytesRead == 0)
                    return null; // Connection closed

                totalRead += bytesRead;
            }

            return buffer;
        }

        #region Simple JSON Helpers

        private static string JsonEscape(string value)
        {
            if (value == null) return "null";

            var sb = new StringBuilder("\"");
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }
            sb.Append("\"");
            return sb.ToString();
        }

        private static int GetIntValue(string json, string key)
        {
            var search = "\"" + key + "\":";
            int idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return 0;

            idx += search.Length;
            var sb = new StringBuilder();
            while (idx < json.Length && (char.IsDigit(json[idx]) || json[idx] == '-'))
            {
                sb.Append(json[idx]);
                idx++;
            }

            int result;
            return int.TryParse(sb.ToString(), out result) ? result : 0;
        }

        private static string GetStringValue(string json, string key)
        {
            var search = "\"" + key + "\":";
            int idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;

            idx += search.Length;

            // Skip whitespace
            while (idx < json.Length && json[idx] == ' ') idx++;

            if (idx >= json.Length) return null;

            // Check for null
            if (json[idx] == 'n')
                return null;

            if (json[idx] != '"')
                return null;

            idx++; // skip opening quote
            var sb = new StringBuilder();
            bool escaped = false;
            while (idx < json.Length)
            {
                char c = json[idx];
                if (escaped)
                {
                    switch (c)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        default: sb.Append(c); break;
                    }
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    break;
                }
                else
                {
                    sb.Append(c);
                }
                idx++;
            }

            return sb.ToString();
        }

        #endregion
    }
}
