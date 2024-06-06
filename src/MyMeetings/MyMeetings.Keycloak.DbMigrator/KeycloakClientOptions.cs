using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMeetings.Keycloak.DbMigrator;
public class KeycloakClientOptions
{
    public const string Name = "Keycloak";

    public string Url { get; set; }
    public string AdminUserName { get; set; }
    public string AdminPassword { get; set; }
    public string RealmName { get; set; }
}
