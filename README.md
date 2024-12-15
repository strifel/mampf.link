# Mampf.Link
A simple Webtool to better organize Group (Food) orders.

![A screenshot of the my order page showing a persons order](https://raw.githubusercontent.com/strifel/mampf.link/refs/heads/main/wwwroot/screenshots/myorder.png)

## Workflow
Somebody creates a group and is therefore the group leader.
They can now share the link with other people who can add Orders.

The group leader can now use the tools to organize the order and finances.

There is also a feature which generates a paypal.me link with the correct amount.

## Why?
We order food quite a lot in groups. There was a lot of complaining from the people needing to 
coordinate the ordering process, that this is quite a tedious job.
This tool is here to replace the countless manuel tables and chat groups that people use to coordinate the 
order taking and finances.

## Self-Host
mampf.link is fully open source (under GPLv2) and selfhostable.

### Docker
The recommended way is docker (as I use it myself).
```yaml
services:
    mampf:
        image: ghcr.io/strifel/mampf.link:latest
        restart: always
        volumes:
            - ./data:/app/var
            - ./aspnet:/root/.aspnet
        environment:
          TZ: "Europe/Berlin" # this should be correct for your group
          MAMPFLINK_IMPRINT: "linkToYourImprint"
          MAMPFLINK_PRIVACY: "linkToYourPrivacySite"
```

After updates (and initial setup) you need to manually run the Migrations

```bash
docker compose exec mampf ./DBMigrations
```

It is strongly recommended to setup a reverse proxy and use https.
An example config for nginx is here:

```nginx
server {
    listen [::]:443;
    server_name mampf.link;
    merge_slashes off;
    
    ssl_certificate     /etc/letsencrypt/live/mampf.link/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/mampf.link/privkey.pem;

     location / { 
        proxy_pass http://ipToMampfContainer:8080;
        proxy_set_header Host $http_host;
        proxy_set_header X-Forwarded-Proto "https";
        proxy_set_header X-Forwarded-Port "443";
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 86400;
        proxy_buffering off;
    } 
}
```