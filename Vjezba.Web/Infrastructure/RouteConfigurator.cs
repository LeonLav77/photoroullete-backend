namespace Vjezba.Infrastructure
{
    public static class RouteConfigurator
    {
        public static void ConfigureRoutes(WebApplication app)
        {
            // Configure Razor Pages routes
            app.MapRazorPages();

            // Configure MVC routes using the Router
            Router.MapRoutes(app);
        }
    }

    public static class Router
    {
        public static void MapRoutes(WebApplication app)
        {
            // Game API routes with constraints
            GameApiRoutes(app);

            // Areas route
            Area(app, "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Default route
            Default(app, "{controller=Home}/{action=Index}/{id?}");
        }

        private static void GameApiRoutes(WebApplication app)
        {
            app.MapControllerRoute(
                name: "game_import",
                pattern: "api/game/import/{gameCode:regex(^[a-zA-Z0-9_-]+$)}",
                defaults: new { controller = "GameApi", action = "ImportGame" }
            );
        }

        public static void Area(WebApplication app, string pattern)
        {
            app.MapControllerRoute(
                name: "areas",
                pattern: pattern
            );
        }

        public static void Default(WebApplication app, string pattern)
        {
            app.MapControllerRoute(
                name: "default",
                pattern: pattern
            );
        }

    }
}