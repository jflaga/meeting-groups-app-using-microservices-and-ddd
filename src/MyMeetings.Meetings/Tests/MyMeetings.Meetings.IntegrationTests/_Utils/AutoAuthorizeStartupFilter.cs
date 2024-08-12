namespace MyMeetings.Meetings.IntegrationTests._Utils;

// from https://knowyourtoolset.com/2024/01/integration-testing-auth/
internal class AutoAuthorizeStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            builder.UseMiddleware<AutoAuthorizeMiddleware>();
            next(builder);
        };
    }
}