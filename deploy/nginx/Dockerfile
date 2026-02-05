FROM nginx:alpine

# Replace default nginx.conf with our config (upstream, rate limit, server_tokens off)
COPY nginx.conf /etc/nginx/nginx.conf

# Server block(s) and reverse proxy config
COPY conf.d/ /etc/nginx/conf.d/

# Security headers and optional SSL snippet
COPY snippets/ /etc/nginx/snippets/

EXPOSE 80
# Expose 443 when enabling SSL
# EXPOSE 443

CMD ["nginx", "-g", "daemon off;"]
