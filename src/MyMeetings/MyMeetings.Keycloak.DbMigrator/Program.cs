using MyMeetings.Keycloak.DbMigrator;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<KeycloakClientOptions>(
    builder.Configuration.GetSection(KeycloakClientOptions.Name));
builder.Services.AddTransient<KeycloakDataSeeder>();

var host = builder.Build();
host.Run();
