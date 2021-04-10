using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DynamicDNSUpdater
{
    internal class StateReaderWriter : IDisposable
    {
        private FileStream StateFileStream { get; }

        public StateReaderWriter(string path)
        {
            StateFileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        }

        public async Task<State> ReadAsync()
        {
            StreamReader sr = new(StateFileStream, Encoding.UTF8, leaveOpen: true);
            string json = await sr.ReadToEndAsync();
            sr.Dispose();
            StateFileStream.Position = 0;
            try
            {
                return JsonSerializer.Deserialize<State>(json, JsonHelper.DefaultJsonSerializerOptions) ?? throw new Exception("State is null");
            }
            catch (JsonException)
            {
                return new State();
            }
        }

        public async Task WriteAsync(State c)
        {
            string json = JsonSerializer.Serialize(c, JsonHelper.DefaultJsonSerializerOptions);
            StateFileStream.SetLength(0);
            StreamWriter sw = new(StateFileStream, Encoding.UTF8, leaveOpen: true);
            await sw.WriteAsync(json);
            await sw.DisposeAsync();
        }

        public void Dispose()
        {
            StateFileStream.Flush();
            ((IDisposable)StateFileStream).Dispose();
        }

        ~StateReaderWriter()
        {
            Dispose();
        }
    }
}
