using Server.DAL;

namespace Server.BL
{
    public enum ShareType
    {
        Recommendation,
        Thought,
        Review
    }
    public class Share
    {
        private int id;
        private string description;
        private string title;
        private ShareType type;
        private int countryId;
        private DateTime createdAt;
        private int userId;
        private string userName;
        private string countryName;
        private string cca3;

        public Share() { }
        public Share(int id, string description, string title, ShareType type, int countryId, int userId, string userName, string countryName, string cca3)
        {
            Id = id;
            Description = description;
            Title = title;
            Type = type;
            CountryId = countryId;
            UserId = userId;
            UserName = userName;
            CountryName = countryName;
            Cca3 = cca3;
        }

        public int UserId { get => userId; set => userId = value; }
        public int Id { get => id; set => id = value; }
        public string Description { get => description; set => description = value; }
        public string Title { get => title; set => title = value; }
        public ShareType Type { get => type; set => type = value; }
        public int CountryId { get => countryId; set => countryId = value; }
        public DateTime CreatedAt { get => createdAt; set => createdAt = value; }
        public string CountryName { get => countryName; set => countryName = value; }
        public string UserName { get => userName; set => userName = value; }

        public string Cca3 { get => cca3; set => cca3 = value; }
        public static List<Share> GetAllShares()
        {
            DBServiceShare dbs = new();
            return dbs.GetAllShares();
        }

        public static List<Share> GetUserShares(int userId)
        {
            DBServiceShare dbs = new();
            return dbs.GetUserShares(userId);
        }
        public static List<Share> GetCountryShares(string countryName)
        {
            DBServiceShare dbs = new();
            return dbs.GetCountryShares(countryName);
        }

        public static bool CreateShare(Share share)
        {
            DBServiceShare dbs = new();
            return dbs.CreateShare(share);
        }

        public static bool UpdateShare(Share share)
        {
            DBServiceShare dbs = new();
            return dbs.UpdateShare(share);
        }

        public static bool DeleteShare(int shareID,int userID)
        {
            DBServiceShare dbs = new();
            return dbs.DeleteShare(shareID,userID);
        }
    }
}
