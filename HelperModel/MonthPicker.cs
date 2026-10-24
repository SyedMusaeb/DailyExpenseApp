using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyExpense.HelperModel
{
    public static class MonthPicker
    {
        public static async Task<DateTime?> PickAsync(DateTime current, int monthsBack = 24)
        {
            var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var months = Enumerable.Range(0, monthsBack + 1)
                                   .Select(i => thisMonth.AddMonths(-i))
                                   .ToList();
            var labels = months
                .Select(m => m.ToString("MMMM yyyy") +
                             (m.Year == current.Year && m.Month == current.Month ? "  ✓" : ""))
                .ToArray();

            string choice = await Application.Current.MainPage
                .DisplayActionSheet("Select Month", "Cancel", null, labels);

            if (string.IsNullOrEmpty(choice) || choice == "Cancel")
                return null;

            int index = Array.IndexOf(labels, choice);
            return index >= 0 ? months[index] : null;
        }
    }
}
