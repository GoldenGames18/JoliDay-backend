using System.ComponentModel.DataAnnotations;
using JoliDay.Dto;

namespace JoliDay.ViewModel;

public class CreateHolidayViewModel
{
    [Required(ErrorMessage = "Nom requis")]
    [StringLength(50, ErrorMessage = "Le nom doit contenir au moins 3 caractères", MinimumLength = 3)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Date de début requise")]
    [DataType(DataType.Date)]
    [CustomValidation(typeof(CreateHolidayViewModel), "ValidateDates")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Date de fin requise")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
    [Required(ErrorMessage = "Adresse requise")]
    public AddressViewModel Address { get; set; }

    public static ValidationResult? ValidateDates(DateTime startDate, ValidationContext context)
    {
        var holidayViewModel = (CreateHolidayViewModel)context.ObjectInstance;
        if (startDate.Date >= DateTime.Today) //si la date de début est sup ou égale à la date actuelle
            return startDate > holidayViewModel.EndDate //si la date de début est supérieure à la date de fin
                ? new ValidationResult("La date de début doit être inférieure à la date de fin")
                : ValidationResult.Success;
        return new ValidationResult(
            $"La date de début ({startDate.Date})  doit être supérieure à la date actuelle ({DateTime.Today})");

        // En fait c'est null mais c'est historiquement normal
        // https://stackoverflow.com/questions/13692312/what-is-the-purpose-of-validationresult-success-field
    }
}