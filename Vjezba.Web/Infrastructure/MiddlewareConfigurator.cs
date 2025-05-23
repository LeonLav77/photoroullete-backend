using Microsoft.AspNetCore.HttpOverrides;

namespace Vjezba.Infrastructure
{
    public static class MiddlewareConfigurator
    {
        public static void ConfigureMiddleware(WebApplication app)
        {
            // Forwarded Headers
            ConfigureForwardedHeaders(app);
            
            // Cookie Policy
            ConfigureCookiePolicy(app);
            
            // Environment-specific middleware
            ConfigureEnvironmentSpecific(app);
            
            // Standard middleware pipeline
            ConfigureStandardMiddleware(app);
        }

        private static void ConfigureForwardedHeaders(WebApplication app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }

        private static void ConfigureCookiePolicy(WebApplication app)
        {
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
                Secure = CookieSecurePolicy.Always
            });
        }

        private static void ConfigureEnvironmentSpecific(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
        }

        private static void ConfigureStandardMiddleware(WebApplication app)
        {
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseRequestLocalization();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}