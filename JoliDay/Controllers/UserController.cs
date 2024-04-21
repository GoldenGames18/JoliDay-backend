using JoliDay.Dto;
using JoliDay.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using JoliDay.Services;
using Swashbuckle.AspNetCore.Annotations;


namespace JoliDay.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IServiceValidator _validator;


        public UserController(UserManager<User> userManager, IMapper mapper, IServiceValidator validator)
        {
            this._userManager = userManager;
            this._mapper = mapper;
           
            this._validator = validator;

        }

        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Chercher un utilisateur par son token")]
        [SwaggerResponse(531, "Erreur lors de la récupération des données")]
        [SwaggerResponse(401, "Token invalide ou expiré")]
        [SwaggerResponse(403, "Utilisateur introuvable")]
        [SwaggerResponse(200, "", typeof(UserDto))]
        public async Task<IActionResult> FindUserByToken() {
            try
            {
                var user = _validator.GetCurrentUser(User.FindFirstValue(ClaimTypes.Email)).Result;
                if (user == null)
                {
                    return NotFound("Utilisateur introuvable");
                }
                return new OkObjectResult(_mapper.Map<User, UserDto>(user));
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is OperationCanceledException )
            {
                return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
            }

        }

        [HttpGet("Statistic")]
        [SwaggerOperation(Summary = "Nombre d'utilisateur")]
        [SwaggerResponse(535, "Erreur lors de la récupération des données")]
        [SwaggerResponse(200, "Connexion au flux sse")]
        public async Task<IActionResult> UsersCount()
        {
            try 
            {
                Response.Headers.Add("Content-Type", "text/event-stream");
                var back = 0;
                while (true)
                {
                    var message = _userManager.Users.Count();
                    if (message > back)
                    {
                        back = message;
                        var data = $"event:user\ndata:{message}\n\n";
                        byte[] tab = ASCIIEncoding.ASCII.GetBytes(data);
                        await Response.Body.WriteAsync(tab, 0, tab.Length);
                        await Response.Body.FlushAsync();
                    }

                    if (HttpContext.RequestAborted.IsCancellationRequested == true)
                    {
                        break;
                    }
                    await Task.Delay(1000);
                }
                return Ok();
            }catch(Exception ex) when(ex is OverflowException||ex is EncoderFallbackException||ex is  InvalidOperationException ||ex is ArgumentOutOfRangeException || ex is ObjectDisposedException || ex is ArgumentNullException || ex is ArgumentException || ex is NotSupportedException || ex is ObjectDisposedException)
            {
                return StatusCode(535, "Arret du flux sse.");
            }
            
        }



    }
}