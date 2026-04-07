# Build from this directory: docker build -t amcart-ui:latest .
FROM node:20-alpine AS build
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm install
COPY . .
ENV VITE_USER_SERVICE_URL=
# Same-origin /api so nginx can route /api/v1/products → ProductService (default /product-api is not proxied in static nginx).
ENV VITE_PRODUCT_SERVICE_URL=/api
RUN npm run build

FROM nginx:1.27-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx/nginx-main.conf /etc/nginx/nginx.conf
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
COPY nginx/snippets /etc/nginx/snippets
EXPOSE 80
