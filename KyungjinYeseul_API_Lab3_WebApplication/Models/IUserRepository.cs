using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyungjinYeseul_API_Lab3_WebApplication.Models
{
    public interface IUserRepository
    {
        IEnumerable<User> Users { get; }
        IEnumerable<TimeSlot> userTimeSlots(int userID);
    }
}
