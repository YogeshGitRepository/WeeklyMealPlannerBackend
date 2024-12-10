using System.Security.Cryptography;

namespace MealPlannerBackend.Utilities
{
    public static class JwtSecretGenerator
    {
        public static string GenerateJwtSecret()
        {
            var key = new byte[32]; 
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }
    }
}
