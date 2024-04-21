using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoliDay.Models
{
    public class Address
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "Pays requis")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Code postal requis")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Ville requise")]
        public string City { get; set; }

        [Required(ErrorMessage = "Nom de rue requis")]
        public string StreetName { get; set; }

        [Required(ErrorMessage = "Numéro de boîte requis")]
        public string StreetNumber { get; set; }
    }
}