using Vjezba.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Vjezba.Web;

var builder = WebApplication.CreateBuilder(args);

AppConfig.Initialize(builder.Configuration);

ServiceConfigurator.ConfigureServices(builder);

builder.Services.AddSignalR(
    options =>
    {
           options.MaximumReceiveMessageSize = 64 * 1024 * 1024; // 10MB
    }
);

var app = builder.Build();

// Configure middleware pipeline
MiddlewareConfigurator.ConfigureMiddleware(app);

// Configure routes
RouteConfigurator.ConfigureRoutes(app);

// Map the SignalR hub
app.MapHub<RouletteHub>("/roulette-hub");

// Seed database
await DatabaseSeeder.SeedDatabase(app);

app.Run();