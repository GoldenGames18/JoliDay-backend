using System.ComponentModel.DataAnnotations;

namespace JoliDay.ViewModel
{
    public class GoogleTokenViewModel
    {
        [Required(ErrorMessage ="Token requis")]
        public string Token { get; set; }
    }
}
