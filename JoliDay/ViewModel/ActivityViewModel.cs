using System.ComponentModel.DataAnnotations;

namespace JoliDay.ViewModel;

public class ActivityViewModel
{
    [Required(ErrorMessage = "Nom requis")]
    [StringLength(50, ErrorMessage = "Le nom doit contenir au moins 3 caractères", MinimumLength = 3)]
    public string Name { get; set; }

    [Required(ErrorMessage = "Description requise")]
    [StringLength(150, ErrorMessage = "La description doit contenir au moins 5 caractères", MinimumLength = 9)]
    public string Description { get; set; }

    [Required(ErrorMessage = "Date de debut requise")]
    [DataType(DataType.Date)]
    [CustomValidation(typeof(ActivityViewModel), "ValidateDates")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Date de fin requise")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Adresse requise")]
    public AddressViewModel Address { get; set; }

    public static ValidationResult? ValidateDates(DateTime startDate, ValidationContext context)
    {
        var activityViewModel = (ActivityViewModel)context.ObjectInstance;
        return startDate > activityViewModel.EndDate //si la date de début est supérieure à la date de fin
            ? new ValidationResult("La date de début doit être inférieure à la date de fin")
            : ValidationResult.Success;

        // En fait c'est null mais c'est historiquement normal
        // https://stackoverflow.com/questions/13692312/what-is-the-purpose-of-validationresult-success-field
    }
}