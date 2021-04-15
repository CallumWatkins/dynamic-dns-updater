using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace DynamicDNSUpdater
{
    class NamecheapDDNSUpdater : DDNSUpdater
    {
        public string Password { get; }

        public NamecheapDDNSUpdater(Config.DomainConfig[] domains, string password) : base(domains)
        {
            Password = password;
        }

        public override async Task<bool> UpdateAsync(System.Net.IPAddress newIPAddress)
        {
            bool success = true;
            foreach (Config.DomainConfig domainConfig in Domains)
            {
                string domain = domainConfig.Domain;
                foreach (string host in domainConfig.Hosts)
                {
                    Console.Write($" - {host}.{domain}... ");
                    string url = $"https://dynamicdns.park-your-domain.com/update?host={host}&domain={domain}&password={Password}";
                    try
                    {
                        HttpResponseMessage response = await HttpClient.GetAsync(url);
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

            XmlDocument doc = new();
            try
            {
                doc.Load(await response.Content.ReadAsStreamAsync());
                XmlNode root = doc.DocumentElement ?? throw new XmlException("Missing root node.");

                XmlNode? responseErrorsNode = root.SelectSingleNode("errors");
                if (responseErrorsNode != null)
                {
                    XmlNodeList errors = responseErrorsNode.ChildNodes;
                    if (errors.Count == 0)
                    {
                        Console.WriteLine("Failed - No error message available");
                    }
                    else
                    {
                        Console.Write("Failed - '");
                        foreach (XmlNode error in errors)
                        {
                            Console.Write($"{error.InnerText} ");
                        }
                        Console.WriteLine("'");
                    }
                    return false;
                }

                XmlNode responseIPNode = root.SelectSingleNode("IP") ?? throw new XmlException("Missing IP node.");

                if (!System.Net.IPAddress.TryParse(responseIPNode.InnerText, out System.Net.IPAddress? responseIP))
                {
                    throw new XmlException("IP node contains invalid IP address.");
                }

                if (!responseIP.Equals(newIPAddress))
                {
                    Console.WriteLine("Failed - Incorrect IP returned");
                    return false;
                }

                Console.WriteLine("Done");
                return true;
            }
            catch (XmlException)
            {
                Console.WriteLine("Failed - Response contained invalid or unexpected XML");
                return false;
            }
        }
    }
}
