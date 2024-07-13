using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Runtime;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var authServerSettings = new AuthServerSettings();
builder.Configuration.GetRequiredSection(nameof(AuthServerSettings))
    .Bind(authServerSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        //Configure various JwtBearerOptions such as IDP authority here...
        options.Authority = authServerSettings.Url;
        options.Audience = authServerSettings.Audience;
        options.RequireHttpsMetadata = authServerSettings.RequireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            //ValidateIssuer = authServerSettings.ValidateIssuer, // set to true be default??
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                //context.Principal.AddIdentity(new ClaimsIdentity(
                //    new List<Claim>
                //    {
                //    new Claim(ClaimTypes.Name, "admin")
                //    }));
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        //.RequireClaim("email_verified", "true")
        .Build())
    .AddPolicy("Administrator", new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        //.RequireUserName("admin")
        .RequireClaim("name", "admin")
        .Build());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.RequireAuthorization()
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/weatherforecast_admin", () =>
{
    var forecast = Enumerable.Range(1, 15).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.RequireAuthorization(policyNames: "Administrator")
.WithName("GetWeatherForecast_Admin")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
