using System;
using System.Collections.Generic;

namespace KyungjinYeseul_API_Lab3_WebApplication.Models
{
    public partial class User
    {
        public User()
        {
            TimeSlots = new HashSet<TimeSlot>();
        }

        public int UserID { get; set; }
        public string EmailID { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<TimeSlot> TimeSlots { get; set; }
    }
}
