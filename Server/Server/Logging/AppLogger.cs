using System.Runtime.CompilerServices;

namespace Server.Logging
{
    /// <summary>
    /// Static logging entry point for layers that are not created through dependency
    /// injection (the BL and DAL classes are instantiated with plain <c>new()</c>).
    /// Initialized once at startup from <see cref="ILoggerFactory"/> so it writes to the
    /// same console + rolling-file pipeline as the rest of the app.
    /// </summary>
    public static class AppLogger
    {
        private static ILogger _logger;

        /// <summary>
        /// Wires the static logger to the application's logging pipeline.
        /// Call once during startup after the host is built.
        /// </summary>
        public static void Init(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Server.DAL");
        }

        /// <summary>
        /// Logs an exception together with the source file and method it was caught in.
        /// The caller information is filled in automatically by the compiler, so existing
        /// catch blocks only need to pass the exception.
        /// </summary>
        public static void LogException(Exception ex,
            [CallerMemberName] string member = "",
            [CallerFilePath] string filePath = "")
        {
            string source = $"{Path.GetFileNameWithoutExtension(filePath)}.{member}";

            if (_logger != null)
                _logger.LogError(ex, "Exception in {Source}", source);
            else
                // Fallback if the pipeline was never initialized (e.g. unit tests).
                Console.Error.WriteLine($"[ERROR] Exception in {source}: {ex}");
        }
    }
}
