using JoliDay.Dto;
using JoliDay.Services;
using JoliDay.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace JoliDay.Controllers
{
    [ApiController]
    [Route("Email")]
    public class ContactController : ControllerBase
    {
        private readonly IServiceEmail _serviceEmail;

        public ContactController(IServiceEmail serviceEmail) 
        { 
            this._serviceEmail = serviceEmail;
        }

        [HttpPost]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Envoi d'un e-mail en cas de problème")]
        [SwaggerResponse(200, "")]
        [SwaggerResponse(404, "Erreur lors de l'envoi de l'email")]
        public async Task<IActionResult> ContactMe([Required][FromBody] ContactMeViewModel contactMe) 
        {
            if (_serviceEmail.SendEmail(contactMe))
            {
                return Ok();
            }
            else
            { 
                return BadRequest();
            }
            
        }

    }
}
