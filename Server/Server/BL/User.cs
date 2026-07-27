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

        private bool isBlocked = false;
        private bool isAdmin = false;
        private bool isAllowedToShare = true;

        public int Id { get => id; set => id = value; }
        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Email { get => email; set => email = value; }

        public bool IsBlocked { get => isBlocked; set => isBlocked = value; }
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }
        public bool IsAllowedToShare { get => isAllowedToShare; set => isAllowedToShare = value; }
        public List<string> PreferdContinents { get; set; } = new List<string>();
        public Dictionary<string, LanguageLevel> LanguegeLevels { get; set; } = new Dictionary<string, LanguageLevel>();

        // --- BLL Methods 

        public static List<User> Read()
        {
            DBServiceUser dbs = new();
            return dbs.ReadUsers();
        }

        public int Register()
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
        public static User GetUserById(int id)
        {
            DBServiceUser dbs = new();
            return dbs.GetUserById(id);
        }
        public static bool UpdatePassword(int userId, string hashedPassword)
        {
            DBServiceUser dbs = new();
            return dbs.UpdatePassword(userId, hashedPassword);
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

        public static bool moveToVisited(int userId, int countryId)
        {
            DBServiceUser dbs = new();
            // First try to remove from wishlist, we don't strictly care if it fails (it might not be there)
            dbs.removeCountryFromWishlist(userId, countryId);
            // Then add to visited
            return dbs.addCountryToVisited(userId, countryId) > 0;
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

        
        // --- User Methods

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
