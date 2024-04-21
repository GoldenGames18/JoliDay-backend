using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoliDay.Models
{
    public class Holiday
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "Nom de séjour requis")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Date de debut requis")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Date de fin requis")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Adresse requise")]
        public Address Address { get; set; }

        public IList<Activity>? Activities { get; set; }

        public IList<Transaction>? Transactions { get; set; }

        public IList<Message>? Messages { get; set; }

        public User Owner { get; set; }
        public IList<User>? Users { get; set; }
    }
}