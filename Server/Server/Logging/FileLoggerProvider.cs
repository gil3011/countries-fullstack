using System.Collections.Concurrent;
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
        private readonly object _writeLock = new();
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

        public FileLoggerProvider(string logDirectory, LogLevel minLevel)
        {
            _logDirectory = logDirectory;
            _minLevel = minLevel;
            Directory.CreateDirectory(_logDirectory);
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
            string path = Path.Combine(_logDirectory, $"log-{DateTime.Now:yyyyMMdd}.log");

            // File writes are serialized so concurrent requests don't interleave lines.
            lock (_writeLock)
            {
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
        /// Adds the daily rolling file logger to the logging pipeline.
        /// </summary>
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder,
            string logDirectory = "Logs", LogLevel minLevel = LogLevel.Information)
        {
            builder.AddProvider(new FileLoggerProvider(logDirectory, minLevel));
            return builder;
        }
    }
}
