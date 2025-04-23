using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ITEQ2.Logging
{
    public class LogChanges
    {
        private static readonly string SharedLogFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ITEQ2", "change_log.json");

        private static readonly List<LogEntry> _pendingLogs = new();

        public static void AddChange(string fieldName, string oldValue, string newValue, string itemId)
        {
            _pendingLogs.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                User = Properties.Settings.Default.User,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                ItemId = itemId,
                ChangeType = "FieldUpdate"
            });
        }

        public static void SaveLog()
        {
            if (_pendingLogs.Count == 0) return;

            Directory.CreateDirectory(Path.GetDirectoryName(SharedLogFilePath));

            List<LogEntry> existing = new();

            if (File.Exists(SharedLogFilePath))
            {
                var content = File.ReadAllText(SharedLogFilePath);
                existing = JsonSerializer.Deserialize<List<LogEntry>>(content) ?? new();
            }

            existing.AddRange(_pendingLogs);

            var updatedJson = JsonSerializer.Serialize(existing, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SharedLogFilePath, updatedJson);

            _pendingLogs.Clear();
        }

        public class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string User { get; set; }
            public string ItemId { get; set; }
            public string FieldName { get; set; }
            public string OldValue { get; set; }
            public string NewValue { get; set; }
            public string ChangeType { get; set; }
        }
        public static List<LogEntry> LoadLogsForItem(string itemId)
        {
            if (!File.Exists(SharedLogFilePath))
                return new List<LogEntry>();

            var json = File.ReadAllText(SharedLogFilePath);
            var allLogs = JsonSerializer.Deserialize<List<LogEntry>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            foreach (var log in allLogs)
            {
                Debug.WriteLine($"{log.Timestamp} - {log.User} changed {log.FieldName}: {log.OldValue} → {log.NewValue}");
            }

            return allLogs
                .Where(log => log.ItemId == itemId)
                .OrderByDescending(log => log.Timestamp)
                .ToList();
        }
    }
}
