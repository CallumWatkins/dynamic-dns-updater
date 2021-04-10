using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicDNSUpdater
{
    class IPAddressJsonConverter : JsonConverter<IPAddress>
    {
        public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            _ = IPAddress.TryParse(reader.GetString(), out IPAddress? ip);
            return ip;
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
