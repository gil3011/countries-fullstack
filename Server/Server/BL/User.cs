using Server.DAL;
using System.ComponentModel;
using System.Net;
using System.Text.Json.Serialization;

namespace Server.BL
{
    public enum LanguageLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }

    public class User
    {
        private int id;
        private string username;
        private string password;
        private string email;

        private bool isBlocked;
        private bool isAdmin;
        private bool isAllowedToShare;

        public User() {

            isBlocked = false;
            isAdmin = false;
            isAllowedToShare = true;
        }

        public User(int id, string username, string password, string email, bool isBlocked, bool isAdmin, bool isAllowedToShare)
        {
            Id = id;
            Username = username;
            Password = password;
            Email = email;
            IsBlocked = isBlocked;
            IsAdmin = isAdmin;
            IsAllowedToShare = isAllowedToShare;
        }
        public int Id { get => id; set => id = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Email { get => email; set => email = value; }

        [DefaultValue(false)]
        public bool IsBlocked { get => isBlocked; set => isBlocked = value; }

        [DefaultValue(false)]
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }

        [DefaultValue(true)]
        public bool IsAllowedToShare { get => isAllowedToShare; set => isAllowedToShare = value; }

        // --- BLL Methods 

        public static List<User> Read()
        {
            DBServiceUser dbs = new();
            return dbs.ReadUsers();
        }

        public bool Register()
        {
            DBServiceUser dbs = new();
            return dbs.Register(this);
        }

        public bool UpdateUser()
        {
            DBServiceUser dbs = new();
            return dbs.UpdateUser(this);
        }

        public static bool DeleteUser(int id)
        {
            DBServiceUser dbs = new();
            return dbs.DeleteUser(id);

        }
        public static User GetUserByEmail(string email)
        {
            DBServiceUser dbs = new();
            return dbs.GetUserByEmail(email);
        }
        public static void AddLoginLog(int userId)
        {
            DBServiceUser dbs = new();
            dbs.AddLoginLog(userId);
        }

        // --- User Countries

        public static int addCountryToWishlist(int userId, int countryId)
        {
            DBServiceUser dbs = new();
            return dbs.addCountryToWishlist(userId, countryId);
        }

        public static int removeCountryFromWishlist(int userId, int countryId)
        {
            DBServiceUser dbs = new();
            return dbs.removeCountryFromWishlist(userId, countryId);
        }

        public static List<Country> getWishlist(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.getWishlist(userId);
        }

        public static int addCountryToVisited(int userId, int countryId)
        {
            DBServiceUser dbs = new();
            return dbs.addCountryToVisited(userId, countryId);
        }

        public static int removeCountryFromVisited(int userId, int countryId)
        {
            DBServiceUser dbs = new();
            return dbs.removeCountryFromVisited(userId, countryId);
        }

        public static List<Country> getVisited(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.getVisited(userId);
        }

        // --- Admin Methods

        public static bool BlockUser(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.BlockUser(userId);
        }

        public static bool UnblockUser(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.unblockUser(userId);
        }

        public static bool PreventSharing(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.preventSharing(userId);
        }

        public static bool AllowSharing(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.allowSharing(userId);
        }

        public static Dictionary<string, int> GetAdminStats()
        {
            DBServiceUser dbs = new();
            return dbs.GetAdminStats();
        }

        public static Dictionary<DateTime, int> GetDailyLoginCounts()
        {
            DBServiceUser dbs = new();
            return dbs.GetDailyLoginCounts();
        }

        public static bool AddContinentPreference(int userId, string preference)
        {
            DBServiceUser dbs = new();
            return dbs.AddContinentPreference(userId, preference);
        }

        public static bool RemoveContinentPreference(int userId, string preference)
        {
            DBServiceUser dbs = new();
            return dbs.RemoveContinentPreference(userId, preference);
        }

        public static bool AddLanguageToUser(int userId, string language, string lanLevel)
        {
            DBServiceUser dbs = new();
            if (!Enum.TryParse<LanguageLevel>(lanLevel, true, out var level))
            {
                return false;
            }
            return dbs.AddLanguageToUser(userId, language, level.ToString());
        }
        public static bool RemoveLanguageFromUser(int userId, string language)
        {
            DBServiceUser dbs = new();
            return dbs.RemoveLanguageFromUser(userId, language);
        }

        public static List<string> GetContinentPrefernces(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.GetContinentPrefernces(userId);
        }

        public static Dictionary<string, string> GetUserLanguages(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.GetUserLanguages(userId);
        }
    }
}
