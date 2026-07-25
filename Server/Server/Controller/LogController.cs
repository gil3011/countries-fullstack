using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Server.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private const string LogDirectory = "Logs";

        private static string LogDirectoryPath =>
            Path.Combine(Directory.GetCurrentDirectory(), LogDirectory);

        // GET: api/Log/dates
        [HttpGet("dates")]
        public IActionResult GetAvailableDates()
        {
            try
            {
                if (!Directory.Exists(LogDirectoryPath))
                    return Ok(new List<string>());

                var dates = new List<string>();
                foreach (string file in Directory.GetFiles(LogDirectoryPath, "log-*.log"))
                {
                    string name = Path.GetFileNameWithoutExtension(file); // log-yyyyMMdd
                    string datePart = name.Length >= 8 ? name[^8..] : string.Empty;

                    if (DateTime.TryParseExact(datePart, "yyyyMMdd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime date))
                    {
                        dates.Add(date.ToString("yyyy-MM-dd"));
                    }
                }

                dates.Sort(StringComparer.Ordinal);
                dates.Reverse(); 
                return Ok(dates);
            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while listing log dates.");
            }
        }

        // GET: api/Log/2026-07-24
        // Returns the log lines for a single day.
        [HttpGet("{date}")]
        public IActionResult GetLogByDate(string date)
        {
            try
            {
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime parsedDate))
                {
                    return BadRequest("Date must be in yyyy-MM-dd format.");
                }

                string fileName = $"log-{parsedDate:yyyyMMdd}.log";
                string path = Path.Combine(LogDirectoryPath, fileName);

                if (!System.IO.File.Exists(path))
                    return Ok(new { date, found = false, lineCount = 0, lines = Array.Empty<string>() });

                // Share ReadWrite so reading never conflicts with the logger appending.
                string content;
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    content = sr.ReadToEnd();
                }

                string[] lines = content.Split('\n')
                    .Select(l => l.TrimEnd('\r'))
                    .Where(l => l.Length > 0)
                    .ToArray();

                return Ok(new { date, found = true, lineCount = lines.Length, lines });
            }
            catch (Exception ex)
            {
                Server.Logging.AppLogger.LogException(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while reading the log file.");
            }
        }
    }
}
