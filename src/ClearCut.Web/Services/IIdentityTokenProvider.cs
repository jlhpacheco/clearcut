using System.Threading.Tasks;

namespace ClearCut.Web.Services;

public interface IIdentityTokenProvider
{
    /// <summary>
    /// Retrieves a valid OIDC ID token for the specified target audience.
    /// </summary>
    Task<string> GetIdentityTokenAsync(string audience);
}
