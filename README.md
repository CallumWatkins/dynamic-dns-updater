# Dynamic DNS Updater
A simple DDNS updater.

## Supported Providers
 - Namecheap

## Configuration
An example configuration file is created on first launch.

## Docker Usage
[callumwatkins/dynamicdnsupdater](https://hub.docker.com/r/callumwatkins/dynamicdnsupdater)

Configuration and state stored in `/data`.

Example `docker-compose.yml` file for ARM32 devices:

```
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