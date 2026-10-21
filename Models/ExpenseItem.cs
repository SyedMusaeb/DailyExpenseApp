using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyExpense.Models
{
    internal class ExpenseItem
    {
        [Newtonsoft.Json.JsonIgnore]
        public string Id { get; set; }
        public string SamanName { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string WeightDisplay { get; set; }
    }
}
