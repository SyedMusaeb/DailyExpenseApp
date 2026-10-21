using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyExpense.Models
{
    internal class Session
    {
        public static bool IsAdmin => Preferences.Get("Role", "User") == "Admin";
        public static string UserId => Preferences.Get("UserId", "");
    }   
}
