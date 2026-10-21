using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyExpense.Views.Admin.Model
{
    internal class AdminExpenseItem
    {
        [Newtonsoft.Json.JsonIgnore]
        public string Id { get; set; }
        public string SamanName { get; set; }
        public string CategoryName { get; set; }
        public string UserName { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string WeightDisplay { get; set; }
        public bool IsDeleted { get; set; }
        public string StatusText { get; set; }     
        public string CardColor { get; set; }     
    }
}
