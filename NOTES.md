# Meeting Groups app using Microservices and DDD


# Some resources

## Keycloak, Authentication

Keycloak DB Migrator reference: [eShopOnAbp's `EShopOnAbp.Keycloak.DbMigrator`](https://github.com/abpframework/eShopOnAbp/tree/327fbcc341fd7b5bb7dfa223593d3df2a7721c89/shared/EShopOnAbp.Keycloak.DbMigrator)

[A comprehensive overview of authentication in ASP.NET Core – for fellow developers who're struggling with authentication in .NET](https://www.reddit.com/r/dotnet/comments/we9qx8/a_comprehensive_overview_of_authentication_in/)

[use the JWT Bearer authentication scheme directly in ASP.NET Web API](https://stackoverflow.com/a/67556318/1451757)

[how the world of authentication works (Angular, ASP.NET Core Web API, Keycloak, Swagger)](https://stackoverflow.com/a/77104803/1451757)

[The audience aud claim in a JWT is meant to refer to the Resource Servers that should accept the token.](https://stackoverflow.com/a/28503265/1451757)

[How To Configure Audience In Keycloak](https://dev.to/metacosmos/how-to-configure-audience-in-keycloak-kp4)

[Generate Keycloak Bearer Token Using Postman](https://czetsuya.medium.com/generate-keycloak-bearer-token-using-postman-5bd81d7d1f8)

["Audience support" from Keycloak's documentation](https://www.keycloak.org/docs/latest/server_admin/#audience-support)

[Blazor Server Application with Keycloak Authentication](https://github.com/csinisa/blazor_server_keycloak/commit/4a20c0e7155feaf549d271e8ee56aaca9bf22bb9)

 - This app's logout was working, but mine was not working because of error "Missing parameters: id_token_hint". Error disappears if I use the ["Blazor Web App with OpenID Connect (OIDC)" sample fron dotnet](https://github.com/dotnet/blazor-samples/tree/main/8.0/BlazorWebAppOidc)

[Blazor Samples from dotnet GitHub: Blazor Web App with OpenID Connect (OIDC)](https://github.com/dotnet/blazor-samples/tree/main/8.0/BlazorWebAppOidc)


## Messaging with MassTransit and RabbitMQ

[Using MassTransit with RabbitMQ in ASP.NET Core](https://code-maze.com/masstransit-rabbitmq-aspnetcore/)

[.NET Aspire Messaging With RabbitMQ & MassTransit](https://fiyaz-hasan-me-blog.azurewebsites.net/aspire-messaging-with-rabbitmq-and-masstransit/)


## Integration Testing

"Integration Testing for ASP.NET APIs" by Erik Dahl

 - [Part 1: Basics](https://knowyourtoolset.com/2024/01/integration-testing/)
 - [Part 2: Data (TestContainers)](https://knowyourtoolset.com/2024/01/integration-testing-data/)
 - [Part 3: Auth](https://knowyourtoolset.com/2024/01/integration-testing-auth/)

["Your Essential Guide to xUnit Lifecycle"](https://www.youtube.com/watch?v=lMKkKx68xHg)

 - IAsyncLifetime, BeforeAfterTestAtrribute, Fixture, IClassFixture, ICollectionFixture

["How to Test RabbitMQ with Testcontainers in .NET"](https://www.youtube.com/watch?v=DMs3ZuakHGA)

["MassTransit Testing with Web Application Factory"](https://www.youtube.com/watch?v=Uzme7vInDz0) by Chris Patterson

### On the value of integration tests

> "In my experience, writing "integration" tests in ASP.NET Core are for controllers is far more valuable than trying to unit test them, and is easier than ever in ASP.NET Core"
> --- [Andrew Lock](https://andrewlock.net/should-you-unit-test-controllers-in-aspnetcore/)

> "Writing integration (or functional) tests on a C# API gives more confidence in the code that is written, in addition, it increases the productivity during all stages of development."
> --- [Tim Deschryver](https://timdeschryver.dev/blog/why-writing-integration-tests-on-a-csharp-api-is-a-productivity-booster) 

> "In my opinion, one good integration test is worth 1,000 unit tests."
> ---  [Khalid Abuhakmeh](https://khalidabuhakmeh.com/secrets-of-a-dotnet-professional#integration-tests--unit-tests)
    