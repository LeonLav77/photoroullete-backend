using Microsoft.Extensions.Configuration;

namespace Vjezba.Infrastructure
{
    /// <summary>
    /// Simple static class to access application configuration values.
    /// </summary>
    public static class AppConfig
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string PublicUrl => _configuration["ApplicationSettings:PublicUrl"] ?? "https://localhost:5001";
        
        public static string GoogleClientId => _configuration["Authentication:Google:ClientId"] ?? string.Empty;
        public static string GoogleClientSecret => _configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
        
        public static string DbConnection => _configuration.GetConnectionString("ClientManagerDbContext") ?? string.Empty;
        
        public static string CustomField1 => _configuration["custom-field:custom-field-1"] ?? string.Empty;
        public static string CustomField2 => _configuration["custom-field:custom-field-2"] ?? string.Empty;
        
        public static string Environment => _configuration["ApplicationSettings:ASPNETCORE_ENVIRONMENT"] ?? "Development";
        
    }
}