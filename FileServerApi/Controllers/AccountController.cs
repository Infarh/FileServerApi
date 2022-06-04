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

    private static readonly List<Person> __Peoples = new()
    {
        new Person { Login = "Admin", Password = "123", Role = "admin" },
        new Person { Login =  "User", Password = "321", Role = "user" }
    };

    public AccountController(IConfiguration Configuration) { _Configuration = Configuration; }

    private static ClaimsIdentity? GetIdentity(string UserName, string Password)
    {
        if (__Peoples.FirstOrDefault(x => x.Login == UserName && x.Password == Password) is not { } person)
            return null;

        Claim[] claims =
        {
            new (ClaimsIdentity.DefaultNameClaimType, person.Login),
            new (ClaimsIdentity.DefaultRoleClaimType, person.Role)
        };

        return new ClaimsIdentity(
            claims: claims,
            authenticationType: "Token",
            nameType: ClaimsIdentity.DefaultNameClaimType,
            roleType: ClaimsIdentity.DefaultRoleClaimType);
    }

    [HttpPost("token")]
    public IActionResult Token(string UserName, string Password)
    {
        if (GetIdentity(UserName, Password) is not { } identity)
            return BadRequest(new { ErrorText = "Invalid UserName or Password." });

        var key = _Configuration["JwtAuth:Key"];
        var issuer = _Configuration["JwtAuth:Issuer"];
        var audience = _Configuration["JwtAuth:Audience"];

        var expires = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_Configuration.GetValue("JwtAuth:ExpiresTimeMinutes", 15)));
        var security_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(security_key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            notBefore: DateTime.UtcNow,
            claims: identity.Claims,
            expires: expires,
            signingCredentials: credentials);

        var token_str = new JwtSecurityTokenHandler().WriteToken(jwt);

        return Ok(new
        {
            token = token_str,
            UserName = identity.Name,
            Autorization = $"Bearer {token_str}"
        });
    }

    [HttpPost("login")]
    public IActionResult Login(LoginModel Model)
    {
        var key = _Configuration["JwtAuth:Key"];
        var issuer = _Configuration["JwtAuth:Issuer"];

        var security_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(security_key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        {
            new(JwtRegisteredClaimNames.Sub, Model.UserName),
            //new(JwtRegisteredClaimNames.Email, user.Email),
            new("roles", "User"),
            new("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = DateTime.Now.AddMinutes(120);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var token_str = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            Token = token_str,
            Expires = expires,
            Autorization = $"Bearer {token_str}"
        });
    }
}