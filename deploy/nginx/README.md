# AmCart Nginx Reverse Proxy

This directory contains the nginx configuration used as the **reverse proxy** for AmCart: it receives all traffic and proxies to the login-app (and later API services). It also adds security headers and rate limiting.

## Layout

- **nginx.conf** – Main config: upstream `login_app`, rate limit zone, `server_tokens off`, includes `conf.d/*.conf`.
- **conf.d/default.conf** – Server block: listens on 80, `proxy_pass` to login-app, includes security snippet, applies rate limit.
- **snippets/security.conf** – Security headers (X-Frame-Options, X-Content-Type-Options, etc.).
- **snippets/ssl.conf** – Commented SSL settings; uncomment and set cert paths when enabling HTTPS.
- **Dockerfile** – Builds an image that copies these files and runs nginx.

## Build and run

From the **repository root** (so nginx can reach the login-app service):

```bash
docker compose up --build
```

This starts both `nginx` (reverse proxy) and `login-app`. Nginx listens on host port 80 and forwards `/` to the login-app container.

To build only the nginx image:

```bash
docker build -t amcart-nginx deploy/nginx
```

## Enabling SSL (HTTPS)

1. Obtain certificates (e.g. Let's Encrypt, or self-signed for dev). Place `cert.pem` and `key.pem` on the host or in a volume.
2. In **snippets/ssl.conf**: uncomment the directives and set `ssl_certificate` and `ssl_certificate_key` to the paths inside the container (e.g. `/etc/nginx/ssl/cert.pem`). Mount the certs into that path when running the container.
3. In **conf.d/default.conf**: add a `listen 443 ssl;` server block (or duplicate the server block for 443), and add `include /etc/nginx/snippets/ssl.conf;`.
4. In **snippets/security.conf**: uncomment the HSTS header.
5. In the **Dockerfile**: uncomment `EXPOSE 443`.
6. In **docker-compose.yml**: expose `443:443` for the nginx service, and add a volume mount for the certificate directory.

Do not bake certificate files into the image; mount them at runtime.

## Environments

The same config is intended for local, staging, and production. For production you typically:

- Mount real SSL certificates.
- Set `server_name` in `conf.d/default.conf` to your domain (or use an env-substituted config).
- Optionally add a separate `conf.d/prod.conf` or override only the server name.

## Rate limiting

The default zone `one` allows 10 requests per second per IP, with a burst of 20. To change: edit `limit_req_zone` in **nginx.conf** and `limit_req zone=one burst=...` in **conf.d/default.conf**.
