namespace FileServerApi.Services.Interfaces;

public interface IJWTProvider
{
    (string Token, DateTime Expires) GetToken(DateTime Time, string User, params string[] Roles) => GetToken(Time, User, (IEnumerable<string>)Roles);

    (string Token, DateTime Expires) GetToken(DateTime Time, string User, IEnumerable<string> Roles);
}
