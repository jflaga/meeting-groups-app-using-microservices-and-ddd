using System.Security.Claims;

namespace MyMeetings.Meetings.IntegrationTests._Utils;

internal class AutoAuthorizeMiddleware(RequestDelegate rd)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var identity = new ClaimsIdentity("Bearer");

        //identity.AddClaim(new Claim("sub", "1234567"));
        //identity.AddClaim(new Claim(ClaimTypes.Name, "test-name"));

        httpContext.User.AddIdentity(identity);
        await rd.Invoke(httpContext);
    }
}