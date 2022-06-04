namespace FileServerApi.Services.Interfaces;

public interface IJWTProvider
{
    (string Token, DateTime Expires) GetToken(string User, string Role, DateTime Time);
}
