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
            // Areas route
            Area(app, "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // Default route
            Default(app, "{controller=Home}/{action=Index}/{id?}");
        }

        // Helper methods to make route configuration more Laravel-like
        
        public static void Get(WebApplication app, string name, string pattern, 
            string controller, string action, object? defaults = null, object? constraints = null)
        {
            var routeBuilder = app.MapControllerRoute(
                name: name,
                pattern: pattern,
                defaults: MergeDefaults(controller, action, defaults)
            );

            if (constraints != null)
            {
                routeBuilder.WithDisplayName(name);
            }
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

        private static object MergeDefaults(string controller, string action, object? additionalDefaults)
        {
            var defaults = new
            {
                controller = controller,
                action = action
            };

            // If no additional defaults, just return the base defaults
            if (additionalDefaults == null)
            {
                return defaults;
            }

            // Otherwise, we'd need to merge the properties
            // For simplicity in this example, we're just returning the base defaults
            // In a real implementation, you'd use reflection to merge the objects
            return defaults;
        }
    }
}