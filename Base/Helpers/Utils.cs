using System.Security.Cryptography;

namespace Base.Helpers
{
    public static class Utils
    {
        public static string GenerateUrlSafeRandomKey(int byteLength = 32)
        {
            if (byteLength <= 0) throw new ArgumentOutOfRangeException(nameof(byteLength), "byteLength must be greater than 0.");

            var bytes = RandomNumberGenerator.GetBytes(byteLength);

            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}
