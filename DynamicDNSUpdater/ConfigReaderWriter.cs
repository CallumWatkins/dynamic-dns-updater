using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DynamicDNSUpdater
{
    class ConfigReaderWriter
    {
        public string Path { get; }

        public ConfigReaderWriter(string path)
        {
            Path = path;
        }

        public async Task<Config?> ReadAsync()
        {
            if (!File.Exists(Path)) return null;

            string json = await File.ReadAllTextAsync(Path, Encoding.UTF8);
            return JsonSerializer.Deserialize<Config>(json, JsonHelper.DefaultJsonSerializerOptions) ?? throw new System.Exception("Config is null");
        }

        public async Task WriteAsync(Config cfg)
        {
            string json = JsonSerializer.Serialize(cfg, JsonHelper.DefaultJsonSerializerOptions);
            await File.WriteAllTextAsync(Path, json, Encoding.UTF8);
        }
    }
}
