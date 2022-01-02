namespace Crews.API;

public class TokenOptions
{
    public const string Token = "Token";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;
}