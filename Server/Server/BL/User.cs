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
            return DBServiceUser.ReadUsers();
        }

        public bool Register()
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

        public static bool AddContinentPreference(int userId, string preference)
        {
            return DBServiceUser.AddContinentPreference(userId, preference);
        }

        public static bool RemoveContinentPreference(int userId, string preference)
        {
            return DBServiceUser.RemoveContinentPreference(userId, preference);
        }

        public static bool AddLanguageToUser(int userId, string language, string lanLevel)
        {
            if (!Enum.TryParse<LanguageLevel>(lanLevel, true, out var level))
            {
                return false;
            }
            return DBServiceUser.AddLanguageToUser(userId, language, level.ToString());
        }
        public static bool RemoveLanguageFromUser(int userId, string language)
        {
            return DBServiceUser.RemoveLanguageFromUser(userId, language);
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
