# build
FROM node:20 AS build
WORKDIR /app
COPY ./frontend/csp-web ./
RUN npm ci || npm install
RUN npm run build

# serve (static)
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
