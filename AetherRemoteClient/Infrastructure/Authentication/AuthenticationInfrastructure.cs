using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Domain.Exceptions.Network;
using AetherRemoteClient.Utils.Extensions;
using AetherRemoteCommon.Network.Domain.Api;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;

namespace AetherRemoteClient.Infrastructure.Authentication;

/// <summary>
///     Exposes methods for authenticating against the server's https api
/// </summary>
public class AuthenticationInfrastructure : IDisposable
{
    // Where we should post for authentication
#if DEBUG
    // private const string AuthenticationUrl = "https://localhost:5006/api/auth/login"; // Local
    private const string AuthenticationUrl = "https://foxitsvc.com:5017/api/auth/login"; // Beta
#else
    private const string AuthenticationUrl = "https://foxitsvc.com:5006/api/auth/login"; // Prod
#endif
    
    // Long-lived HttpClient
    private static readonly HttpClient Client = new();
    private static readonly JsonWebTokenHandler JwtTokenHandler = new();
    
    // For async locking support
    private readonly SemaphoreSlim _lock = new(1, 1);

    // The information used in the authentication process
    private string? _secret;
    private string? _token;
    private DateTimeOffset _expiresAtUtc;

    /// <summary> Sets the secret to authenticate against when connecting to SignalR server</summary>
    public void SetSecret(string secret)
    {
        if (_secret == secret)
            return;
        
        _secret = secret;
        
        // Invalidate tokens here to prevent a sign-out & sign-in from using the previous session's cached token
        InvalidateToken();
    }

    /// <summary> Gets or refreshes a token using the secret provided from <see cref="SetSecret"/></summary>
    public async Task<string?> GetTokenAsync()
    {
        if (CachedTokenValid())
            return _token;

        try
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            
            // Check again because the token could have expired between waiting for the lock and acquiring it
            if (CachedTokenValid())
                return _token;

            await RefreshToken().ConfigureAwait(false);

            return _token;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    ///     Invalidate any current tokens
    /// </summary>
    public void InvalidateToken()
    {
        _token = null;
        _expiresAtUtc = DateTimeOffset.MinValue;
    }

    private async Task RefreshToken()
    {
        if (string.IsNullOrWhiteSpace(_secret))
            throw new ArAuthAuthenticationException(ArAuthAuthenticationErrorCode.SecretNotSetOrInvalid);

        try
        {
            var request = new GetTokenRequest(_secret, Plugin.Version);
            var payload = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                
            var response = await Client.PostAsync(AuthenticationUrl, payload).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (JsonConvert.DeserializeObject<GetTokenResponse>(content) is not { } result)
                throw new ArAuthAuthenticationException(ArAuthAuthenticationErrorCode.InvalidOrMalformedToken);

            // This is one of the more regular failure paths for things like version mismatch, unknown secret, etc.
            if (result.Result is not GetTokenEc.Success)
                throw new ArAuthAuthenticationException(result.Result.ToArAuthAuthenticationErrorCode());

            if (JwtTokenHandler.CanReadToken(result.Secret) is false)
                throw new ArAuthAuthenticationException(ArAuthAuthenticationErrorCode.InvalidOrMalformedToken);
            
            var token = JwtTokenHandler.ReadToken(result.Secret);
            if (token.ValidTo < DateTimeOffset.UtcNow)
                throw new ArAuthAuthenticationException(ArAuthAuthenticationErrorCode.InvalidOrMalformedToken);
            
            _token = result.Secret;
            _expiresAtUtc = token.ValidTo;
        }
        catch (HttpRequestException e) when (e.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
        {
            throw new ArAuthAuthenticationException(ArAuthAuthenticationErrorCode.AuthenticationServerUnreachable);
        }
    }

    private bool CachedTokenValid()
    {
        if (string.IsNullOrWhiteSpace(_token))
            return false;
        
        // Include a 10-minute buffer just to refresh early since tokens last 4 hours
        return _expiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(10);
    }

    public void Dispose()
    {
        _lock.Dispose();
        GC.SuppressFinalize(this);
    }
}