using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyExpense.HelperModel
{
    internal class CategorySummary
    {
        public string CategoryName { get; set; }
        public string CategoryIcon { get; set; }
        public string MonthYear { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
