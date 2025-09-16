using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AppSage.Infrastructure.AI
{
    public class Utility
    {

        public  static string GenerateCacheKey(string prompt, AIQueryConfig queryConfig)
        {
            var cacheInput = new
            {
                Prompt = prompt,
                QueryConfig = queryConfig,
            };

            var json = JsonSerializer.Serialize(cacheInput);
            
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
