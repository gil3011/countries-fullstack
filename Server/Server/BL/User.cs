using Server.DAL;
using System.ComponentModel;
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
            return DBServiceUser.ReadUsers();
        }

        public int Register()
        {
            return DBServiceUser.Register(this);
        }

        public bool UpdateUser()
        {
            return DBServiceUser.UpdateUser(this);
        }

        public static bool DeleteUser(int id)
        {
            return DBServiceUser.DeleteUser(id);

        }
        public static User GetUserByEmail(string email)
        {
            return DBServiceUser.GetUserByEmail(email);
        }

        // --- User Countries

        public static int addCountryToWishlist(int userId, int countryId)
        {
            return DBServiceUser.addCountryToWishlist(userId, countryId);
        }

        public static int removeCountryFromWishlist(int userId, int countryId)
        {
            return DBServiceUser.removeCountryFromWishlist(userId, countryId);
        }

        public static List<Country> getWishlist(int userId)
        {
            return DBServiceUser.getWishlist(userId);
        }

        public static int addCountryToVisited(int userId, int countryId)
        {
            return DBServiceUser.addCountryToVisited(userId, countryId);
        }

        public static int removeCountryFromVisited(int userId, int countryId)
        {
            return DBServiceUser.removeCountryFromVisited(userId, countryId);
        }

        public static List<Country> getVisited(int userId)
        {
            return DBServiceUser.getVisited(userId);
        }

        // --- Admin Methods

        public static bool BlockUser(int userId)
        {
            return DBServiceUser.BlockUser(userId);
        }

        public static bool UnblockUser(int userId)
        {
            return DBServiceUser.unblockUser(userId);
        }

        public static bool PreventSharing(int userId)
        {
            return DBServiceUser.preventSharing(userId);
        }

        public static bool AllowSharing(int userId)
        {
            return DBServiceUser.allowSharing(userId);
        }

        public static Dictionary<string, int> GetAdminStats()
        {
            return DBServiceUser.GetAdminStats();
        }

        public static List<string> GetContinentPrefernces(int userId)
        {
            return DBServiceUser.GetContinentPrefernces(userId);
        }

        public static Dictionary<string, string> GetUserLanguages(int userId)
        {
            return DBServiceUser.GetUserLanguages(userId);
        }


    }
}
