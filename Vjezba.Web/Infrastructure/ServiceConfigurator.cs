using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Vjezba.DAL;
using Vjezba.Model;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Vjezba.Infrastructure
{
    public static class ServiceConfigurator
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            // MVC and Razor
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
            builder.Services.AddRazorPages();
            
            // Database
            ConfigureDatabase(builder);
            
            // Email
            builder.Services.AddTransient<IEmailSender, NoOpEmailSender>();
            
            // Identity
            ConfigureIdentity(builder);
            
            // Authentication
            ConfigureAuthentication(builder);
            
            // Localization
            ConfigureLocalization(builder);
            
            // CORS
            ConfigureCors(builder);
        }

        private static void ConfigureDatabase(WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<ClientManagerDbContext>(options =>
                options.UseSqlite(
                    AppConfig.DbConnection,
                    opt => opt.MigrationsAssembly("Vjezba.DAL")));
        }

        private static void ConfigureIdentity(WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ClientManagerDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
        }

        private static void ConfigureAuthentication(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = AppConfig.GoogleClientId;
                    options.ClientSecret = AppConfig.GoogleClientSecret;
                    options.CallbackPath = "/signin-google";

                    options.Events = new OAuthEvents
                    {
                        OnRedirectToAuthorizationEndpoint = context =>
                        {
                            // Get the PublicUrl from AppConfig
                            var publicUrl = AppConfig.PublicUrl;

                            var uri = new Uri(context.RedirectUri);
                            var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                            queryString.Set("redirect_uri", $"{publicUrl}/signin-google");

                            var uriBuilder = new UriBuilder(uri)
                            {
                                Query = queryString.ToString()
                            };

                            context.Response.Redirect(uriBuilder.ToString());
                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            context.Response.Redirect("/Home/Error?message=" + context.Failure?.Message);
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        private static void ConfigureLocalization(WebApplicationBuilder builder)
        {
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("hr-HR")
                };

                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
        }

        private static void ConfigureCors(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
            });
        }
    }
}