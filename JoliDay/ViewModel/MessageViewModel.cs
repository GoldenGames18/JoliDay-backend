using System.ComponentModel.DataAnnotations;

namespace JoliDay.ViewModel
{
    public class MessageViewModel
    {
        [Required(ErrorMessage ="Message requis")]
        public string Content { get; set; } 
    }
}
