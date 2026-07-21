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

        public Share() { }
        public Share(int id, string description, string title, ShareType type, int countryId, int userId)
        {
            Id = id;
            Description = description;
            Title = title;
            Type = type;
            CountryId = countryId;
            UserId = userId;
        }

        public int UserId { get => userId; set => userId = value; }
        public int Id { get => id; set => id = value; }
        public string Description { get => description; set => description = value; }
        public string Title { get => title; set => title = value; }
        public ShareType Type { get => type; set => type = value; }
        public int CountryId { get => countryId; set => countryId = value; }
        public DateTime CreatedAt { get => createdAt; set => createdAt = value; }

        public static List<Share> GetAllShares()
        {
            return DBServiceShare.GetAllShares();
        }

        public static List<Share> GetUserShares(int userId)
        {
            return DBServiceShare.GetUserShares(userId);
        }
        public static List<Share> GetCountryShares(string countryName)
        {
            return DBServiceShare.GetCountryShares(countryName);
        }

        public static bool CreateShare(Share share)
        {
            return DBServiceShare.CreateShare(share);
        }

        public static bool UpdateShare(Share share)
        {
            return DBServiceShare.UpdateShare(share);
        }

        public static bool DeleteShare(int shareID,int userID)
        {
            return DBServiceShare.DeleteShare(shareID,userID);
        }
    }
}
