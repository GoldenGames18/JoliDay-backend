using System.ComponentModel.DataAnnotations;

namespace JoliDay.ViewModel;

public class AddressViewModel
{
    [Required(ErrorMessage = "Le pays est requis")]
    public string Country { get; set; }

    [Required(ErrorMessage = "Le code postal est requis")]
    [MaxLength(length: 15, ErrorMessage = "Le code postal ne doit pas dépasser 15 caractères")]
    public string PostalCode { get; set; }

    [Required(ErrorMessage = "La ville est requise")]
    public string City { get; set; }

    [Required(ErrorMessage = "Le nom de la rue est requis")]
    public string StreetName { get; set; }

    [Required(ErrorMessage = "Le numéro de rue est requis")]
    public string StreetNumber { get; set; }
}