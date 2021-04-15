using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DynamicDNSUpdater
{
    class IPFinder
    {
        private HttpClient HttpClient { get; } = new();

        public async Task<IPFinderResponse> FindAsync()
        {
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.GetAsync("https://checkip.amazonaws.com");
            }
            catch (HttpRequestException e)
            {
                return IPFinderResponse.NotFound($"Request failed: '{e.Message}'");
            }

            if (!response.StatusCode.Equals(HttpStatusCode.OK))
            {
                return IPFinderResponse.NotFound($"HTTP Status Code {response.StatusCode}");
            }

            if (IPAddress.TryParse((await response.Content.ReadAsStringAsync()).Trim(), out IPAddress? ip))
            {
                return IPFinderResponse.Found(ip);
            }
            else
            {
                return IPFinderResponse.NotFound("Response did not include a valid IP address");
            }
        }

        public struct IPFinderResponse
        {
            [MemberNotNullWhen(true, nameof(IPAddress))]
            [MemberNotNullWhen(false, nameof(ErrorMessage))]
            public bool IPFound { get; private set; }
            public IPAddress? IPAddress { get; private set; }
            public string? ErrorMessage { get; private set; }

            public static IPFinderResponse Found(IPAddress ipAddress)
            {
                return new IPFinderResponse()
                {
                    IPFound = true,
                    IPAddress = ipAddress,
                    ErrorMessage = null
                };
            }

            public static IPFinderResponse NotFound(string errorMessage)
            {
                return new IPFinderResponse()
                {
                    IPFound = false,
                    IPAddress = null,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
