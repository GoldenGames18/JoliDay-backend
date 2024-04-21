using System.ComponentModel.DataAnnotations;
using JoliDay.Models;

namespace JoliDay.ViewModel;

public class InviteViewModel
{
    [Required(ErrorMessage = "Vacances requises")]
    public string HolidayId { get; set; }
    [Required(ErrorMessage = "Invité requis")]
    public string Email { get; set; }
}