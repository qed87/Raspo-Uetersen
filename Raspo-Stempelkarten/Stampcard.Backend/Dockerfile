FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID

# Tools nachinstallieren (minimal, nur was du wirklich brauchst)
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        iputils-ping \
        dnsutils \
    && rm -rf /var/lib/apt/lists/*     # Cache sofort löschen → Image bleibt klein

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Stampcard.Backend/Stampcard.Backend.csproj", "Stampcard.Backend/"]
RUN dotnet restore "Stampcard.Backend/Stampcard.Backend.csproj"
COPY . .
WORKDIR "/src/Stampcard.Backend"
RUN dotnet build "./Stampcard.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Stampcard.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stampcard.Backend.dll"]
