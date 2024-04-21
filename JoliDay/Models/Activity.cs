using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoliDay.Models
{
    public class Activity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "Nom requis")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description requise")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Date de debut requis")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Date de fin requis")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Adresse requise")]
        public Address Address { get; set; }
    }
}