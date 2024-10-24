using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyMeetings.Administration.WebApi.MeetingGroupProposals;
using System.Reflection;

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
        options.Authority = authServerSettings.Url;
        options.Audience = authServerSettings.Audience;
        options.RequireHttpsMetadata = authServerSettings.RequireHttpsMetadata;
    });

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireRole("AdminRole")
        //.RequireAssertion(x =>
        //{
        //    var roles = x.User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(x => x.Value);
        //    return roles.Contains("AdminRole");
        //})
        .Build());

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());
    //x.AddConsumer<MeetingGroupProposedIntegrationEventConsumer>();

    // TODO: change to Transactional Outbox
    x.AddInMemoryInboxOutbox();

    x.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString("RabbitMQConnection");
        cfg.Host(host);
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<MeetingGroupProposalsService>();

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
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/administration/MeetingGroupProposals",
    async ([FromServices] MeetingGroupProposalsService service) =>
    {
        return Results.Ok(service.GetAll());
    })
    .RequireAuthorization();

app.MapPost("/administration/MeetingGroupProposals/accept/{id}",
    async (Guid id,
        [FromServices] MeetingGroupProposalsService service,
        IPublishEndpoint publishEndpoint) =>
    {
        service.Accept(id);

        await publishEndpoint.Publish(new MeetingGroupProposalAcceptedIntegrationEvent
        {
            MeetingGroupProposalId = id
        });

        return Results.Ok();
    })
    .RequireAuthorization();



app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
