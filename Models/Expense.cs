using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyExpense.Models
{
    internal class Expense
    {
        [Newtonsoft.Json.JsonIgnore]
        public string Id { get; set; }
        public string UserId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string SamanName { get; set; }
        public string SamanWeight { get; set; }
        public string Unit { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; }
        public string Notes { get; set; }
        public DateTime Date { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedOn { get; set; }
        public string DeletedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
