using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

using FileServerApi.Infrastructure;
using FileServerApi.Services.Interfaces;

namespace FileServerApi.Services;

public class IdentityManager : IIdentityManager
{
    static IdentityManager()
    {
        __Users["Admin"] = HashPassword("123");
        __UserRoles["Admin"] = new ConcurrentHashSet<string>(new[] { "Admin", "User" });
    }

    private static readonly ConcurrentDictionary<string, string> __Users = new();

    private static readonly ConcurrentDictionary<string, ConcurrentHashSet<string>> __UserRoles = new();

    private static string HashPassword(string Password)
    {
        using var sha25 = SHA256.Create();
        var password_hash = sha25.ComputeHash(Encoding.UTF8.GetBytes(Password));
        return string.Join("", password_hash.Select(v => v.ToString("x2")));
    }

    public bool IsUserExist(string User) => __Users.ContainsKey(User);

    public bool Login(string User, string Password) => 
        __Users.TryGetValue(User, out var user_password_hash) && 
        string.Equals(user_password_hash, HashPassword(Password));

    public void Register(string User, string Password)
    {
        if (IsUserExist(User)) return;

        __Users[User] = HashPassword(Password);
        __UserRoles[User] = new (new[] { "User" });
    }

    public void AddToRole(string User, string Role)
    {
        if(!__UserRoles.TryGetValue(User, out var roles) || roles.Contains(Role)) 
            return;

        roles.Add(Role);
    }

    public void RemoveFromRole(string User, string Role)
    {
        if (!__UserRoles.TryGetValue(User, out var roles) || !roles.Contains(Role))
            return;
        roles.Remove(Role);
    }

    public bool IsInRole(string User, string Role)
    {
        if (!__UserRoles.TryGetValue(User, out var roles))
            return false;
        return roles.Contains(Role);
    }

    public IEnumerable<string> GetRoles(string User) =>
        !__UserRoles.TryGetValue(User, out var roles) 
            ? Enumerable.Empty<string>() 
            : roles.AsEnumerable();
}
