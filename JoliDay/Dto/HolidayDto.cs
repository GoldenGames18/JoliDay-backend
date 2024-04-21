namespace JoliDay.Dto;

public class HolidayDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AddressDto Address { get; set; }
    public UserDto Owner { get; set; }
    public IList<UserDto> Users { get; set; }
    public IList<ActivityDto> Activities { get; set; }
}