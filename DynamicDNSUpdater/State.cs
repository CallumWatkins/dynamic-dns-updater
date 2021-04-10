using System;
using System.Net;

namespace DynamicDNSUpdater
{
    public class State
    {
        public IPAddress? CurrentIPAddress { get; set; }
        public DateTime? LastUpdatedTimestamp { get; set; }
        public IPAddress[] PreviousIPAddresses { get; set; } = Array.Empty<IPAddress>();
    }
}
