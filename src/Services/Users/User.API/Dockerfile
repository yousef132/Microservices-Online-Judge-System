FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/Users/User.API/Users.API.csproj", "Services/Users/User.API/"]
COPY ["BuildingBlocks/BuildingBlocks.Core/BuildingBlocks.Core.csproj", "BuildingBlocks/BuildingBlocks.Core/"]
COPY ["BuildingBlocks/BuildingBlocks.Identity/BuildingBlocks.Identity.csproj", "BuildingBlocks/BuildingBlocks.Identity/"]
COPY ["BuildingBlocks/BuildingBlocks.Logging/BuildingBlocks.Logging.csproj", "BuildingBlocks/BuildingBlocks.Logging/"]
COPY ["Gateways/API.Gateway/API.Gateway.csproj", "Gateways/API.Gateway/"]
RUN dotnet restore "Services/Users/User.API/Users.API.csproj"
COPY . .
WORKDIR "/src/Services/Users/User.API"
RUN dotnet build "./Users.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Users.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Users.API.dll"]
