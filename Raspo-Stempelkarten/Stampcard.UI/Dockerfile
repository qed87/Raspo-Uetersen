FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Als root arbeiten, um Pakete zu installieren
USER root

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        curl \
        iputils-ping \
        dnsutils \
    && rm -rf /var/lib/apt/lists/*

# Danach wieder zurück zum non-root-User wechseln (Sicherheit!)
USER $APP_UID

WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Stampcard.UI/Stampcard.UI.csproj", "Stampcard.UI/"]
RUN dotnet restore "Stampcard.UI/Stampcard.UI.csproj"
COPY . .
WORKDIR "/src/Stampcard.UI"
RUN dotnet build "./Stampcard.UI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# set by the CI from the git tags, shown in the footer; .git is not part of the build context
ARG APP_VERSION=0.0.0-local
RUN dotnet publish "./Stampcard.UI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false \
    /p:InformationalVersion=$APP_VERSION /p:IncludeSourceRevisionInInformationalVersion=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Stampcard.UI.dll"]
