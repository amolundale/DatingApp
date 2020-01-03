using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace DatingApp.API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            
            //if(!ModelState.IsValid)
            //return BadRequest(ModelState);
        
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if(await _repo.UserExits(userForRegisterDto.Username))
                return BadRequest("Username Already Exists");

                var userToCreate = new User{
                    Username = userForRegisterDto.Username
                };

                var creatUser = await _repo.Register(userToCreate, userForRegisterDto.Password);
                return StatusCode(201);
        }


    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
    {
        var userFromRepo = await _repo.Login(userForLoginDto.Username,userForLoginDto.Password);

        if(userFromRepo == null)
        return Unauthorized();

        var claims = new []
        {
            new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
            new Claim(ClaimTypes.Name,userFromRepo.Username)
        };

        var s = _config.GetSection("AppSettings:Token").Value.ToString();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(/*_config.GetSection("AppSettings.Token").Value*/s));
        
        var creads = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor  = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creads
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new {token= tokenHandler.WriteToken(token)});

    }  


    }
}