using Server.DAL;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Server.BL
{
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
    }
}
