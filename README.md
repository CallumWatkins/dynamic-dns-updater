# Dynamic DNS Updater
A simple DDNS updater.

## Supported Providers
 - Namecheap

## Configuration
| Option | Description | Required | Default Value |
| --- | --- | --- | --- |
| `Provider` | The name of the DDNS provider. | Yes | - |
| `Password` | The password used to authenticate to the provider. | Yes | - |
| `Domains` | An array of domains along with the hosts to be updated. | Yes | - |
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
[callumwatkins/dynamicdnsupdater](https://hub.docker.com/r/callumwatkins/dynamicdnsupdater)

Configuration and state stored in `/data`.

### Example `docker-compose.yml` file for ARM32 devices:

```yml
version: "2.1"
services:
  ddns:
    image: callumwatkins/dynamicdnsupdater:arm32
    container_name: ddns
    network_mode: host
    volumes:
      - /home/user/ddns/data:/data
    restart: unless-stopped
```

## How It Works
Periodically checks https://checkip.amazonaws.com to see if the public IP address of the device has changed. If a change is detected, the DNS record for each host is updated.