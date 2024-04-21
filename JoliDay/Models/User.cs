using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using static System.Net.WebRequestMethods;

namespace JoliDay.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Nom requis")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Prénom requis")]
        public string FirstName { get; set; }

        public string PathPicture { get; set; } = "https://panoramix.cg.helmo.be/~e200072/picture/user.png";
    }
}