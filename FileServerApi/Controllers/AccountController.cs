using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

using FileServerApi.Models;
using FileServerApi.Models.Identity;

using Microsoft.AspNetCore.Mvc;

using Microsoft.IdentityModel.Tokens;

namespace FileServerApi.Controllers;

[ApiController, Route("api/v1/account")]
public class AccountController : ControllerBase
{
    private readonly IConfiguration _Configuration;

    public AccountController(IConfiguration Configuration) { _Configuration = Configuration; }

    [HttpPost("login")]
    public IActionResult Login(LoginModel Model)
    {
        var key = _Configuration["JwtAuth:Key"];
        var issuer = _Configuration["JwtAuth:Issuer"];
        var audience = _Configuration["JwtAuth:Audience"];

        var security_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(security_key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        {
            new(JwtRegisteredClaimNames.Sub, Model.UserName),
            new("roles", "User"),
            new("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = DateTime.Now.AddMinutes(120);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var token_str = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            Autorization = $"Bearer {token_str}",
            Token = token_str,
            Model.UserName,
            Expires = expires,
        });
    }
}