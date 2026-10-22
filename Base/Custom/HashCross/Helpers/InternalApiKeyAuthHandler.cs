using System.Security.Claims;
using System.Text.Encodings.Web;
using Base.Custom.HashCross.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Base.Custom.HashCross.Helpers.HashCrossEnum.HashCrossProtocol;

public sealed class InternalApiKeyAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder, IConfiguration configuration) :
    AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly IConfiguration _configuration = configuration;
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HttpHeader.HASHAPIKEY, out var providedKey))
            return Task.FromResult(AuthenticateResult.NoResult());

        var expectedKey = _configuration[HashCrossConfigPath.AUTH_INTERNALAPIKEY];

        if (string.IsNullOrWhiteSpace(expectedKey))
            return Task.FromResult(AuthenticateResult.Fail("Internal API key is not configured."));

        var providedKeyValue = providedKey.ToString();

        if (string.IsNullOrWhiteSpace(providedKeyValue))
            return Task.FromResult(AuthenticateResult.Fail("Internal API key is missing."));

        if (!string.Equals(providedKeyValue, expectedKey, StringComparison.Ordinal))
            return Task.FromResult(AuthenticateResult.Fail("Invalid internal API key."));

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, CommonApiAuth.NameIdentifier),
        new Claim(ClaimTypes.Name, CommonApiAuth.DisplayName),
        new Claim(CommonApiAuth.ClientTypeClaim, CommonApiAuth.ClientTypeValue)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}