using System.ComponentModel.DataAnnotations;

namespace FileServerApi.Models.Identity;

public class User
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    [Required]
    public string Role { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.Now;
}
