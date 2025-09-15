# ============= BUILD STAGE =============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# kopiuj sln i projekty osobno (lepsza cache warstwa)
COPY OctoBackup.sln .
COPY Src/Bootstraper/Server/Server.Api/Server.Api.csproj Src/Bootstraper/Server/Server.Api/
# tu ewentualnie reszta projektów *.csproj jeśli są zależności

RUN dotnet restore "OctoBackup.sln"

# kopiuj resztę źródeł
COPY . .

WORKDIR /src/Src/Bootstraper/Server/Server.Api

RUN dotnet publish "Server.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false


# ============= RUNTIME STAGE =============
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# instalacja narzędzi: mysqldump, pg_dump, sqlcmd
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        mysql-client \
        postgresql-client \
        curl gnupg2 apt-transport-https && \
    curl -sSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl -sSL https://packages.microsoft.com/config/debian/12/prod.list \
        > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y --no-install-recommends \
        msodbcsql18 mssql-tools18 && \
    echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# usunięcie symboli debug itp.
RUN find . -name "*.pdb" -delete && \
    find . -name "*.xml" -delete

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Server.Api.dll"]
