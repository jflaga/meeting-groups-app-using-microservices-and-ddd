using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Mvc;
using MyMeetings.Meetings.IntegrationEvents;
using MyMeetings.Meetings.WebApi.MeetingGroupProposals;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
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

app.MapPost("/meetings/MeetingGroupProposals",
    async ([FromBody] MeetingGroupProposal proposal, 
        [FromServices] MeetingGroupProposalsService service,
        IPublishEndpoint publishEndpoint) =>
    {
        service.Add(proposal);

        await publishEndpoint.Publish(new MeetingGroupProposedIntegrationEvent
        {
            MeetingGroupProposalId = proposal.Id,
            Name = proposal.Name,
            LocationCity = proposal.LocationCity,
            LocationCountryCode = proposal.LocationCountryCode,
            ProposalUserId = proposal.ProposalUserId,
            ProposalDate = proposal.ProposalDate,
        });
    });

app.MapGet("/meetings/MeetingGroupProposals",
    ([FromServices] MeetingGroupProposalsService service) =>
    {
        return service.GetAll();
    });

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
