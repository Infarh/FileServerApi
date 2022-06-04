namespace FileServerApi.Models.Identity;

internal class Person
{
    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;
}
