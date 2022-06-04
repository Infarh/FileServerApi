using System.ComponentModel.DataAnnotations;

namespace FileServerApi.Models.Identity;

public class UserModel
{
    [Required]
    public string UserName { get; set; } = null!;
}
