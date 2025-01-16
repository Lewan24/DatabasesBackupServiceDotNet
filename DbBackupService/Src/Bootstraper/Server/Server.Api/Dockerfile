FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Src/Bootstraper/Server/Server.Api/Server.Api.csproj", "Src/Bootstraper/Server/Server.Api/"]
COPY ["Src/Modules/BackupModule/Modules.Backup.Api/Modules.Backup.Api.csproj", "Src/Modules/BackupModule/Modules.Backup.Api/"]
COPY ["Src/Modules/BackupModule/Modules.Backup.Application/Modules.Backup.Application.csproj", "Src/Modules/BackupModule/Modules.Backup.Application/"]
COPY ["Src/Modules/BackupModule/Modules.Backup.Core/Modules.Backup.Core.csproj", "Src/Modules/BackupModule/Modules.Backup.Core/"]
COPY ["Src/Modules/BackupModule/Modules.Backup.Infrastructure/Modules.Backup.Infrastructure.csproj", "Src/Modules/BackupModule/Modules.Backup.Infrastructure/"]
COPY ["Src/Bootstraper/Client/Client.UI/Client.UI.csproj", "Src/Bootstraper/Client/Client.UI/"]
RUN dotnet restore "Src/Bootstraper/Server/Server.Api/Server.Api.csproj"
COPY . .
WORKDIR "/src/Src/Bootstraper/Server/Server.Api"
RUN dotnet build "Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.Api.dll"]
