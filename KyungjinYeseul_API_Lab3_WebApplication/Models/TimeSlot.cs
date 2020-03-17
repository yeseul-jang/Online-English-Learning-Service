using System;
using System.Collections.Generic;

namespace KyungjinYeseul_API_Lab3_WebApplication.Models
{
    public partial class TimeSlot
    {
        public int TimeSlotID { get; set; }
        public int UserID { get; set; }
        public string StrTimeSlot { get; set; }

        public User User { get; set; }

        public bool isBooked(string teacherEmail, string date_time)
        {
            Appointment_DBOperation dynamoDB = new Appointment_DBOperation("Appointment");

            string date_timeslot = date_time + "_" + StrTimeSlot;

            return dynamoDB.isBookedAsync(teacherEmail, date_timeslot).Result;            
        }
    }
}
