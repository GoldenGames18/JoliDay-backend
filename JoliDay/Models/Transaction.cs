using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoliDay.Models
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }

        [Required(ErrorMessage = "Titre de transaction requis")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description requise")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Montant requis")]
        public float Amount { get; set; }

        public string? InvoiceURL { get; set; }
        public User Owner { get; set; }
    }
}