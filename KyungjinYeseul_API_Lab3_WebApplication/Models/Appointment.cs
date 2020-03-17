using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyungjinYeseul_API_Lab3_WebApplication.Models
{
    public class Appointment
    {
        public string TeacherEmailID { get; set; }
        public string AppointmentDate_TimeSlot { get; set; }
        public string StudentEmailID { get; set; }
        public string StudentFullName { get; set; }
        public string TeacherFullName { get; set; }
        public string Course { get; set; }
        public string EmailMessage { get; set; }
        public string StudentComment { get; set; }
        public string TeacherComment { get; set; }
        public string Status { get; set; }
    }
}
