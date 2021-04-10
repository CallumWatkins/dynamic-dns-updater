using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicDNSUpdater
{
    class JsonHelper
    {
        public static JsonSerializerOptions DefaultJsonSerializerOptions { get; private set; }

        static JsonHelper()
        {
            JsonSerializerOptions options = new()
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            options.Converters.Add(new IPAddressJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());

            DefaultJsonSerializerOptions = options;
        }
    }
}
