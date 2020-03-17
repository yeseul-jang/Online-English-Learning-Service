using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyungjinYeseul_API_Lab3_WebApplication.Models
{
    public class EFUserRepository : IUserRepository
    {
        private API_Lab3Context context;


        public EFUserRepository(API_Lab3Context ctx)
        {
            context = ctx;
        }

        public IEnumerable<User> Users => context.User.Include(u => u.TimeSlots);

        public IEnumerable<TimeSlot> userTimeSlots(int userID)
        {
            return context.TimeSlot.Where(t => t.UserID == userID);
        }
    }
}
