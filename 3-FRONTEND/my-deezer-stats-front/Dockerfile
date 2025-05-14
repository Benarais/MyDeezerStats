### Stage 1: Build the Angular app
FROM node:18 AS build

WORKDIR /app/MyDeezerStats/3-FRONTEND/my-deezer-stats-front

# Copy package.json and package-lock.json
COPY package.json package-lock.json ./

# Install dependencies (including @types/jasmine)
RUN npm install

# Copy the rest of the application
COPY . .

# Build the Angular app
RUN npm run build --prod


### Stage 2: Serve the Angular app with Nginx
FROM nginx:alpine AS serve

# Copy the build output to Nginx html folder
COPY --from=build /app/MyDeezerStats/3-FRONTEND/my-deezer-stats-front/dist /usr/share/nginx/html

# Copy the default Nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
