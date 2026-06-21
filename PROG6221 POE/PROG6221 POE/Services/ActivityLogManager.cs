using System;
using System.Collections.Generic;
using System.Linq;

namespace PROG6221_POE.Services
{
    public class ActivityLogManager
    {
        private List<LogEntry> _logEntries;

        public event Action<string> OnLogUpdated;

        public ActivityLogManager()
        {
            _logEntries = new List<LogEntry>();
            // Add initial entry
            AddEntry("System Started", "Activity log initialized");
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
            OnLogUpdated?.Invoke($"{action}: {details}");

            // Debug output
            System.Diagnostics.Debug.WriteLine($"Log Entry Added: [{entry.Timestamp:HH:mm:ss}] {action} - {details}");
        }

        public List<LogEntry> GetRecentEntries(int count = 10)
        {
            return _logEntries
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToList();
        }

        public List<LogEntry> GetAllEntries()
        {
            return _logEntries
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        public string GetLogSummary(int count = 10)
        {
            var entries = GetRecentEntries(count);
            if (!entries.Any())
                return "No activities logged yet.";

            string summary = "RECENT ACTIVITY LOG\n\n";
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                summary += $"{i + 1}. [{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Action}";
                if (!string.IsNullOrEmpty(entry.Details))
                    summary += $" - {entry.Details}";
                summary += "\n";
            }

            int totalCount = _logEntries.Count;
            if (totalCount > count)
            {
                summary += $"\nShowing {count} of {totalCount} entries.";
            }
            else
            {
                summary += $"\nShowing all {totalCount} entries.";
            }

            return summary;
        }

        public string GetFullLogSummary()
        {
            var entries = GetAllEntries();
            if (!entries.Any())
                return "No activities logged yet.";

            string summary = "COMPLETE ACTIVITY LOG\n";
            summary += $"Total Entries: {entries.Count}\n";
            summary += "==================================================\n\n";

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                summary += $"{i + 1}. [{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {entry.Action}";
                if (!string.IsNullOrEmpty(entry.Details))
                    summary += $" - {entry.Details}";
                summary += "\n";
            }

            summary += "\n==================================================\n";
            summary += $"End of Activity Log ({entries.Count} entries)";

            return summary;
        }

        public int GetTotalCount() => _logEntries.Count;

        public void ClearLog()
        {
            _logEntries.Clear();
            AddEntry("Log Cleared", "All activity entries removed");
            OnLogUpdated?.Invoke("Activity log cleared");
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
    }
}