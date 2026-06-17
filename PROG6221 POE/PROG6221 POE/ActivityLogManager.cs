using System;
using System.Collections.Generic;
using System.Linq;

namespace PROG6221_POE
{
    public class ActivityLogManager
    {
        private List<LogEntry> _logEntries;
        private int _maxDisplayCount = 10;

        public event Action<string> OnLogUpdated;

        public ActivityLogManager()
        {
            _logEntries = new List<LogEntry>();
        }

        public void AddEntry(string action, string details = "")
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Action = action,
                Details = details
            };
            _logEntries.Add(entry);
            OnLogUpdated?.Invoke($"📝 {action}: {details}");
        }

        public List<LogEntry> GetRecentEntries(int count = 10)
        {
            return _logEntries
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList();
        }

        public string GetLogSummary(int count = 10)
        {
            var entries = GetRecentEntries(count);
            if (!entries.Any())
                return "No activities logged yet.";

            string summary = "📋 **Recent Activity Log** 📋\n\n";
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                summary += $"{i + 1}. [{entry.Timestamp:HH:mm}] {entry.Action}";
                if (!string.IsNullOrEmpty(entry.Details))
                    summary += $" - {entry.Details}";
                summary += "\n";
            }
            return summary;
        }

        public int GetTotalCount() => _logEntries.Count;

        public void ClearLog()
        {
            _logEntries.Clear();
            OnLogUpdated?.Invoke("🗑️ Activity log cleared");
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
    }
}