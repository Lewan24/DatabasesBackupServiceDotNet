# Zobacz https://aka.ms/customizecontainer, aby dowiedzieć się, jak dostosować kontener debugowania i jak program Visual Studio używa tego pliku Dockerfile do kompilowania obrazów w celu szybszego debugowania.

# Ten etap jest używany podczas uruchamiania z programu VS w trybie szybkim (domyślnie dla konfiguracji debugowania)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Ten etap służy do kompilowania projektu usługi
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bootstraper/Server/Server.Api/Server.Api.csproj", "Bootstraper/Server/Server.Api/"]
COPY ["Modules/BackupModule/Modules.Backup.Api/Modules.Backup.Api.csproj", "Modules/BackupModule/Modules.Backup.Api/"]
COPY ["Modules/BackupModule/Modules.Backup.Application/Modules.Backup.Application.csproj", "Modules/BackupModule/Modules.Backup.Application/"]
COPY ["Modules/BackupModule/Modules.Backup.Core/Modules.Backup.Core.csproj", "Modules/BackupModule/Modules.Backup.Core/"]
COPY ["Bootstraper/Client/Client.UI/Client.UI.csproj", "Bootstraper/Client/Client.UI/"]
RUN dotnet restore "./Bootstraper/Server/Server.Api/Server.Api.csproj"
COPY . .
WORKDIR "/src/Bootstraper/Server/Server.Api"
RUN dotnet build "./Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Ten etap służy do publikowania projektu usługi do skopiowania do etapu końcowego
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Ten etap jest używany w środowisku produkcyjnym lub w przypadku uruchamiania z programu VS w trybie regularnym (domyślnie, gdy nie jest używana konfiguracja debugowania)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.Api.dll"]