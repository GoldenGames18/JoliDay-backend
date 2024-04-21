using System.ComponentModel.DataAnnotations;

namespace JoliDay.ViewModel
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nom requis")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Prénom requis")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Email requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mot de passe requis")]
        public string Password { get; set; }
    }
}