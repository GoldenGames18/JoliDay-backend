using System.ComponentModel.DataAnnotations;

namespace JoliDay.Dto
{
    public class MessageDto
    {
        public string Id { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Content { get; set; }
        public UserDto Owner { get; set; }
    }
}
