using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyExpense.Models
{
    internal class Category
    {
        [Newtonsoft.Json.JsonIgnore]
        public int Id { get; set; }

        public string CategoryName { get; set; }

        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
