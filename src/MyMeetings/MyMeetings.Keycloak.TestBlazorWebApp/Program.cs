using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MyMeetings.Keycloak.TestBlazorWebApp;
using MyMeetings.Keycloak.TestBlazorWebApp.Components;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var authServerSettings = new AuthServerSettings();
builder.Configuration.GetRequiredSection(nameof(AuthServerSettings))
    .Bind(authServerSettings);

// Add services to the container.
builder.Services.AddAuthentication(MS_OIDC_SCHEME)
    .AddOpenIdConnect(MS_OIDC_SCHEME, oidcOptions =>
    {
        // For the following OIDC settings, any line that's commented out
        // represents a DEFAULT setting. If you adopt the default, you can
        // remove the line if you wish.

        // ........................................................................
        // The OIDC handler must use a sign-in scheme capable of persisting 
        // user credentials across requests.

        oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // ........................................................................

        // ........................................................................
        // The "openid" and "profile" scopes are required for the OIDC handler 
        // and included by default. You should enable these scopes here if scopes 
        // are provided by "Authentication:Schemes:MicrosoftOidc:Scope" 
        // configuration because configuration may overwrite the scopes collection.

        //oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);
        // ........................................................................

        // ........................................................................
        // The following paths must match the redirect and post logout redirect 
        // paths configured when registering the application with the OIDC provider. 
        // For Microsoft Entra ID, this is accomplished through the "Authentication" 
        // blade of the application's registration in the Azure portal. Both the
        // signin and signout paths must be registered as Redirect URIs. The default 
        // values are "/signin-oidc" and "/signout-callback-oidc".
        // Microsoft Identity currently only redirects back to the 
        // SignedOutCallbackPath if authority is 
        // https://login.microsoftonline.com/{TENANT ID}/v2.0/ as it is above. 
        // You can use the "common" authority instead, and logout redirects back to 
        // the Blazor app. For more information, see 
        // https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/5783

        //oidcOptions.CallbackPath = new PathString("/signin-oidc");
        //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
        // ........................................................................

        // ........................................................................
        // The RemoteSignOutPath is the "Front-channel logout URL" for remote single 
        // sign-out. The default value is "/signout-oidc".

        //oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");
        // ........................................................................

        // ........................................................................
        // The following example Authority is configured for Microsoft Entra ID
        // and a single-tenant application registration. Set the {TENANT ID} 
        // placeholder to the Tenant ID. The "common" Authority 
        // https://login.microsoftonline.com/common/v2.0/ should be used 
        // for multi-tenant apps. You can also use the "common" Authority for 
        // single-tenant apps, but it requires a custom IssuerValidator as shown 
        // in the comments below. 

        oidcOptions.Authority = authServerSettings.Authority;
        // ........................................................................

        // ........................................................................
        // Set the Client ID for the app. Set the {CLIENT ID} placeholder to
        // the Client ID.

        oidcOptions.ClientId = authServerSettings.ClientId;
        // ........................................................................

        // ........................................................................
        // ClientSecret shouldn't be compiled into the application assembly or 
        // checked into source control. Adopt User Secrets, Azure KeyVault, 
        // or an environment variable to supply the value. Authentication scheme 
        // configuration is automatically read from 
        // "Authentication:Schemes:{SchemeName}:{PropertyName}", so ClientSecret is 
        // for OIDC configuration is automatically read from 
        // "Authentication:Schemes:MicrosoftOidc:ClientSecret" configuration.

        oidcOptions.ClientSecret = authServerSettings.ClientSecret;
        // ........................................................................

        // ........................................................................
        // Setting ResponseType to "code" configures the OIDC handler to use 
        // authorization code flow. Implicit grants and hybrid flows are unnecessary
        // in this mode. In a Microsoft Entra ID app registration, you don't need to 
        // select either box for the authorization endpoint to return access tokens 
        // or ID tokens. The OIDC handler automatically requests the appropriate 
        // tokens using the code returned from the authorization endpoint.

        oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
        // ........................................................................

        // ........................................................................
        // Many OIDC servers use "name" and "role" rather than the SOAP/WS-Fed 
        // defaults in ClaimTypes. If you don't use ClaimTypes, mapping inbound 
        // claims to ASP.NET Core's ClaimTypes isn't necessary.

        oidcOptions.MapInboundClaims = false;

        // We are telling the authentication that the roles by this name will be used
        // to display the name, and for checking roles. (from https://codyanhorn.tech/blog/blazor/2020/09/06/Blazor-Server-Get-Access-Token-for-User.html)
        oidcOptions.TokenValidationParameters.NameClaimType = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Name;
        oidcOptions.TokenValidationParameters.RoleClaimType = "role";
        // ........................................................................

        // ........................................................................
        // Many OIDC providers work with the default issuer validator, but the
        // configuration must account for the issuer parameterized with "{TENANT ID}" 
        // returned by the "common" endpoint's /.well-known/openid-configuration
        // For more information, see
        // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1731

        //var microsoftIssuerValidator = AadIssuerValidator.GetAadIssuerValidator(oidcOptions.Authority);
        //oidcOptions.TokenValidationParameters.IssuerValidator = microsoftIssuerValidator.Validate;
        // ........................................................................

        // ........................................................................
        // OIDC connect options set later via ConfigureCookieOidcRefresh
        //
        // (1) The "offline_access" scope is required for the refresh token.
        //
        // (2) SaveTokens is set to true, which saves the access and refresh tokens
        // in the cookie, so the app can authenticate requests for weather data and
        // use the refresh token to obtain a new access token on access token
        // expiration.
        // ........................................................................

        ///////////////////////////////////////////////////////////////////////////
        // MINE, additional

        // RequireHttpsMetadata is false in Development else Keyclock throws an error
        // This should be true in Production
        oidcOptions.RequireHttpsMetadata = authServerSettings.RequireHttpsMetadata;

        oidcOptions.Scope.Add("TestWebApi_ClientScope");

        oidcOptions.Events = new OpenIdConnectEvents
        {
            // from https://codyanhorn.tech/blog/blazor/2020/09/06/Blazor-Server-Get-Access-Token-for-User.html
            OnTokenValidated = eventArgs =>
            {
                // We get the AccessToken from the ProtocolMessage.
                // WARNING: This might change based on what type of Authentication Provider you are using
                var accessToken = eventArgs.TokenEndpointResponse.AccessToken;
                eventArgs.Principal.AddIdentity(new ClaimsIdentity(
                    new Claim[]
                    {
                    // Make note of the claim with the name "access_token"
                    // We will use it in an Authentication Service for look up.
                    new Claim("access_token", accessToken)
                    }
                ));

                //// Here we take the accessToken and put all the claims into another
                //// Identity on the users Principal, giving us access to them when needed.
                //var jwtToken = new JwtSecurityToken(accessToken);
                //eventArgs.Principal.AddIdentity(new ClaimsIdentity(
                //    jwtToken.Claims,
                //    "jwt",
                //    eventArgs.Options.TokenValidationParameters.NameClaimType,
                //    eventArgs.Options.TokenValidationParameters.RoleClaimType
                //));

                return Task.CompletedTask;
            },
        };

    })
    //.EnableTokenAcquisitionToCallDownstreamApi()
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

// ConfigureCookieOidcRefresh attaches a cookie OnValidatePrincipal callback to get
// a new access token when the current one expires, and reissue a cookie with the
// new access token saved inside. If the refresh fails, the user will be signed
// out. OIDC connect options are set for saving tokens and the offline access
// scope.
builder.Services.ConfigureCookieOidcRefresh(CookieAuthenticationDefaults.AuthenticationScheme, MS_OIDC_SCHEME);

builder.Services.AddAuthorization();
//builder.Services.AddAuthorizationBuilder()
//    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build())
//    //.AddPolicy("AnonymousPolicy", policy => policy.RequireAssertion(_ => true))
//    ;

builder.Services.AddHttpClient();

builder.Services.AddScoped<IAuthenticationService, IdentityAuthenticationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

//app.UseCookiePolicy();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();


//static void MapKeyCloakRolesToRoleClaims(UserInformationReceivedContext context)
//{
//    if (context.Principal.Identity is not ClaimsIdentity claimsIdentity) return;

//    if (context.User.RootElement.TryGetProperty("preferred_username", out var username))
//    {
//        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, username.ToString()));
//    }

//    // TODO: more on roles???
//}

public partial class Program
{
    public const string MS_OIDC_SCHEME = "MicrosoftOidc";
}
