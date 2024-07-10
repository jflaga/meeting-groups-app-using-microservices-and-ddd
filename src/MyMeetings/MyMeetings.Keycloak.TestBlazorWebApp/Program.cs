using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MyMeetings.Keycloak.TestBlazorWebApp.Components;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorPages().WithRazorPagesRoot("/Components/Pages"); // for login/logout

var authServerSettings = new AuthServerSettings();
builder.Configuration.GetRequiredSection(nameof(AuthServerSettings))
    .Bind(authServerSettings);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
//.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
//{
//    //Configure some cookie options like expiration and security 
//})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, oidcOptions =>
{
    //Configure series of OIDC options like flow, authority, etc

    oidcOptions.Authority = authServerSettings.Authority;

    oidcOptions.ClientId = authServerSettings.ClientId;
    oidcOptions.ClientSecret = authServerSettings.ClientSecret;
    oidcOptions.RequireHttpsMetadata = authServerSettings.RequireHttpsMetadata;
    oidcOptions.ResponseType = OpenIdConnectResponseType.Code;

    oidcOptions.GetClaimsFromUserInfoEndpoint = true;
    oidcOptions.SaveTokens = true;
    oidcOptions.MapInboundClaims = true;

    //oidcOptions.CallbackPath = new PathString("/signin-oidc");
    //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
    //oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");
    //oidcOptions.SignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;

    //options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    //options.TokenValidationParameters.RoleClaimType = "role";

    //oidcOptions.Scope.Clear();
    oidcOptions.Scope.Add("openid");
    oidcOptions.Scope.Add("profile");
    oidcOptions.Scope.Add("email");
    oidcOptions.Scope.Add("roles");

    oidcOptions.Events = new OpenIdConnectEvents
    {
        OnUserInformationReceived = context =>
        {
            MapKeyCloakRolesToRoleClaims(context);
            return Task.CompletedTask;
        },
    };
});

builder.Services.AddAuthorization();
//builder.Services.AddAuthorizationBuilder()
//    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .Build())
//    //.AddPolicy("AnonymousPolicy", policy => policy.RequireAssertion(_ => true))
//    ;

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

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();


static void MapKeyCloakRolesToRoleClaims(UserInformationReceivedContext context)
{
    if (context.Principal.Identity is not ClaimsIdentity claimsIdentity) return;

    if (context.User.RootElement.TryGetProperty("preferred_username", out var username))
    {
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, username.ToString()));
    }

    // TODO: more on roles???
}