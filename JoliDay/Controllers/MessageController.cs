using AutoMapper;
using JoliDay.Dto;
using JoliDay.Models;
using JoliDay.Services;
using JoliDay.Utils;
using JoliDay.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PusherServer;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace JoliDay.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly JoliDayContext _context;
        private readonly IServiceValidator _validator;
        public MessageController(IMapper mapper, JoliDayContext context, IServiceValidator serviceValidator) 
        {
            this._mapper = mapper;
            this._context = context;
            this._validator = serviceValidator; 
        } 

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Récupération des messages")]
        [SwaggerResponse(531, "Erreur lors de la récupération des données")]
        [SwaggerResponse(401, "Token invalide ou expiré")]
        [SwaggerResponse(403, "Pas membres de la holiday")]
        [SwaggerResponse(404, "Vacances indisponibles")]
        [SwaggerResponse(200, "", typeof(List<MessageDto>))]
        public async Task<IActionResult> LoadMessage([Required][FromRoute]string id)
        {
            try
            {
                var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
                if (user == null)
                {
                    return NotFound("Utilisateur introuvable");
                }

                var holiday = await _context.Holidays.Include(h => h.Owner).Include(h => h.Users).Include(h => h.Messages).ThenInclude(m => m.Owner).FirstOrDefaultAsync(h => h.Id == id);

                if (holiday == null)
                {
                    return NotFound("Vacances indisponibles");
                }

                if (!_validator.IsMemberOf(user, holiday))
                {
                    return Forbid();
                }

                var messages = holiday.Messages.Skip(Math.Max(0, holiday.Messages.Count - 100)).Take(100).ToList();
                return new OkObjectResult(_mapper.Map<List<MessageDto>>(messages));
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
            {
                return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
            }

        }

        [HttpPost("{id}")]
        [SwaggerOperation(Summary = "Création d'un message")]
        [SwaggerResponse(531, "Erreur lors de la récupération des données")]
        [SwaggerResponse(401, "Token invalide ou expiré")]
        [SwaggerResponse(403, "Pas membres de la holiday")]
        [SwaggerResponse(404, "Vacances indisponibles")]
        [SwaggerResponse(200, "")]
        public async Task<IActionResult> SendMessage([Required][FromBody] MessageViewModel messageViewModel , [Required][FromRoute]string id) 
        {
            try 
            {
                var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
                if (user == null)
                {
                    return NotFound("Utilisateur introuvable");
                }
                var holiday = await _context.Holidays.Include(h => h.Owner).Include(h => h.Users).Include(h => h.Messages).ThenInclude(m => m.Owner).FirstOrDefaultAsync(h => h.Id == id);

                if (holiday == null)
                {
                    return NotFound("Vacances indisponibles");
                }

                if (!_validator.IsMemberOf(user,holiday))
                {
                    return Forbid();
                }

                var message = new Message() { Content = messageViewModel.Content, Owner = user };
                holiday.Messages.Add(message);
                await _context.SaveChangesAsync();

                var options = new PusherOptions
                {
                    Cluster = "eu",
                    Encrypted = true,

                };


                var pusher = new Pusher(
                  "",
                  "",
                  "",
                  options);


                var result = await pusher.TriggerAsync(
                  holiday.Id.ToString(),
                  "message",
                  _mapper.Map<Message, MessageDto>(message));


                return Ok();
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException)
            {
                return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
            }
        }
    }
}
