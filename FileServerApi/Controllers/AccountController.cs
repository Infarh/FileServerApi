using FileServerApi.Models.Identity;
using FileServerApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileServerApi.Controllers;

[ApiController, Route("api/v1/account")]
public class AccountController : ControllerBase
{
    private readonly IIdentityManager _IdentityManager;
    private readonly IJWTProvider _JWTProvider;

    public AccountController(IIdentityManager IdentityManager, IJWTProvider JWTProvider)
    {
        _IdentityManager = IdentityManager;
        _JWTProvider = JWTProvider;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginModel Model)
    {
        if (!_IdentityManager.Login(Model.UserName, Model.Password))
            return BadRequest();

        var role = _IdentityManager.GetRoles(Model.UserName).FirstOrDefault();

        var token_str = _JWTProvider.GetToken(Model.UserName, role ?? "User", DateTime.Now);
        return Ok(new
        {
            Autorization = $"Bearer {token_str}",
            Token = token_str,
            Model.UserName,
        });
    }
}