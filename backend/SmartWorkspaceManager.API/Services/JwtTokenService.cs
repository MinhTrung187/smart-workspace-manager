using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SmartWorkspaceManager.Domain.Entities;
using SmartWorkspaceManager.Application.Interfaces;

namespace SmartWorkspaceManager.API.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_options.Secret) || Encoding.UTF8.GetByteCount(_options.Secret) < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be configured and contain at least 32 bytes.");
        }

        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.ExpiresInMinutes);

        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object?>
        {
            ["sub"] = user.Id.ToString(),
            ["email"] = user.Email,
            ["name"] = user.FullName,
            ["iss"] = _options.Issuer,
            ["aud"] = _options.Audience,
            ["iat"] = ToUnixTimeSeconds(now),
            ["exp"] = ToUnixTimeSeconds(expiresAt)
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";
        var signature = Sign(unsignedToken);

        return ($"{unsignedToken}.{signature}", expiresAt);
    }

    private string Sign(string value)
    {
        var key = Encoding.UTF8.GetBytes(_options.Secret);
        using var hmac = new HMACSHA256(key);
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }

    private static long ToUnixTimeSeconds(DateTime value)
    {
        return new DateTimeOffset(value).ToUnixTimeSeconds();
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
