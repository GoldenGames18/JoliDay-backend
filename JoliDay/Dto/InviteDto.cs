using System.ComponentModel.DataAnnotations;

namespace JoliDay.Dto;

public class InviteDto
{
    public string Id { get; set; }
    public bool IsRead { get; set; } = false;
    public string Title { get; set; }
}