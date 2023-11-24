# Dynamic DNS Updater
A simple DDNS updater.

## Supported Providers
 - Namecheap
 - Cloudflare

## Configuration
| Option | Description | Required | Default Value |
| --- | --- | --- | --- |
| `Provider` | The name of the DDNS provider. | Yes | - |
| `Password` | The password used to authenticate to the provider.<br />- Namecheap: The [Dynamic DNS Password](https://www.namecheap.com/support/knowledgebase/article.aspx/595/11/how-do-i-enable-dynamic-dns-for-a-domain/) for the domain.<br />- Cloudflare: An [API Token](https://dash.cloudflare.com/profile/api-tokens) with `DNS:Edit` permissions in all relevant zones. | Yes | - |
| `Domains` | An array of domains along with the hosts to be updated.<br />- Namecheap: `Domain` is the domain name and `Hosts` is an array of hosts.<br />- Cloudflare: `Domain` is the Zone ID and `Hosts` is an array of Record IDs. | Yes | - |
| `UpdateFrequencySeconds` | The number of seconds between each check of the current IP address. | No | 900 (15 minutes) |

### Example `DDNS_config.json` configuration file (created on first launch):
```json
{
  "Provider": "Namecheap",
  "Password": "your-ddns-password-here",
  "Domains": [
    {
      "Domain": "example.com",
      "Hosts": [ "@", "www" ]
    }
  ],
  "UpdateFrequencySeconds": 900
}
```

## Docker Usage
[callumwatkins/dynamicdnsupdater](https://hub.docker.com/r/callumwatkins/dynamicdnsupdater) (supported architectures: `amd64`, `arm32v7`, `arm64v8`)

Configuration and state stored in `/data`.

### Example `docker-compose.yml` file for ARM32 devices:

```yml
version: "2.1"
services:
  ddns:
    image: callumwatkins/dynamicdnsupdater:arm32v7-latest
    container_name: ddns
    network_mode: host
    volumes:
      - /home/user/ddns/data:/data
    restart: unless-stopped
```

## How It Works
Periodically checks https://checkip.amazonaws.com to see if the public IP address of the device has changed. If a change is detected, the DNS record for each host is updated.