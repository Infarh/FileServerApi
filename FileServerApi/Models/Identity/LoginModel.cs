using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FileServerApi.Models.Identity;

public class LoginModel
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}