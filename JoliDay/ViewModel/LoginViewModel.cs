using System.ComponentModel.DataAnnotations;

namespace JoliDay.ViewModel
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mots de passe requis")]
        public string Password { get; set; }
    }
}