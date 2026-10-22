using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Base.Helpers
{
    public static class JsonHelper
    {
        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json);
        }
        public static string ToJson<T>(T json)
        {
            //TODO: Change
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.None
            };
            return JsonConvert.SerializeObject(json, settings);
        }
    }
}
