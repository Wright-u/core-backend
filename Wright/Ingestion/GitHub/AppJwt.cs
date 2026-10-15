using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Wright.Ingestion.GitHub;

public class GitHubAppJwt
{
    private readonly string _appId;
    private readonly string _privateKey;

    public GitHubAppJwt(IConfiguration config)
    {
        _appId = config["GitHub:AppID"] ?? throw new InvalidOperationException("GitHub:AppID is required.");
        var keyPath = config["GitHub:PrivateKey"] ?? throw new InvalidOperationException("GitHub:PrivateKey is required.");

        _privateKey = File.ReadAllText(keyPath);
    }

    public string Generate()
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(_privateKey);
        var now = DateTimeOffset.UtcNow;

        var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256) {
            CryptoProviderFactory = new CryptoProviderFactory
            {
                CacheSignatureProviders = false
            }
        };

        var token = new JwtSecurityToken(
            issuer: _appId,
            notBefore: now.AddSeconds(-60).UtcDateTime,
            expires: now.AddMinutes(8).UtcDateTime,
            signingCredentials: credentials
        );

        token.Payload["iat"] = now.AddSeconds(-60).ToUnixTimeSeconds();
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}