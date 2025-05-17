# Étape 1 : Construction de l'application Angular
FROM node:18-alpine AS build

WORKDIR /app

# Copie des fichiers de dépendances
COPY package.json package-lock.json ./

# Installation des dépendances (mode production)
RUN npm ci

# Copie du reste de l'application
COPY . .

# Construction de l'application en mode production
RUN npm run build --prod

# Étape 2 : Serveur Nginx pour l'application
FROM nginx:1.25-alpine

# Copie des fichiers construits depuis l'étape de build
COPY --from=build /app/dist/my-deezer-stats-front /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
