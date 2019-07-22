using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthzController : ControllerBase
    {
        private IConfiguration _configuration;
        private SignInManager<IdentityUser> _signInManager;
        private UserManager<IdentityUser> _userManager;

        public AuthzController(IConfiguration configuration, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Register an account in the system.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("register")]
        public async Task<IActionResult> Register(Models.RegisterModel model)
        {
            try
            {
                // Validate the ModelState
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Create the account. Replace the result.Errors below with custom error handling.
                IdentityUser user = new IdentityUser(model.UserName);
                user.Email = model.EmailAddress;
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                return Ok();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Authentication Endpoint
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route("token")]
        public async Task<IActionResult> Authenticate(Models.LoginModel model)
        {
            try
            {
                // Validate the ModelState
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Get user if they exist. If not, obfuscate the fact they don't exist. 
                var user = _userManager.Users.Where(x => x.UserName == model.UserName).FirstOrDefault();
                if (user == null)
                    return Unauthorized();

                // Check their password. If not correct, send them unauthorized message.
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                    return Unauthorized();

                // Sign them in. Not sure if this step is really necessary.
                await _signInManager.SignInAsync(user, false, "JWT");

                // Return the created token.
                return Ok(this.CreateToken(user));
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Creates the Token Model
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private Models.JWTModel CreateToken(IdentityUser user)
        {
            Models.JWTModel token = new Models.JWTModel();
            try
            {
                token.UserName = user.UserName;
                // Configured for 1 day expiration. Change to fit need.
                token.Expiration = DateTime.UtcNow.AddDays(1);
                token.AccessToken = this.GenerateToken(user);

                return token;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Creates the Access Token with role claims.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GenerateToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtIssuerOptions")["SecretKey"]);
            

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));

            // Add Roles Claim
            var roles = _userManager.GetRolesAsync(user).Result;
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}