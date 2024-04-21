using AutoMapper;
using Google.Apis.Auth;
using JoliDay.Dto;
using JoliDay.Models;
using JoliDay.Services;
using JoliDay.Utils;
using JoliDay.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;

namespace JoliDay.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IServiceToken _serviceToken;
        private readonly IMapper _mapper;
        public AuthenticationController(UserManager<User> userManager, IServiceToken serviceToken, IMapper mapper) 
        {
            this._userManager = userManager;
            this._mapper = mapper;
            this._serviceToken = serviceToken;
           
        }

        #region connection

        [HttpPost("Register")]
        [SwaggerOperation(Summary = "Inscription d'un utilisateur")]
        [SwaggerResponse(536, "Erreur lors de la génération du token")]
        [SwaggerResponse(400, "Erreur email indisponible")]
        [SwaggerResponse(200, "", typeof(UserCredentialDto))]
        [SwaggerResponse(401, "Token invalide ou expiré")]
        public async Task<IActionResult> Create([Required][FromBody] RegisterViewModel register)
        {
            try
            {
                var requestEmail = await _userManager.FindByEmailAsync(register.Email);
                if (requestEmail != null)
                    return new BadRequestObjectResult(new Error()
                    { Code = StatusCodes.Status400BadRequest, Message = "Email indisponible" });

                User user = new User()
                {
                    Email = register.Email,
                    FirstName = register.FirstName,
                    Name = register.Name,
                    UserName = register.Email,
                    NormalizedEmail = register.Email
                };
                var request = await _userManager.CreateAsync(user, register.Password);
                if (!request.Succeeded)
                    return new BadRequestObjectResult(new Error()
                    { Message = request.Errors.ToArray()[0].Description, Code = StatusCodes.Status400BadRequest });


                if (!_userManager.AddToRoleAsync(user, "Vacationer").Result.Succeeded && !request.Succeeded)
                    return new BadRequestObjectResult(new Error()
                    {
                        Code = StatusCodes.Status400BadRequest,
                        Message = "Erreur lors de la création du compte. Merci de contacter l'administrateur"
                    });



                var userViewModel = _mapper.Map<User, UserDto>(user);

                return new OkObjectResult(new UserCredentialDto()
                {
                    User = userViewModel,
                    Token = new JwtSecurityTokenHandler().WriteToken(_serviceToken.GenereteToken(user))
                });
            }
            catch (Exception ex) when(ex is ArgumentNullException || ex is ArgumentException || ex is SecurityTokenEncryptionFailedException)
            {
                return StatusCode(536, "Une erreur lors de la génération du token. Veuillez contacter l'administrateur.");
            }

            
        }

        [HttpPost("Login")]
        [SwaggerOperation(Summary = "Connexion d'un utilisateur")]
        [SwaggerResponse(536, "Erreur lors de la génération du token")]
        [SwaggerResponse(531, "Erreur lors de la récupération des données")]
        [SwaggerResponse(400, "Erreur lors de la connexion")]
        [SwaggerResponse(200, "", typeof(UserCredentialDto))]
        public async Task<IActionResult> Login([Required][FromBody] LoginViewModel login)
        {

            try 
            {
                User? user = await _userManager.FindByEmailAsync(login.Email);

                if (user == null)
                    return new BadRequestObjectResult(new Error()
                    { Message = "Email ou mot de passe invalide", Code = StatusCodes.Status400BadRequest });

                if (!await _userManager.CheckPasswordAsync(user, login.Password))
                    return new BadRequestObjectResult(new Error()
                    { Message = "Email ou mot de passe invalide", Code = StatusCodes.Status400BadRequest });



                var userViewModel = _mapper.Map<User, UserDto>(user);

                return new OkObjectResult(new UserCredentialDto()
                {
                    User = userViewModel,
                    Token = new JwtSecurityTokenHandler().WriteToken(_serviceToken.GenereteToken(user))
                });
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentException || ex is SecurityTokenEncryptionFailedException)
            {
                return StatusCode(536, "Une erreur lors de la génération du token. Veuillez contacter l'administrateur.");
            }
            catch (Exception ex) when (ex is OperationCanceledException)
            {
                return StatusCode(531, "Une erreur lors de la récupération des données. Veuillez contacter l'administrateur.");
            }


        }

        [HttpPost("Google")]
        [SwaggerOperation(Summary = "Connexion d'un utilisateur avec Google")]
        [SwaggerResponse(536, "Erreur lors de la génération du token")]
        [SwaggerResponse(537, "Token invalide")]
        [SwaggerResponse(400, "Erreur token invalide")]
        [SwaggerResponse(200, "", typeof(UserCredentialDto))]
        public async Task<IActionResult> GoogleAccount([Required][FromBody] GoogleTokenViewModel google)
        {


            try
            {
                var data = GoogleJsonWebSignature.ValidateAsync(google.Token).Result;
                if (data == null)
                    return BadRequest(new Error() { Code = 400, Message = "Errer" });

                User? user = await _userManager.FindByEmailAsync(data.Email);
                if (user == null)
                {
                    user = new User
                    {
                        Email = data.Email,
                        FirstName = data.GivenName,
                        Name = data.FamilyName,
                        UserName = data.Email,
                        NormalizedEmail = data.Email,
                        PathPicture = data.Picture
                    };

                    var request = await _userManager.CreateAsync(user);
                    if (!request.Succeeded)
                        return new BadRequestObjectResult(new Error()
                        { Message = request.Errors.ToArray()[0].Description, Code = StatusCodes.Status400BadRequest });

                    if (!_userManager.AddToRoleAsync(user, "Vacationer").Result.Succeeded && !request.Succeeded)
                        return new BadRequestObjectResult(new Error()
                        {
                            Code = StatusCodes.Status400BadRequest,
                            Message = "Erreur lors de la création du compte. Merci de contacter l'administrateur"
                        });

                }
                var userViewModel = _mapper.Map<User, UserDto>(user);
                return new OkObjectResult(new UserCredentialDto()
                {
                    User = userViewModel,
                    Token = new JwtSecurityTokenHandler().WriteToken(_serviceToken.GenereteToken(user))
                });
            }
            catch (InvalidJwtException)
            {
                return StatusCode(537, "Token invalide");
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentException || ex is SecurityTokenEncryptionFailedException)
            {
                return StatusCode(536, "Une erreur lors de la génération du token. Veuillez contacter l'administrateur.");
            }
        }

        #endregion

    }
}
