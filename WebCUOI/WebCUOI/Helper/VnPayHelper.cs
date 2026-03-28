
namespace WebCUOI.Helpers // ✅ dùng số nhiều cho "Helpers"

{
    public static class VnpayHelper
    {
        public static string HmacSHA512(string key, string inputData)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(System.Text.Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputData));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
