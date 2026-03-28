using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace WebCUOI.Helpers
{
    public static class SessionExtensions
    {
        // 🔹 Lưu object vào Session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // 🔹 Lấy object từ Session
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
