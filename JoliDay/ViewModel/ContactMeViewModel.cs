using System.ComponentModel.DataAnnotations;

namespace JoliDay.ViewModel
{
    public class ContactMeViewModel
    {
        [Required(ErrorMessage ="Email requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Sujet requis")]
        public string Subject { get; set; }
        
        [Required(ErrorMessage = "Texte requis")]
        public string Body { get; set; }    

    }
}
