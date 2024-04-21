using JoliDay.Models;
using JoliDay.ViewModel;

namespace JoliDay.Services
{
    public interface IServiceEmail
    {

        bool SendEmail(ContactMeViewModel contactMe); 
    }
}
