using Microsoft.Extensions.Configuration;

namespace Vjezba.Infrastructure
{
    /// <summary>
    /// Simple static class to access application configuration values.
    /// </summary>
    public static class AppConfig
    {
        private static IConfiguration _configuration;

        // Initialize the configuration in Program.cs
        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Application settings
        public static string PublicUrl => _configuration["ApplicationSettings:PublicUrl"] ?? "https://localhost:5001";
        
        // Authentication settings
        public static string GoogleClientId => _configuration["Authentication:Google:ClientId"] ?? string.Empty;
        public static string GoogleClientSecret => _configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
        
        // Database connection strings
        public static string DbConnection => _configuration.GetConnectionString("ClientManagerDbContext") ?? string.Empty;
        
        // Custom fields
        public static string CustomField1 => _configuration["custom-field:custom-field-1"] ?? string.Empty;
        public static string CustomField2 => _configuration["custom-field:custom-field-2"] ?? string.Empty;
        
        // Environment settings
        public static string Environment => _configuration["ApplicationSettings:ASPNETCORE_ENVIRONMENT"] ?? "Development";
        
        // Add any other configuration properties you need to access
    }
}