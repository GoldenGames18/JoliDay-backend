using JoliDay.Models;
using Microsoft.EntityFrameworkCore;

namespace JoliDay.Services
{
    public class ServiceValidator : IServiceValidator
    {

        private readonly JoliDayContext _context;
        public ServiceValidator(JoliDayContext context) 
        { 
            _context = context;
        }

      

        public async Task<User?> GetCurrentUser(string claim)
        {
            if (claim == null)
                return null;
            else
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == claim);
           
        }
        public async Task<Holiday?> GetCurrentHoliday(string idHoliday)=> await _context.Holidays.Include(h => h.Activities)
            .Include(holiday => holiday.Owner)
            .Include(holiday => holiday.Users!)
            .Include(h => h.Activities!).ThenInclude(h => h.Address)
            .FirstOrDefaultAsync(h => h.Id == idHoliday);

        public bool IsMemberOf(User user, Holiday holiday) 
        {
            if (IsOwner(user, holiday))
            {
                return true;
            }
            else 
            {
                return IsMember(user, holiday);
            }
        }
        public bool IsOwner(User user, Holiday holiday) => holiday.Owner.Id == user.Id;

        public bool IsMember(User user, Holiday holiday) => holiday.Users.Contains(user);

    }
}
