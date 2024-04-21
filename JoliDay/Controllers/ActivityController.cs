using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using JoliDay.Dto;
using JoliDay.Models;
using JoliDay.Services;
using JoliDay.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace JoliDay.Controllers;

[ApiController]
[Route("Holiday/{idHoliday}")]
[Authorize]
public class ActivityController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly JoliDayContext _context;
    private readonly IServiceValidator _validator;

    public ActivityController(IMapper mapper, JoliDayContext context, IServiceValidator validator)
    {
        this._mapper = mapper;
        this._context = context;
        this._validator = validator;
    }

    [HttpGet("ics")]
    [SwaggerOperation(Summary = "Génération du fichier ics")]
    [SwaggerResponse(530, "Erreur lors de la génération du fichier")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "", typeof(File))]

    public async Task<IActionResult> GenerateICS([Required][FromRoute] string idHoliday)
    {
        try 
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = _validator.GetCurrentHoliday(idHoliday).Result;
            if (holiday == null)
            {
                return NotFound("Vacance introuvable");
            }


            if (!_validator.IsMemberOf(user, holiday))
            {
                return Forbid();
            }

            var calendar = new Calendar();

            foreach (var item in holiday.Activities)
            {
                calendar.Events.Add(new CalendarEvent()
                {
                    Summary = item.Name,
                    Description = item.Description,
                    Start = new CalDateTime(item.StartDate),
                    End = new CalDateTime(item.EndDate),
                    Location = String.Format("{0} {1} {2} {3}", item.Address.StreetName, item.Address.StreetNumber, item.Address.PostalCode, item.Address.City)
                });
            }

            var serializer = new CalendarSerializer();
            string result = serializer.SerializeToString(calendar);
            return File(Encoding.ASCII.GetBytes(result), "text/calendar", "calendar.ics");

        }
        catch(Exception ex) when(ex is ArgumentNullException || ex is EncoderFallbackException || ex is FormatException || ex is NotSupportedException) 
        {
            return StatusCode(530, "Une erreur lors de la génération du fichier. Veuillez contacter l'administrateur.");
        }
        catch(Exception ex) when(ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }
        
        
    }


    [HttpGet("Activity")]
    [SwaggerOperation(Summary = "Récupération de toutes les activités d'une holiday")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "", typeof(List<ActivityDto>))]
    public async Task<IActionResult> AllActivities([Required][FromRoute] string idHoliday)
    {
        try 
        {

            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday =  _validator.GetCurrentHoliday(idHoliday).Result;
            if (holiday == null)
            {
                return NotFound("Vacance introuvable");
            }

            if (!_validator.IsMemberOf(user, holiday))
            {
                return Forbid();

            }

            var activitiesDto = _mapper.Map<List<ActivityDto>>(holiday.Activities ?? new List<Activity>());
            return Ok(activitiesDto);
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }

    }

    [HttpGet("Activity/{idActivity}")]
    [SwaggerOperation(Summary = "Trouver une activité par son identifiant")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "", typeof(ActivityDto))]
    public async Task<IActionResult> GetActivity([Required][FromRoute] string idHoliday, [Required][FromRoute] string idActivity)
    {
        try 
        {

            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = _validator.GetCurrentHoliday(idHoliday).Result;
            if (holiday == null)
            {
                return NotFound("Vacance introuvable");
            }

            if (!_validator.IsMemberOf(user, holiday))
            {
                return Forbid();
            }

            var activity = holiday.Activities?.FirstOrDefault(a => a.Id == idActivity);
            if (activity == null)
            {
                return NotFound("Activité introuvable");
            }

            var activityDto = _mapper.Map<ActivityDto>(activity);
            return Ok(activityDto);
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }

    }

    [HttpPost("Activity")]
    [SwaggerOperation(Summary = "Création d'une activité")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(532, "Erreur lors de sauvegarde des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "", typeof(ActivityDto))]
    public async Task<IActionResult> Create([Required][FromBody] ActivityViewModel activityViewModel, [Required][FromRoute] string idHoliday)
    {
        try
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = _validator.GetCurrentHoliday(idHoliday).Result;
            if (holiday == null)
            {
                return NotFound("Vacance introuvable");
            }

            if (!_validator.IsMemberOf(user, holiday))
            {
                return Forbid();
            }
            var activity = _mapper.Map<ActivityViewModel, Activity>(activityViewModel);

            if (activity.StartDate >= holiday.StartDate && activity.EndDate <= holiday.EndDate)
            {
                holiday.Activities!.Add(activity);
                _context.Holidays.Update(holiday);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Les dates que vous avez entré ne correspondent pas aux dates des vacances");
            }
            var activityDto = _mapper.Map<ActivityDto>(activity);
            return new OkObjectResult(activityDto);
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }
        catch (Exception ex) when(ex is NotSupportedException || ex is DbUpdateException || ex is DbUpdateConcurrencyException)
        {
            return StatusCode(532, "Une erreur lors de la sauvegarde des données. Veuillez contacter l'administrateur.");
        }

    }

    [HttpPut("Activity/{idActivity}")]
    [SwaggerOperation(Summary = "Modification d'une activité")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(533, "Erreur lors de la modification des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "")]
    public async Task<IActionResult> Edit([Required][FromBody] ActivityViewModel editActivityViewModel, [Required][FromRoute] string idHoliday,
        [Required][FromRoute] string idActivity)
    {
        try
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = _validator.GetCurrentHoliday(idHoliday).Result;
            if (holiday == null)
            {
                return NotFound("Vacance introuvable");
            }

            if (!_validator.IsMemberOf(user, holiday))
            {
                return Forbid();
            }

            var baseActivity = holiday.Activities?.FirstOrDefault(a => a.Id == idActivity);
            if (baseActivity == null)
            {
                return NotFound("Activité introuvable");
            }

            
           

            if (editActivityViewModel.StartDate >= holiday.StartDate && editActivityViewModel.EndDate <= holiday.EndDate)
            {

                baseActivity.EndDate = editActivityViewModel.EndDate;
                baseActivity.StartDate = editActivityViewModel.StartDate;
                baseActivity.Address = _mapper.Map<Address>(editActivityViewModel.Address);
                baseActivity.Description = editActivityViewModel.Description;
                baseActivity.Name = editActivityViewModel.Name;

                _context.Holidays.Update(holiday);
                await _context.SaveChangesAsync();
            }
            else 
            {
                return BadRequest("Les dates que vous avez entré ne correspondent pas aux dates des vacances");
            }
                
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

    [HttpDelete("Activity/{idActivity}")]
    [SwaggerOperation(Summary = "Suppression d'une activité")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(534, "Erreur lors de la suppression des données")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(404, "Elément introuvable")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "")]
    public async Task<IActionResult> Delete([Required][FromRoute] string idHoliday, [Required][FromRoute] string idActivity)
    {
        try 
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var holiday = _validator.GetCurrentHoliday(idHoliday).Result;
            if (holiday == null)
            {
                return NotFound("Vacance introuvable");
            }

            if (!_validator.IsMemberOf(user, holiday))
            {
                return Forbid();
            }

            var activity = holiday.Activities?.FirstOrDefault(a => a.Id == idActivity);
            if (activity == null)
            {
                return NotFound("Activité introuvable");
            }


            holiday.Activities!.Remove(activity);
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
            return StatusCode(534, "Une erreur lors de la suppresion des données. Veuillez contacter l'administrateur.");
        }

    }


}