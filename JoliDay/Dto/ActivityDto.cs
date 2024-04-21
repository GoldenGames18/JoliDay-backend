namespace JoliDay.Dto;

public class ActivityDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AddressDto Address { get; set; }
}