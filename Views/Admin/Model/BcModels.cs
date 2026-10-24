using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DailyExpense.Views.Admin.Model
{
    internal class BcModels
    {
    }

    public class BcMember
    {
        [JsonIgnore] public string Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BcPayment
    {
        [JsonIgnore] public string Id { get; set; }
        public string MemberId { get; set; }
        public string Month { get; set; }        // "2026-10"
        public decimal Amount { get; set; }
        public string EnteredBy { get; set; }
        public DateTime EnteredOn { get; set; }
    }

    // screen ke liye helper classes
    public class BcRow
    {
        public string MemberId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string AmountText { get; set; }
        public string AmountColor { get; set; }
        public string RowColor { get; set; }
    }

    public class BcSummaryRow
    {
        public string Name { get; set; }
        public string TotalText { get; set; }
        public string PercentText { get; set; }
    }
}
