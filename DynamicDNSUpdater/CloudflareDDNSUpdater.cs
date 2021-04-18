using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace DynamicDNSUpdater
{
    class CloudflareDDNSUpdater : DDNSUpdater
    {
        public string Password { get; }

        public CloudflareDDNSUpdater(Config.DomainConfig[] domains, string password) : base(domains)
        {
            Password = password;
        }

        public override async Task<bool> UpdateAsync(System.Net.IPAddress newIPAddress)
        {
            bool success = true;
            foreach (Config.DomainConfig domainConfig in Domains)
            {
                string zoneId = domainConfig.Domain;
                foreach (string recordId in domainConfig.Hosts)
                {
                    Console.Write($" - Zone:{zoneId} Record:{recordId}... ");
                    string url = $"https://api.cloudflare.com/client/v4/zones/{zoneId}/dns_records/{recordId}";
                    string content = JsonSerializer.Serialize(new { content = newIPAddress.ToString() }, JsonHelper.DefaultJsonSerializerOptions);

                    HttpRequestMessage request = new()
                    {
                        Method = HttpMethod.Patch,
                        RequestUri = new Uri(url),
                        Content = new StringContent(content, Encoding.UTF8, "application/json")
                    };

                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Password);

                    try
                    {
                        HttpResponseMessage response = await HttpClient.SendAsync(request);
                        success &= await ValidateResponse(response, newIPAddress);
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine($"Failed - Request failed: '{e.Message}'");
                        success = false;
                    }
                }
            }

            return success;
        }

        private static async Task<bool> ValidateResponse(HttpResponseMessage response, System.Net.IPAddress newIPAddress)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Failed - HTTP status {response.StatusCode}");
                Console.WriteLine("Response content: ");
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                Console.WriteLine();
                return false;
            }

            try
            {
                JsonDocument? doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

                if (doc == null)
                {
                    throw new Exception("Response content missing");
                }

                var root = doc.RootElement;
                bool success = root.GetProperty("success").GetBoolean();

                if (!success)
                {
                    Console.WriteLine($"Failed - Errors:");

                    JsonElement.ArrayEnumerator errors = root.GetProperty("errors").EnumerateArray();
                    foreach (JsonElement error in errors)
                    {
                        int errorCode = error.GetProperty("code").GetInt32();
                        string? errorMessage = error.GetProperty("message").GetString();

                        Console.WriteLine($"{errorCode}: {errorMessage ?? ""}");
                    }
                    Console.WriteLine();
                    return false;
                }

                string? resultContent = root.GetProperty("result").GetProperty("content").GetString();

                if (!System.Net.IPAddress.TryParse(resultContent, out System.Net.IPAddress? responseIP))
                {
                    throw new Exception("IP result contains invalid IP address.");
                }

                if (!newIPAddress.Equals(responseIP))
                {
                    throw new Exception("Incorrect IP returned");
                }

                Console.WriteLine("Done");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed - {e.Message}");
                return false;
            }
        }
    }
}
