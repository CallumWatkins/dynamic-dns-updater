using System;
using System.Diagnostics.CodeAnalysis;

namespace DynamicDNSUpdater
{
    class Config
    {
        public DDNSProvider Provider { get; set; }
        public string Password { get; set; }
        public DomainConfig[] Domains { get; set; }
        public int UpdateFrequencySeconds { get; set; } = 15*60;

        public enum DDNSProvider
        {
            Namecheap,
            Cloudflare
        }

        public class DomainConfig
        {
            public string Domain { get; set; }
            public string[] Hosts { get; set; }
        }

        public bool IsValid([NotNullWhen(false)] out string? error)
        {
            if (!Enum.IsDefined(typeof(DDNSProvider), Provider))
            {
                error = "Provider is invalid";
                return false;
            }

            if (Domains == null || Domains.Length == 0)
            {
                error = "Domains list is missing or empty";
                return false;
            }

            if (UpdateFrequencySeconds <= 0)
            {
                error = "UpdateFrequencySeconds must be greater than zero";
                return false;
            }

            error = null;
            return true;
        }
    }
}
