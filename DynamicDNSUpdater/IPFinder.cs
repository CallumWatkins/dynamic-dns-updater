using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DynamicDNSUpdater
{
    class IPFinder
    {
        private HttpClient HttpClient { get; } = new();

        public async Task<IPAddress?> FindAsync()
        {
            HttpResponseMessage response = await HttpClient.GetAsync("https://checkip.amazonaws.com");

            if (!response.StatusCode.Equals(HttpStatusCode.OK)) return null;

            _ = IPAddress.TryParse((await response.Content.ReadAsStringAsync()).Trim(), out IPAddress? ip);
            return ip;
        }
    }
}
