using System.Net.Http;
using System.Threading.Tasks;

namespace DynamicDNSUpdater
{
    abstract class DDNSUpdater
    {
        public Config.DomainConfig[] Domains { get; }
        protected HttpClient HttpClient { get; } = new();

        public DDNSUpdater(Config.DomainConfig[] domains)
        {
            Domains = domains;
        }

        public abstract Task<bool> UpdateAsync(System.Net.IPAddress newIPAddress);
    }
}
