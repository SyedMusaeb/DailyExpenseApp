using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DailyExpense.Models
{
    internal class BcMember
    {
        [JsonIgnore] public string Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }

        [JsonIgnore] public string StatusText => IsActive ? "" : "Inactive";
        [JsonIgnore] public string RowColor => IsActive ? "#FFFFFF" : "#F0F0F0";
    }
}
