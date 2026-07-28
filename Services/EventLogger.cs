using System;
using System.Collections.Generic;

namespace CompanyNotificationApp.Services
{
    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }

        public override string ToString()
        {
            string icon = Level switch
            {
                LogLevel.Info => "ℹ️",
                LogLevel.Success => "✅",
                LogLevel.Warning => "⚠️",
                LogLevel.Error => "❌",
                _ => "•"
            };

            return $"[{Timestamp:HH:mm:ss}] {icon} {Level.ToString().ToUpper()}: {Message}";
        }
    }

    public class EventLogger
    {
        private List<LogEntry> _logs = new List<LogEntry>();
        private static EventLogger _instance;
        private static object _lock = new object();

        public event Action<LogEntry> OnLogAdded;

        private EventLogger()
        {
            LogInfo("EventLogger", "Aplikácia spustená");
        }

        public static EventLogger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new EventLogger();
                        }
                    }
                }
                return _instance;
            }
        }

        public void LogInfo(string category, string message)
        {
            AddLog(LogLevel.Info, category, message);
        }

        public void LogSuccess(string category, string message)
        {
            AddLog(LogLevel.Success, category, message);
        }

        public void LogWarning(string category, string message)
        {
            AddLog(LogLevel.Warning, category, message);
        }

        public void LogError(string category, string message)
        {
            AddLog(LogLevel.Error, category, message);
        }

        private void AddLog(LogLevel level, string category, string message)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Category = category
            };

            lock (_logs)
            {
                _logs.Add(logEntry);
            }

            OnLogAdded?.Invoke(logEntry);
        }

        public List<LogEntry> GetAllLogs()
        {
            lock (_logs)
            {
                return new List<LogEntry>(_logs);
            }
        }

        public void ClearLogs()
        {
            lock (_logs)
            {
                _logs.Clear();
            }
        }
    }
}
