using Microsoft.AspNetCore.Mvc;
using Server.DAL;
using System.Globalization;

namespace Server.BL
{
    public class Admin :User
    {
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

        public static bool PromoteToAdmin(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.promoteToAdmin(userId);
        }

        public static bool DemoteFromAdmin(int userId)
        {
            DBServiceUser dbs = new();
            return dbs.demoteFromAdmin(userId);
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

    }
}
