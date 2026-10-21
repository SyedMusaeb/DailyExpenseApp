using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DailyExpense.Models
{
    public class NewRegistration
    {
        [JsonIgnore]
        public string UserI { get; set; }
        public string FullName  { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Image { get; set; }
        public string Details { get; set; }
        public string Status { get; set; }
        public string UserType { get; set; }


    }
}
