using Vjezba.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Vjezba.Web;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

AppConfig.Initialize(builder.Configuration);

ServiceConfigurator.ConfigureServices(builder);

builder.Services.AddSignalR(
    options =>
    {
        options.MaximumReceiveMessageSize = 64 * 1024 * 1024;
        options.EnableDetailedErrors = true;
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    }
);
builder.Services.AddScoped<IGameManager, GameManager>();

builder.Services.AddAntiforgery(options => 
{
    options.SuppressXFrameOptionsHeader = true;
});

builder.Services.AddMvc(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

MiddlewareConfigurator.ConfigureMiddleware(app);

RouteConfigurator.ConfigureRoutes(app);

app.MapHub<RouletteHub>("/roulette-hub");

await DatabaseSeeder.SeedDatabase(app);

if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromFile("firebase-service-account.json")
    });
}

app.Run();