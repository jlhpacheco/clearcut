using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;

namespace ClearCut.Web.Services;

public class GoogleIdentityTokenProvider : IIdentityTokenProvider
{
    private readonly ConcurrentDictionary<string, OidcToken> _tokenCache = new(StringComparer.Ordinal);

    public async Task<string> GetIdentityTokenAsync(string audience)
    {
        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new ArgumentException("Audience cannot be null or empty.", nameof(audience));
        }

        if (!_tokenCache.TryGetValue(audience, out var oidcToken))
        {
            var credential = await GoogleCredential.GetApplicationDefaultAsync();
            oidcToken = await credential.GetOidcTokenAsync(OidcTokenOptions.FromTargetAudience(audience));
            _tokenCache[audience] = oidcToken;
        }

        var token = await oidcToken.GetAccessTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Failed to retrieve a valid OIDC ID token.");
        }
        return token;
    }
}
