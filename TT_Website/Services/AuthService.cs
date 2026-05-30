namespace TT_Website.Services;

public class AuthService
{
    private readonly IConfiguration _configuration;

    public bool IsLoggedIn { get; private set; }

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool Login(string password)
    {
        var adminPassword = _configuration["AdminSettings:Password"];

        if (!string.IsNullOrWhiteSpace(adminPassword) && password == adminPassword)
        {
            IsLoggedIn = true;
            return true;
        }

        return false;
    }

    public void Logout()
    {
        IsLoggedIn = false;
    }
}
