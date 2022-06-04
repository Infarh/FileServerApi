using FileServerApi.Models.Identity;
using FileServerApi.Services.Interfaces;
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
    public IActionResult Login(LoginModel Model)
    {
        if (!_IdentityManager.Login(Model.UserName, Model.Password))
            return BadRequest();

        var role = _IdentityManager.GetRoles(Model.UserName).FirstOrDefault() ?? "User";

        var time = DateTime.Now;
        var (token_str, expires) = _JWTProvider.GetToken(Model.UserName, role, time);

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
}