using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FileServerApi.Models.Identity;

public class UserPasswordModel : UserModel
{
    [Required]
    public string Password { get; set; } = null!;
}