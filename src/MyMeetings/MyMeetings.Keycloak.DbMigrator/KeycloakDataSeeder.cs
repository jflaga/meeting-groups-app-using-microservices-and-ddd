using Keycloak.Net;
using Keycloak.Net.Models.Clients;
using Keycloak.Net.Models.ClientScopes;
using Keycloak.Net.Models.ProtocolMappers;
using Keycloak.Net.Models.Roles;
using Keycloak.Net.Models.Users;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMeetings.Keycloak.DbMigrator;
public class KeycloakDataSeeder
{
    private readonly KeycloakClient keycloakClient;
    private readonly KeycloakClientOptions keycloakOptions;
    private readonly IConfiguration configuration;
    private readonly ILogger<KeycloakDataSeeder> logger;

    public KeycloakDataSeeder(IOptions<KeycloakClientOptions> keycloakClientOptions,
        IConfiguration configuration, ILogger<KeycloakDataSeeder> logger)
    {
        keycloakOptions = keycloakClientOptions.Value;

        keycloakClient = new KeycloakClient(
            keycloakOptions.Url,
            keycloakOptions.AdminUserName,
            keycloakOptions.AdminPassword
        );

        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task SeedAsync()
    {
        //await CreateRelam(); // Import realm not working.
        // Manually create a new realm before running this seeder.

        await UpdateRealmSettingsAsync();
        await UpdateAdminUserAsync();
        await CreateUserJohnDoeAsync();
        await CreateRoleMapperAsync(); // roles scope

        await CreateClientScopesAsync();
        await CreateClientsAsync();
    }

    //private async Task CreateRelam()
    //{
    //    try
    //    {
    //        var masterRealm = await keycloakClient.GetRealmAsync("master");
    //        var isImportSuccessful = await keycloakClient.ImportRealmAsync(keycloakOptions.RealmName, masterRealm);
    //        if (!isImportSuccessful)
    //        {
    //            throw new Exception(
    //                "Creation of new realm failed.");
    //        }
    //    }
    //    catch (Exception ex)
    //    {

    //        throw;
    //    }
    //}

    private async Task UpdateRealmSettingsAsync()
    {
        var myMeetingsRealm = await keycloakClient.GetRealmAsync(keycloakOptions.RealmName);
        if (myMeetingsRealm.AccessTokenLifespan != 30 * 60)
        {
            myMeetingsRealm.AccessTokenLifespan = 30 * 60;
            await keycloakClient.UpdateRealmAsync(keycloakOptions.RealmName, myMeetingsRealm);
        }
    }

    private async Task UpdateAdminUserAsync()
    {
        var users = await keycloakClient.GetUsersAsync(keycloakOptions.RealmName, username: "admin");
        var adminUser = users.FirstOrDefault();
        if (adminUser == null)
        {
            throw new Exception(
                "Keycloak admin user is not provided, check if KEYCLOAK_ADMIN environment variable is passed properly.");
        }

        if (string.IsNullOrEmpty(adminUser.Email))
        {
            adminUser.Email = "admin@example.com";
            adminUser.FirstName = "admin";
            adminUser.EmailVerified = true;

            logger.LogInformation("Updating admin user with email and first name...");
            await keycloakClient.UpdateUserAsync(keycloakOptions.RealmName, adminUser.Id, adminUser);
        }

        var roles = (await keycloakClient.GetRolesAsync(keycloakOptions.RealmName)).ToList();

        if (!roles.Any(x => x.Name == "AdminRole"))
        {
            var adminRole = new Role() { Name = "AdminRole" };            
            await keycloakClient.CreateRoleAsync(keycloakOptions.RealmName,
                new Role() { Name = "AdminRole" });
        }

        await keycloakClient.AddRealmRoleMappingsToUserAsync(keycloakOptions.RealmName,
            adminUser.Id, roles);
    }

    private async Task CreateUserJohnDoeAsync()
    {
        var userJohnDoeExists = (await keycloakClient.GetUsersAsync(keycloakOptions.RealmName))
            .Any(x => x.Email == "johndoe@example.com");
        if (!userJohnDoeExists)
        {
            var user = new User()
            {
                UserName = "johndoe",
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Enabled = true,
                Credentials = [new() { Type = "password", Value = "johndoe", Temporary = false}]
            };
            await keycloakClient.CreateUserAsync(keycloakOptions.RealmName, user);

            //try
            //{
            //    var userId = await keycloakClient.CreateAndRetrieveUserIdAsync(keycloakOptions.RealmName, user);
            //    var johnDoe = await keycloakClient.GetUserAsync(keycloakOptions.RealmName, userId);
            //    //johnDoe.
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}
        }
    }

    private async Task CreateRoleMapperAsync()
    {
        var roleScope = (await keycloakClient.GetClientScopesAsync(keycloakOptions.RealmName))
            .FirstOrDefault(q => q.Name == "roles");
        if (roleScope == null)
            return;

        if (!roleScope.ProtocolMappers.Any(q => q.Name == "roles"))
        {
            await keycloakClient.CreateProtocolMapperAsync(keycloakOptions.RealmName, roleScope.Id,
                new ProtocolMapper()
                {
                    Name = "roles",
                    Protocol = "openid-connect",
                    _ProtocolMapper = "oidc-usermodel-realm-role-mapper",
                    Config = new Dictionary<string, string>()
                    {
                        { "access.token.claim", "true" },
                        { "id.token.claim", "true" },
                        { "claim.name", "role" },
                        { "multivalued", "true" },
                        { "userinfo.token.claim", "true" },
                    }
                });
        }
    }

    private async Task CreateClientScopesAsync()
    {
        await CreateScopeAsync("TestWebApi_ClientScope");
        //await CreateScopeAsync("SampleWebApiService_Alice");
        //await CreateScopeAsync("SampleWebApiService_Bob");
        await CreateScopeAsync("MeetingsModuleWebApi_ClientScope");
        await CreateScopeAsync("AdministrationModuleWebApi_ClientScope");
    }

    private async Task CreateClientsAsync()
    {
        await CreatePostmanClientAsync();
        await CreateTestBlazorWebAppClientAsync();
        await CreateMeetingGroupsClientBlazorWebAppClientAsync();
        await CreateMeetingGroupsAdminBlazorWebAppClientAsync();
    }

    private async Task CreateScopeAsync(string scopeName)
    {
        var scope = (await keycloakClient.GetClientScopesAsync(keycloakOptions.RealmName))
            .FirstOrDefault(q => q.Name == scopeName);

        if (scope == null)
        {
            scope = new ClientScope
            {
                Name = scopeName,
                Description = scopeName + " scope",
                Protocol = "openid-connect",
                Attributes = new Attributes
                {
                    ConsentScreenText = scopeName,
                    DisplayOnConsentScreen = "true",
                    IncludeInTokenScope = "true"
                },
                ProtocolMappers = new List<ProtocolMapper>()
                {
                    new ProtocolMapper()
                    {
                        Name = scopeName,
                        Protocol = "openid-connect",
                        _ProtocolMapper = "oidc-audience-mapper",
                        Config =
                            new
                                Dictionary<string, string>
                                {
                                    { "id.token.claim", "false" },
                                    { "access.token.claim", "true" },
                                    { "included.custom.audience", $"{scopeName}_Audience" }
                                }
                    }
                }
            };

            await keycloakClient.CreateClientScopeAsync(keycloakOptions.RealmName, scope);
        }
    }

    private async Task CreatePostmanClientAsync()
    {
        const string PostmanClientId = "PostmanClient";

        var postmanClient = (await keycloakClient
            .GetClientsAsync(keycloakOptions.RealmName, clientId: PostmanClientId))
            .FirstOrDefault();

        if (postmanClient == null)
        {
            postmanClient = new Client
            {
                ClientId = PostmanClientId,
                Name = "Postman Client",
                Protocol = "openid-connect",
                Enabled = true,
                RedirectUris = new List<string>
                {
                    "https://oauth.pstmn.io/v1/callback"
                },
                FrontChannelLogout = true,
                PublicClient = true,
            };

            await keycloakClient.CreateClientAsync(keycloakOptions.RealmName, postmanClient);

        }

        // It should be optional because Alice should be able to decide the intended recipient(s) of the access token by specifying different scopes.
        // from https://dev.to/metacosmos/how-to-configure-audience-in-keycloak-kp4
        await AddOptionalClientScopesAsync(PostmanClientId, new List<string> {
            "TestWebApi_ClientScope",
            "MeetingsModuleWebApi_ClientScope",
            "AdministrationModuleWebApi_ClientScope"
        });

        // override: make postman client a confidential client instead of a public client
        if (postmanClient.PublicClient ?? true)
        {
            postmanClient.PublicClient = false;
            postmanClient.Secret = configuration["Clients:Postman:Secret"];

            await keycloakClient.UpdateClientAsync(keycloakOptions.RealmName, postmanClient.Id, postmanClient);
        }
    }

    private async Task CreateTestBlazorWebAppClientAsync()
    {
        const string clientAppName = "TestBlazorWebApp";
        const string clientId = $"{clientAppName}_Client";

        var client = (await keycloakClient
            .GetClientsAsync(keycloakOptions.RealmName, clientId: clientId))
            .FirstOrDefault();

        var rootUrl = $"{configuration[$"Clients:{clientAppName}:RootUrl"]}";
        if (client == null)
        {
            client = new Client
            {
                ClientId = clientId,
                Name = $"{clientAppName} Client",
                Protocol = "openid-connect",
                Enabled = true,
                RedirectUris = new List<string>
                {
                    $"{rootUrl.TrimEnd('/')}/signin-oidc"
                },
                FrontChannelLogout = true,
                PublicClient = false,
                Secret = configuration[$"Clients:{clientAppName}:Secret"],
            };

            client.Attributes = new Dictionary<string, object>
            {
                { "post.logout.redirect.uris", $"{rootUrl.TrimEnd('/')}/signout-callback-oidc" }
            };

            await keycloakClient.CreateClientAsync(keycloakOptions.RealmName, client);

            // It should be optional because Alice should be able to decide the intended recipient(s) of the access token by specifying different scopes.
            // from https://dev.to/metacosmos/how-to-configure-audience-in-keycloak-kp4
            await AddOptionalClientScopesAsync(clientId, new List<string> { 
                "TestWebApi_ClientScope" 
            });
        }

        var noPostLogoutRedirectUrisYet = !client.Attributes.Any(x => x.Key == "post.logout.redirect.uris");
        if (noPostLogoutRedirectUrisYet)
        {
            client.Attributes.Add("post.logout.redirect.uris", $"{rootUrl.TrimEnd('/')}/signout-callback-oidc");
            await keycloakClient.UpdateClientAsync(keycloakOptions.RealmName, client.Id, client);
        }
    }

    private async Task CreateMeetingGroupsClientBlazorWebAppClientAsync()
    {
        const string clientAppName = "MeetingGroupsClient_BlazorWebApp";
        const string clientId = $"{clientAppName}_Client";

        var client = (await keycloakClient
            .GetClientsAsync(keycloakOptions.RealmName, clientId: clientId))
            .FirstOrDefault();

        var rootUrl = $"{configuration[$"Clients:{clientAppName}:RootUrl"]}";
        if (client == null)
        {
            client = new Client
            {
                ClientId = clientId,
                Name = $"{clientAppName} Client",
                Protocol = "openid-connect",
                Enabled = true,
                RedirectUris = new List<string>
                {
                    $"{rootUrl.TrimEnd('/')}/signin-oidc"
                },
                FrontChannelLogout = true,
                PublicClient = false,
                Secret = configuration[$"Clients:{clientAppName}:Secret"],
            };

            client.Attributes = new Dictionary<string, object>
            {
                { "post.logout.redirect.uris", $"{rootUrl.TrimEnd('/')}/signout-callback-oidc" }
            };

            await keycloakClient.CreateClientAsync(keycloakOptions.RealmName, client);

            // It should be optional because Alice should be able to decide the intended recipient(s) of the access token by specifying different scopes.
            // from https://dev.to/metacosmos/how-to-configure-audience-in-keycloak-kp4
            await AddOptionalClientScopesAsync(clientId, new List<string> {
                "MeetingsModuleWebApi_ClientScope"
            });
        }

        var noPostLogoutRedirectUrisYet = !client.Attributes.Any(x => x.Key == "post.logout.redirect.uris");
        if (noPostLogoutRedirectUrisYet)
        {
            client.Attributes.Add("post.logout.redirect.uris", $"{rootUrl.TrimEnd('/')}/signout-callback-oidc");
            await keycloakClient.UpdateClientAsync(keycloakOptions.RealmName, client.Id, client);
        }
    }

    private async Task CreateMeetingGroupsAdminBlazorWebAppClientAsync()
    {
        const string clientAppName = "MeetingGroupsAdmin_BlazorWebApp";
        const string clientId = $"{clientAppName}_Client";

        var client = (await keycloakClient
            .GetClientsAsync(keycloakOptions.RealmName, clientId: clientId))
            .FirstOrDefault();

        var rootUrl = $"{configuration[$"Clients:{clientAppName}:RootUrl"]}";
        if (client == null)
        {
            client = new Client
            {
                ClientId = clientId,
                Name = $"{clientAppName} Client",
                Protocol = "openid-connect",
                Enabled = true,
                RedirectUris = new List<string>
                {
                    $"{rootUrl.TrimEnd('/')}/signin-oidc"
                },
                FrontChannelLogout = true,
                PublicClient = false,
                Secret = configuration[$"Clients:{clientAppName}:Secret"],
            };

            client.Attributes = new Dictionary<string, object>
            {
                { "post.logout.redirect.uris", $"{rootUrl.TrimEnd('/')}/signout-callback-oidc" }
            };

            await keycloakClient.CreateClientAsync(keycloakOptions.RealmName, client);

            // It should be optional because Alice should be able to decide the intended recipient(s) of the access token by specifying different scopes.
            // from https://dev.to/metacosmos/how-to-configure-audience-in-keycloak-kp4
            await AddOptionalClientScopesAsync(clientId, new List<string> {
                "AdministrationModuleWebApi_ClientScope"
            });
        }

        var noPostLogoutRedirectUrisYet = !client.Attributes.Any(x => x.Key == "post.logout.redirect.uris");
        if (noPostLogoutRedirectUrisYet)
        {
            client.Attributes.Add("post.logout.redirect.uris", $"{rootUrl.TrimEnd('/')}/signout-callback-oidc");
            await keycloakClient.UpdateClientAsync(keycloakOptions.RealmName, client.Id, client);
        }
    }

    private async Task AddOptionalClientScopesAsync(string clientName, List<string> scopes)
    {
        var client = (await keycloakClient.GetClientsAsync(keycloakOptions.RealmName, clientId: clientName))
            .FirstOrDefault();
        if (client == null)
        {
            logger.LogError($"Couldn't find {clientName}! Could not seed optional scopes!");
            return;
        }

        var clientOptionalScopes =
            (await keycloakClient.GetOptionalClientScopesAsync(keycloakOptions.RealmName, client.Id)).ToList();

        var clientScopes = (await keycloakClient.GetClientScopesAsync(keycloakOptions.RealmName)).ToList();

        foreach (var scope in scopes)
        {
            if (!clientOptionalScopes.Any(q => q.Name == scope))
            {
                var serviceScope = clientScopes.First(q => q.Name == scope);
                logger.LogInformation($"Seeding {scope} scope to {clientName}.");
                await keycloakClient.UpdateOptionalClientScopeAsync(keycloakOptions.RealmName, client.Id,
                    serviceScope.Id);
            }
        }
    }
}
