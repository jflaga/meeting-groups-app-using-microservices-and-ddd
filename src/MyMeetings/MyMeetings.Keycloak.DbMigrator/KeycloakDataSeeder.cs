using Keycloak.Net;
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

    public KeycloakDataSeeder(IOptions<KeycloakClientOptions> keycloakClientOptions)
    {
        keycloakOptions = keycloakClientOptions.Value;

        keycloakClient = new KeycloakClient(
            keycloakOptions.Url,
            keycloakOptions.AdminUserName,
            keycloakOptions.AdminPassword
        );
    }

    public async Task SeedAsync()
    {
        await UpdateRealmSettingsAsync();
    }

    private async Task UpdateRealmSettingsAsync()
    {
        var masterRealm = await keycloakClient.GetRealmAsync(keycloakOptions.RealmName);
        if (masterRealm.AccessTokenLifespan != 30 * 60)
        {
            masterRealm.AccessTokenLifespan = 30 * 60;
            await keycloakClient.UpdateRealmAsync(keycloakOptions.RealmName, masterRealm);
        }
    }
}
