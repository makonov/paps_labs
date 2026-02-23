using ConferenceApi.Models;
using ConferenceApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ConferenceApi.Controllers;

using ConferenceApi.Models;
using ConferenceApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var token = _auth.Login(request.Username, request.Password);
        if (token == null)
            return Unauthorized(new { error = "invalid_credentials", message = "Неверный логин или пароль" });

        return Ok(new JwtResponse { AccessToken = token });
    }
}