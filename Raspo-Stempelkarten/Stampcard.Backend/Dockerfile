# Build context: ./Raspo-Stempelkarten (see .github/workflows/ci-cd.yml)

# ─── Build ────────────────────────────────────────
FROM node:24-alpine AS build
WORKDIR /src
COPY Stampcard.Backend/package.json Stampcard.Backend/package-lock.json ./
RUN npm ci
COPY Stampcard.Backend/tsconfig.json ./
COPY Stampcard.Backend/src ./src
RUN npm run build

# ─── Runtime ──────────────────────────────────────
FROM node:24-alpine AS runtime
# NODE_ENV stays unset on purpose: it selects res/appsettings.<NODE_ENV>.json
ENV PORT=8080 \
    HOST=0.0.0.0
WORKDIR /app
COPY Stampcard.Backend/package.json Stampcard.Backend/package-lock.json ./
RUN npm ci --omit=dev && npm cache clean --force
COPY --from=build /src/dist ./dist
# config is loaded from res/appsettings.json relative to the working directory (mount over /app/res/appsettings.json)
COPY Stampcard.Backend/res ./res
USER node
EXPOSE 8080
CMD ["node", "dist/index.js"]
