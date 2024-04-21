using System.ComponentModel.DataAnnotations;

namespace JoliDay.Dto;

public class AddressDto
{
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string StreetName { get; set; }
    public string StreetNumber { get; set; }
}