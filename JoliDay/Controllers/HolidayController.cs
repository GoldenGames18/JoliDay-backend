using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AutoMapper;
using JoliDay.Dto;
using JoliDay.Models;
using JoliDay.Services;
using JoliDay.Utils;
using JoliDay.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace JoliDay.Controllers;

//Created the HolidayController and added an admin route - GetAll returns a list of all holidays as dtos.
// Added a user route to Create a Holiday from a ViewModel.
[ApiController]
[Route("[controller]")]
[Authorize]
public class HolidayController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly JoliDayContext _context;
    private readonly IServiceValidator _validator;

    public HolidayController(IMapper mapper, JoliDayContext context, IServiceValidator serviceValidator)
    {
        this._mapper = mapper;
        this._context = context;
        this._validator = serviceValidator;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Récupération de toutes les holiday")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(404, "Utilisateur introuvable")]
    [SwaggerResponse(200, "", typeof(List<HolidayDto>))]
    public async Task<IActionResult> AllHolidays()
    {
        try
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holidays = await _context.Holidays
                .OrderBy(h => h.StartDate)
                .Include(h => h.Address)
                .Include(h => h.Owner)
                .Include(h => h.Users)
                .Include(h => h.Activities!).ThenInclude(h => h.Address)
                .Where(h => h.Owner.Id == user.Id || h.Users!.Contains(user))
                .ToListAsync();
            var holidaysDto = _mapper.Map<List<HolidayDto>>(holidays);
            return Ok(holidaysDto);
        }
        catch (Exception)
        {
            return StatusCode(500, "Une erreur interne s'est produite. Veuillez contacter l'administrateur.");
        }
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Création d'une holiday")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(532, "Erreur lors de sauvegarde des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(404, "Utilisateur introuvable")]
    [SwaggerResponse(200, "", typeof(HolidayDto))]
    public async Task<IActionResult> Create([Required] [FromBody] CreateHolidayViewModel createHolidayViewModel)
    {
        try
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = _mapper.Map<CreateHolidayViewModel, Holiday>(createHolidayViewModel);
            holiday.Owner = user;
            await _context.Holidays.AddAsync(holiday);
            await _context.SaveChangesAsync();
            return new OkObjectResult(_mapper.Map<Holiday, HolidayDto>(holiday));
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }
        catch (Exception ex) when (ex is NotSupportedException || ex is DbUpdateException || ex is DbUpdateConcurrencyException)
        {
            return StatusCode(532, "Une erreur lors de la sauvegarde des données. Veuillez contacter l'administrateur.");
        }
    }

    [HttpPut]
    [Route("{idHoliday}")]
    [SwaggerOperation(Summary = "Modification d'une invitation")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(533, "Erreur lors de la modification des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(404, "Vacances introuvable")]
    [SwaggerResponse(200, "")]
    public async Task<IActionResult> Edit([Required][FromBody] EditHolidayViewModel editHolidayViewModel, [Required][FromRoute] string idHoliday)
    {
        try
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = await _context.Holidays.Include(h => h.Address)
                .Include(h => h.Owner)
                .FirstOrDefaultAsync(h => h.Id == idHoliday);


            if (holiday == null)
            {
                return NotFound("Vacances introuvable");
               
            }


            if (!_validator.IsOwner(user, holiday))
            {
                return Forbid();
            }

            if (editHolidayViewModel.StartDate < holiday.StartDate && editHolidayViewModel.StartDate < DateTime.Today)
            {
                return new BadRequestObjectResult(new Error()
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vous ne pouvez pas déplacer une vacances avant sa date de début si celle-ci est déjà passée."
                });
            }

            holiday.Name = editHolidayViewModel.Name;
            holiday.StartDate = editHolidayViewModel.StartDate;
            holiday.EndDate = editHolidayViewModel.EndDate;
            holiday.Address.StreetName = editHolidayViewModel.Address.StreetName;
            holiday.Address.City = editHolidayViewModel.Address.City;
            holiday.Address.PostalCode = editHolidayViewModel.Address.PostalCode;
            holiday.Address.Country = editHolidayViewModel.Address.Country;
            holiday.Address.StreetNumber = editHolidayViewModel.Address.StreetNumber;


            _context.Holidays.Update(holiday);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }
        catch (Exception ex) when (ex is NotSupportedException || ex is DbUpdateException || ex is DbUpdateConcurrencyException)
        {
            return StatusCode(533, "Une erreur lors de la modification des données. Veuillez contacter l'administrateur.");
        }
    }

    [HttpDelete]
    [Route("{id}")]
    [SwaggerOperation(Summary = "Suppression d'une holiday")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(534, "Erreur lors de la suppression des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(404, "Vacances introuvable")]
    [SwaggerResponse(200, "")]
    public async Task<IActionResult> Delete([Required][FromRoute]string id)
    {
        try
        {

            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = await _context.Holidays.Include(holiday => holiday.Owner).Include(h => h.Messages).Include(holiday => holiday.Activities).FirstOrDefaultAsync(h => h.Id == id);
            if (holiday == null)
            {
                return NotFound("Vacances indisponibles");
            }

            if (!_validator.IsOwner(user, holiday))
            {
                return Forbid();
            }


            if (holiday.Activities != null)
            {
                _context.Activitys.RemoveRange(holiday.Activities);
            }

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }
        catch (Exception ex) when (ex is NotSupportedException || ex is DbUpdateException || ex is DbUpdateConcurrencyException)
        {
            return StatusCode(534, "Une erreur lors de la suppresion des données. Veuillez contacter l'administrateur.");
        }
    }
    
    [HttpDelete]
    [Route("{idHoliday}/User/{idUser}")]
    [SwaggerOperation(Summary = "Suppression d'un utilisateur d'une holiday")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(534, "Erreur lors de la suppression des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(404, "Vacances introuvable")]
    [SwaggerResponse(200, "")]
    public async Task<IActionResult> DeleteUser([Required] [FromRoute] string idHoliday,
        [Required] [FromRoute] string idUser)
    {
        try
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = await _context.Holidays.Include(holiday => holiday.Owner)
                .Include(holiday => holiday.Users).FirstOrDefaultAsync(h => h.Id == idHoliday);
            if (holiday == null)
            {
                return NotFound("Vacances introuvable");
            }

            if (!_validator.IsOwner(user, holiday))
            {
                return Forbid();
            }

            if (holiday.Users == null)
            {
                return new BadRequestObjectResult(new Error()
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Utilisateur introuvable"
                });
            }

            var userToDelete = holiday.Users.FirstOrDefault(u => u.Id == idUser);
            if (userToDelete == null)
            {
                return new BadRequestObjectResult(new Error()
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Utilisateur introuvable"
                });
            }

            holiday.Users.Remove(userToDelete);
            _context.Holidays.Update(holiday);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531,"Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }
        catch (Exception ex) when (ex is NotSupportedException || ex is DbUpdateException || ex is DbUpdateConcurrencyException)
        {
            return StatusCode(534,"Une erreur lors de la suppresion des données. Veuillez contacter l'administrateur.");
        }
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("Statistic/{date:datetime}")]
    [SwaggerOperation(Summary = "Statistique Holiday")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(200, "", typeof(List<GoneVacationerDto>))]
    public async Task<IActionResult> GonePeople([Required] [FromRoute] DateTime date)

    {
        try
        {
            var holidays = await _context.Holidays
                .Include(h => h.Users)
                .Include(h => h.Owner)
                .Include(h => h.Address)
                .Where(h => h.StartDate <= date && h.EndDate >= date)
                .ToListAsync();
            IDictionary<string, int> usersPerCountry = new Dictionary<string, int>();
            ISet<User> usersChecked = new HashSet<User>();
            holidays.ForEach(h =>
            {
                var country = h.Address.Country;
                if (!usersPerCountry.ContainsKey(country))
                {
                    usersPerCountry[country] = 0;
                }

                if (!usersChecked.Contains(h.Owner))
                {
                    usersChecked.Add(h.Owner);
                    usersPerCountry[country]++;
                }

                if (h.Users == null) return;
                foreach (var hUser in h.Users)
                {
                    if (usersChecked.Contains(hUser)) continue;
                    usersChecked.Add(hUser);
                    usersPerCountry[country]++;
                }
            });
            IList<GoneVacationerDto> dtoList = usersPerCountry.OrderBy(p => p.Key)
                .Select(pair => new GoneVacationerDto { Country = pair.Key, Users = pair.Value }).ToList();

            return Ok(dtoList);
        }
        catch (Exception ex) when (ex is ArgumentNullException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }
    }
}