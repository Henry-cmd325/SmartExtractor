# --- Etapa 1: Build ---
FROM node:22-alpine AS build

WORKDIR /app

# Copiamos archivos de dependencias primero
COPY package*.json ./

# Instalamos dependencias (clean install para asegurar consistencia)
RUN npm install

# Copiamos el resto del código
COPY . .

# Construimos la aplicación para producción
RUN npm run build

# --- Etapa 2: Runtime ---
FROM node:22-alpine AS final
WORKDIR /app

# Variables para que Nuxt escuche en todas las interfaces dentro de Docker
ENV NODE_ENV=production
ENV NUXT_HOST=0.0.0.0
ENV NUXT_PORT=3000

# Copiamos solo lo necesario desde la etapa de build
COPY --from=build /app/.output ./.output

EXPOSE 3000

# Ejecutamos el servidor de Nitro (el motor de Nuxt 3)
CMD ["node", ".output/server/index.mjs"]