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

[ApiController]
[Route("[controller]")]
[Authorize]
public class InvitationController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IServiceValidator _validator;
    private readonly JoliDayContext _context;

    public InvitationController(IMapper mapper, JoliDayContext context, IServiceValidator serviceValidator)
    {
        this._mapper = mapper;
        this._context = context;
        _validator = serviceValidator;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Création d'une invitation")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(532, "Erreur lors de sauvegarde des données")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "")]
    public async Task<IActionResult> CreateInvite([Required][FromBody] InviteViewModel inviteViewModel)
    {
        try 
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }


            var holiday = await _context.Holidays.Include(h => h.Owner)
            .Include(holiday => holiday.Users)
            .FirstOrDefaultAsync(h => h.Id == inviteViewModel.HolidayId);

            if (holiday == null) 
            {
                return NotFound("Vacance introuvable");
            }

            if (!_validator.IsOwner(user,holiday))
            {
                return Forbid();
            }

            var invitedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == inviteViewModel.Email);
            if (invitedUser == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            if (invitedUser == user)
            {
                return new BadRequestObjectResult(new Error()
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "Vous ne pouvez pas vous inviter vous-même"
                });
                
            }

            if (holiday.Users != null && holiday.Users.Contains(invitedUser))
            {
             
                return new BadRequestObjectResult(new Error()
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "L'utilisateur est déjà invité."
                });
            }

            var response = await _context.Invites.Include(i => i.User).Include(i => i.Holiday).AnyAsync(i => i.User.Id == invitedUser.Id && i.Holiday.Id == holiday.Id);
            if (response)
            {
                return new BadRequestObjectResult(new Error()
                {
                    Code = StatusCodes.Status400BadRequest,
                    Message = "L'utilisateur a déjà reçu une invitation."
                });
            }

            _context.Invites.Add(new Invite
            {
                Holiday = holiday,
                User = invitedUser,
            });
            await _context.SaveChangesAsync();
            return Ok();
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
    [Route("{id}")]
    [SwaggerOperation(Summary = "Modification d'une invitation")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(533, "Erreur lors de la modification des données")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "")]
    public async Task<IActionResult> ReadInvite([Required][FromRoute] string id)
    {
        try 
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

           
            var invite = await _context.Invites.Include(i => i.User).Include(i => i.Holiday)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (invite == null) 
            {
                return NotFound("Invitation introuvable");
            }

            if (user != invite.User)
            {
                return Forbid();
            }

            invite.IsRead = true;
            _context.Invites.Update(invite);
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

    [HttpGet]
    [SwaggerOperation(Summary = "Récupération de toutes les invitations")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "", typeof(List<InviteDto>))]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var invites = await _context.Invites.Include(i => i.User).Include(i => i.Holiday)
                .Where(i => i.User.Id == user.Id).ToListAsync();
            var invitesDto = _mapper.Map<List<InviteDto>>(invites);
            return Ok(invitesDto);
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
        {
            return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
        }


    }

    [HttpDelete]
    [Route("{id}")]
    [SwaggerOperation(Summary = "Suppression d'une invitation")]
    [SwaggerResponse(531, "Erreur lors de la récupération des données")]
    [SwaggerResponse(534, "Erreur lors de la suppression des données")]
    [SwaggerResponse(404, "Vacances introuvables")]
    [SwaggerResponse(401, "Token invalide ou expiré")]
    [SwaggerResponse(403, "Pas membres de la holiday")]
    [SwaggerResponse(200, "Accepter une invitation", typeof(HolidayDto))]
    public async Task<IActionResult> HandleInvite([Required][FromBody] HandleInvitationViewModel acceptInvite, [Required][FromRoute]string id)
    {
        try
        {
            var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
            if (user == null)
            {
                return NotFound("Utilisateur introuvable");
            }

            var invite = await _context.Invites.Include(i => i.User).Include(i => i.Holiday)
                .ThenInclude(holiday => holiday.Users)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invite == null) 
            {
                return NotFound("Invitation introuvable");
            }

            if (user != invite.User)
            {
                return Forbid();
            }

            if (acceptInvite.Accept)
            {
                var holiday = await _context.Holidays
                    .Include(h => h.Owner).Include(h => h.Transactions)
                    .Include(h => h.Address)
                    .Include(h => h.Users)
                    .Include(h => h.Activities).ThenInclude(a => a.Address)
                    .FirstOrDefaultAsync(h => h.Id == invite.Holiday.Id);

                if (holiday == null)
                {
                    return NotFound("Vacances introuvables");
                }

                holiday.Users ??= new List<User>();
                holiday.Users.Add(user);
                _context.Holidays.Update(holiday);
                _context.Invites.Remove(invite);
                await _context.SaveChangesAsync();
                var holidayDto = _mapper.Map<HolidayDto>(holiday);
                return Ok(holidayDto);
            }

            _context.Invites.Remove(invite);
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