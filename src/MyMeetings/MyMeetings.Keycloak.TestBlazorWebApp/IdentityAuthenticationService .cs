using Microsoft.AspNetCore.Components.Authorization;

namespace MyMeetings.Keycloak.TestBlazorWebApp;

// from https://codyanhorn.tech/blog/blazor/2020/09/06/Blazor-Server-Get-Access-Token-for-User.html
public class AuthUserProfile
{
    public string Name { get; set; }
    public string PreferredUsername { get; set; }
}

public class AuthUser
{
    public string AccessToken { get; set; }
    public AuthUserProfile Profile { get; set; }
}

public interface IAuthenticationService
{
    bool IsAuthenticated { get; }
    AuthUser User { get; }
    string AccessToken { get; }

    Task Setup();
    //string LoginUrl(
    //    string redirectUri = null
    //);
    //string LogoutUrl();
}

public class IdentityAuthenticationService : IAuthenticationService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public IdentityAuthenticationService(
        AuthenticationStateProvider authenticationStateProvider
    )
    {
        _authenticationStateProvider = authenticationStateProvider;
        // This brings me great *shame*, but it works.
        Setup().GetAwaiter().GetResult();
    }

    public bool IsAuthenticated { get; private set; } = false;
    public AuthUser User { get; private set; }
    public string AccessToken => User?.AccessToken ?? "";

    public async Task Setup()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        IsAuthenticated = false;
        if (authState.User.Identity.IsAuthenticated)
        {
            // Here we grab some details we need from the AuthState User
            var inRole = authState.User.IsInRole("Admin");
            // Here is where we cache the AccessToke for later use.
            var accessToken = authState.User.Claims.Where(a => a.Type == "access_token").FirstOrDefault()?.Value;
            var name = authState.User.Claims.Where(a => a.Type == "name").FirstOrDefault()?.Value;
            var preferredUsername = authState.User.Claims.Where(a => a.Type == "preferred_username").FirstOrDefault()?.Value;

            User = new AuthUser
            {
                AccessToken = accessToken,
                Profile = new AuthUserProfile
                {
                    Name = name ?? preferredUsername,
                    PreferredUsername = preferredUsername
                }
            };
            IsAuthenticated = true;
        }
    }

    //public string LoginUrl(
    //    string redirectUri
    //) => $"/Account/Login?redirectUri={redirectUri}";

    //public string LogoutUrl() => "/Account/Logout";
}