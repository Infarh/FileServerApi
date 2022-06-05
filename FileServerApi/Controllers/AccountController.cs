using System.IdentityModel.Tokens.Jwt;

using FileServerApi.Models.Identity;
using FileServerApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileServerApi.Controllers;

[ApiController, Route("api/v1/account")]
public class AccountController : ControllerBase
{
    private readonly IIdentityManager _IdentityManager;
    private readonly IJWTProvider _JWTProvider;
    private readonly ILogger<AccountController> _Logger;

    public AccountController(IIdentityManager IdentityManager, IJWTProvider JWTProvider, ILogger<AccountController> Logger)
    {
        _IdentityManager = IdentityManager;
        _JWTProvider = JWTProvider;
        _Logger = Logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Login(UserPasswordModel Model)
    {
        if (!_IdentityManager.Login(Model.UserName, Model.Password))
            return BadRequest();

        var role = _IdentityManager.GetRoles(Model.UserName);

        var time = DateTime.Now;
        var (token_str, expires) = _JWTProvider.GetToken(time, Model.UserName, role);

        //var encoder = new JwtSecurityTokenHandler();
        //var token_src = encoder.ReadJwtToken(token_str);

        _Logger.LogInformation("Сформирован jwt для {0}. Выдан {1}. Истекает {2}", 
            Model.UserName, time, expires);

        return Ok(new
        {
            Autorization = $"Bearer {token_str}",
            Token = token_str,
            Model.UserName,
            Expires = expires,
        });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Register(UserPasswordModel Model)
    {
        _IdentityManager.Register(Model.UserName, Model.Password);
        return Ok(new { Model.UserName });
    }

    [HttpPost("roles/add/{Role}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult AddRole(UserModel Model, string Role)
    {
        _IdentityManager.AddToRole(Model.UserName, Role);
        return Ok(new { Model.UserName, Role });
    }

    [HttpGet("roles/{UserName}")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public IActionResult GetRoles(string UserName)
    {
        var roles = _IdentityManager.GetRoles(UserName);
        return Ok(roles);
    }

    [HttpPost("roles/remove/{Role}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult RemoveRole(UserModel Model, string Role)
    {
        _IdentityManager.RemoveFromRole(Model.UserName, Role);
        return Ok(new { Model.UserName, Role });
    }

    [HttpGet("{UserName}/role/{Role}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult IsInRole(string UserName, string Role)
    {
        var result = _IdentityManager.IsInRole(UserName, Role);
        return Ok(new
        {
            User = UserName,
            Role,
            IsIn = result,
        });
    }
}