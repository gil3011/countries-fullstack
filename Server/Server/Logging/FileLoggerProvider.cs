using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace Server.Logging
{
    /// <summary>
    /// Minimal file logger provider. Writes log entries to a daily rolling file
    /// under the configured directory (default: "Logs/log-yyyyMMdd.log").
    /// Registered alongside the built-in console logger so entries appear in both places.
    /// </summary>
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;
        private readonly LogLevel _minLevel;
        private readonly int _retentionDays;
        private readonly object _writeLock = new();
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
        private string _currentLogDate;

        public FileLoggerProvider(string logDirectory, LogLevel minLevel, int retentionDays)
        {
            _logDirectory = logDirectory;
            _minLevel = minLevel;
            _retentionDays = retentionDays;
            Directory.CreateDirectory(_logDirectory);

            // Purge stale files at startup, then remember today so the first write
            // of each new day triggers another purge (covers long-running processes).
            _currentLogDate = DateTime.Now.ToString("yyyyMMdd");
            CleanupOldLogs();
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new FileLogger(this, name));

        internal bool IsEnabled(LogLevel level) => level >= _minLevel && level != LogLevel.None;

        internal void Write(LogLevel level, string category, string message, Exception exception)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
              .Append(" [").Append(level.ToString().ToUpperInvariant()).Append("] ")
              .Append(category).Append(" - ").Append(message);

            if (exception != null)
                sb.AppendLine().Append(exception);

            string line = sb.ToString();
            string today = DateTime.Now.ToString("yyyyMMdd");
            string path = Path.Combine(_logDirectory, $"log-{today}.log");

            // File writes are serialized so concurrent requests don't interleave lines.
            lock (_writeLock)
            {
                // The day rolled over since the last write: purge files past retention.
                if (today != _currentLogDate)
                {
                    _currentLogDate = today;
                    CleanupOldLogs();
                }

                try
                {
                    File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // Logging must never crash the request pipeline.
                }
            }
        }

        /// <summary>
        /// Deletes daily log files older than the retention window. The date is parsed
        /// from the file name (log-yyyyMMdd.log) rather than the file system timestamp.
        /// </summary>
        private void CleanupOldLogs()
        {
            if (_retentionDays <= 0)
                return; // Retention disabled: keep everything.

            try
            {
                DateTime cutoff = DateTime.Now.Date.AddDays(-_retentionDays);

                foreach (string file in Directory.GetFiles(_logDirectory, "log-*.log"))
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    string datePart = name.Length >= 8 ? name[^8..] : string.Empty;

                    if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime fileDate)
                        && fileDate < cutoff)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // Cleanup must never crash logging.
            }
        }

        public void Dispose() => _loggers.Clear();

        private sealed class FileLogger : ILogger
        {
            private readonly FileLoggerProvider _provider;
            private readonly string _category;

            public FileLogger(FileLoggerProvider provider, string category)
            {
                _provider = provider;
                _category = category;
            }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => _provider.IsEnabled(logLevel);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;

                string message = formatter(state, exception);
                _provider.Write(logLevel, _category, message, exception);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    public static class FileLoggerExtensions
    {
        /// <summary>
        /// Adds the daily rolling file logger to the logging pipeline. Files older than
        /// <paramref name="retentionDays"/> days are deleted (set to 0 to keep forever).
        /// </summary>
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder,
            string logDirectory = "Logs", LogLevel minLevel = LogLevel.Information, int retentionDays = 30)
        {
            builder.AddProvider(new FileLoggerProvider(logDirectory, minLevel, retentionDays));
            return builder;
        }
    }
}
