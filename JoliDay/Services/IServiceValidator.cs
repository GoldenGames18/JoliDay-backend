using JoliDay.Models;

namespace JoliDay.Services
{
    public interface IServiceValidator
    {

        Task<Holiday?> GetCurrentHoliday(string idHoliday);
        Task<User?> GetCurrentUser(string claim);
    
        bool IsOwner(User user, Holiday holiday);
        bool IsMember(User user, Holiday holiday);
        bool IsMemberOf(User user, Holiday holiday);

    }
}
