using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JoliDay.Models
{
    public class Invite
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public bool IsRead { get; set; } = false;
        public Holiday Holiday { get; set; }
        public User User { get; set; }
    }
}