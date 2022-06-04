namespace FileServerApi.Services.Interfaces;

public interface IJWTProvider
{
    string GetToken(string User, string Role, DateTime Time);
}
