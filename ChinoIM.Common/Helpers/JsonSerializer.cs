using ChinoIM.Common.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ChinoIM.Common.Helpers
{
    public class JsonSerializer
    {
        private static ILogger logger = LogManager.CreateLogger<JsonSerializer>();

        public static string Serialize(object obj)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj);
                return json;
            }
            catch
            {
                logger.LogError("Serialize error");
                return "";
            }
        }

        public static T Deserialize<T>(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
            catch
            {
                logger.LogError("Deserialize error: '{0}'", json);
                return default(T);
            }
        }
    }
}
