using System.Text.Json;

namespace Vjezba.Model.Helpers
{
    public class DebugHelper
    {
        public static void DD(object obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }
    }
}
