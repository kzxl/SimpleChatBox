using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ChatBox.Server.Models;

namespace ChatBox.Server.Data
{
    /// <summary>
    /// Lưu trữ user accounts trên server.
    /// Dùng file JSON đơn giản (có thể nâng cấp lên SQLite sau).
    /// </summary>
    public class UserStore
    {
        private readonly string _filePath;
        private List<UserAccount> _users;
        private readonly object _lock = new object();

        public UserStore(string filePath = null)
        {
            _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.json");
            LoadUsers();
        }

        /// <summary>
        /// Xác thực user bằng username và password hash
        /// </summary>
        public UserAccount Authenticate(string username, string passwordHash)
        {
            lock (_lock)
            {
                return _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.PasswordHash == passwordHash);
            }
        }

        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        public bool Register(string username, string passwordHash, string displayName)
        {
            lock (_lock)
            {
                // Kiểm tra trùng username
                if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                    return false;

                var user = new UserAccount
                {
                    UserId = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Username = username,
                    PasswordHash = passwordHash,
                    DisplayName = displayName ?? username,
                    CreatedAt = DateTime.Now
                };

                _users.Add(user);
                SaveUsers();
                return true;
            }
        }

        /// <summary>
        /// Lấy user theo username
        /// </summary>
        public UserAccount GetByUsername(string username)
        {
            lock (_lock)
            {
                return _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Hash password bằng SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        private void LoadUsers()
        {
            _users = new List<UserAccount>();

            if (!File.Exists(_filePath))
                return;

            try
            {
                var json = File.ReadAllText(_filePath);
                // Simple JSON array parsing
                _users = SimpleJsonDeserializeUsers(json);
            }
            catch
            {
                _users = new List<UserAccount>();
            }
        }

        private void SaveUsers()
        {
            try
            {
                var json = SimpleJsonSerializeUsers(_users);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving users: {ex.Message}");
            }
        }

        #region Simple JSON Serialization

        private string SimpleJsonSerializeUsers(List<UserAccount> users)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < users.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var u = users[i];
                sb.Append("{");
                sb.AppendFormat("\"UserId\":\"{0}\"", Escape(u.UserId));
                sb.AppendFormat(",\"Username\":\"{0}\"", Escape(u.Username));
                sb.AppendFormat(",\"PasswordHash\":\"{0}\"", Escape(u.PasswordHash));
                sb.AppendFormat(",\"DisplayName\":\"{0}\"", Escape(u.DisplayName));
                sb.AppendFormat(",\"CreatedAt\":\"{0:O}\"", u.CreatedAt);
                sb.Append("}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private List<UserAccount> SimpleJsonDeserializeUsers(string json)
        {
            var result = new List<UserAccount>();
            if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
                return result;

            // Split by },{
            json = json.Trim().TrimStart('[').TrimEnd(']');
            var objects = SplitJsonObjects(json);

            foreach (var obj in objects)
            {
                var user = new UserAccount();
                user.UserId = ExtractValue(obj, "UserId");
                user.Username = ExtractValue(obj, "Username");
                user.PasswordHash = ExtractValue(obj, "PasswordHash");
                user.DisplayName = ExtractValue(obj, "DisplayName");

                var createdStr = ExtractValue(obj, "CreatedAt");
                DateTime dt;
                if (DateTime.TryParse(createdStr, out dt))
                    user.CreatedAt = dt;

                result.Add(user);
            }

            return result;
        }

        private List<string> SplitJsonObjects(string json)
        {
            var objects = new List<string>();
            int depth = 0;
            int start = 0;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{') depth++;
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        objects.Add(json.Substring(start, i - start + 1));
                        start = i + 1;
                        // Skip comma
                        while (start < json.Length && (json[start] == ',' || json[start] == ' '))
                            start++;
                    }
                }
            }

            return objects;
        }

        private string ExtractValue(string json, string key)
        {
            var search = "\"" + key + "\":\"";
            int idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;

            idx += search.Length;
            int end = json.IndexOf('"', idx);
            if (end < 0) return null;

            return Unescape(json.Substring(idx, end - idx));
        }

        private string Escape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private string Unescape(string s)
        {
            if (s == null) return "";
            return s.Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        #endregion
    }
}
