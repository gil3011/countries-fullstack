using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Server.BL;
using Server.DTO;
using System.Globalization;

namespace Server.Conntroller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private const string LogDirectory = "Logs";
        private static string LogDirectoryPath => Path.Combine(Directory.GetCurrentDirectory(), LogDirectory);

        // Admins actions

        [HttpPut("blockUser/{id}")]
        public IActionResult blockUser(int id)
        {
            try
            {
                bool result = Admin.BlockUser(id);
                if (!result)
                    return BadRequest("User block failed");
                return Ok(new { message = "User blocked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpPut("unblockUser/{id}")]
        public IActionResult unblockUser(int id)
        {
            try
            {
                bool result = Admin.UnblockUser(id);
                if (!result)
                    return BadRequest("User unblock failed");
                return Ok(new { message = "User unblocked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpPut("preventSharing/{id}")]
        public IActionResult preventSharing(int id)
        {
            try
            {
                bool result = Admin.PreventSharing(id);
                if (!result)
                    return BadRequest("User prevent sharing failed");
                return Ok(new { message = "User prevent sharing successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpPut("allowSharing/{id}")]
        public IActionResult allowSharing(int id)
        {
            try
            {
                bool result = Admin.AllowSharing(id);
                if (!result)
                    return BadRequest("User allow sharing failed");
                return Ok(new { message = "User allow sharing successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            try
            {
                var stats = Admin.GetAdminStats();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving wishlist.");
            }
        }

        [HttpGet("GetDailyLoginCounts")]
        public IActionResult GetDailyLoginCounts()
        {
            try
            {
                var loginCounts = Admin.GetDailyLoginCounts();
                return Ok(loginCounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("promote/{userId}")]
        public IActionResult PromoteToAdmin(int userId)
        {
            try
            {
                bool success = Admin.PromoteToAdmin(userId);

                if (!success)
                {
                    return NotFound(new
                    {
                        message = "User was not found or could not be promoted."
                    });
                }

                return Ok(new
                {
                    message = "User was promoted to admin successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while promoting the user.",
                    error = ex.Message
                });
            }
        }

        [HttpPut("demote/{userId}")]
        public IActionResult DemoteFromAdmin(int userId)
        {
            try
            {
                bool success = Admin.DemoteFromAdmin(userId);

                if (!success)
                {
                    return NotFound(new
                    {
                        message = "User was not found or could not be demoted."
                    });
                }

                return Ok(new
                {
                    message = "User was demoted from admin successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while demoting the user.",
                    error = ex.Message
                });
            }
        }
        
        // Log's controls

        [HttpGet("log/dates")]
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

        [HttpGet("log/{date}")]
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
