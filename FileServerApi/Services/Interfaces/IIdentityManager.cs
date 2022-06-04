namespace FileServerApi.Services.Interfaces;

public interface IIdentityManager
{
    bool IsUserExist(string User);

    bool Login(string User, string Password);

    void Register(string User, string Password);

    void AddToRole(string User, string Role);

    void RemoveFromRole(string User, string Role);

    bool IsInRole(string User, string Role);

    IEnumerable<string> GetRoles(string User);
}
