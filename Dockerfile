# syntax=docker/dockerfile:1

# ---- Stage 1: build the React SPA ----
FROM node:22-alpine AS frontend
WORKDIR /frontend
COPY src/frontend/guess43-web/package.json src/frontend/guess43-web/package-lock.json ./
RUN npm ci
COPY src/frontend/guess43-web/ ./
RUN npm run build

# ---- Stage 2: build & publish the ASP.NET Core API ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src
COPY Directory.Build.props .editorconfig ./
COPY src/backend/Guess43.Domain/Guess43.Domain.csproj src/backend/Guess43.Domain/
COPY src/backend/Guess43.Application/Guess43.Application.csproj src/backend/Guess43.Application/
COPY src/backend/Guess43.Infrastructure/Guess43.Infrastructure.csproj src/backend/Guess43.Infrastructure/
COPY src/backend/Guess43.Api/Guess43.Api.csproj src/backend/Guess43.Api/
RUN dotnet restore src/backend/Guess43.Api/Guess43.Api.csproj
COPY src/backend/ src/backend/
RUN dotnet publish src/backend/Guess43.Api/Guess43.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Stage 3: runtime image serving SPA + API from one origin ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=backend /app/publish ./
COPY --from=frontend /frontend/dist ./wwwroot
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --retries=5 \
    CMD curl -fsS http://localhost:8080/health/live || exit 1
USER $APP_UID
ENTRYPOINT ["dotnet", "Guess43.Api.dll"]
