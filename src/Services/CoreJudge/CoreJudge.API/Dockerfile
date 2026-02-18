FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Services/CoreJudge/CoreJudge.API/CoreJudge.API.csproj", "Services/CoreJudge/CoreJudge.API/"]
COPY ["BuildingBlocks/BuildingBlocks.Core/BuildingBlocks.Core.csproj", "BuildingBlocks/BuildingBlocks.Core/"]
COPY ["BuildingBlocks/BuildingBlocks.Identity/BuildingBlocks.Identity.csproj", "BuildingBlocks/BuildingBlocks.Identity/"]
COPY ["Services/CoreJudge/CoreJudge.Infrastructure/CoreJudge.Infrastructure.csproj", "Services/CoreJudge/CoreJudge.Infrastructure/"]
COPY ["Services/CoreJudge/CoreJudge.Application/CoreJudge.Application.csproj", "Services/CoreJudge/CoreJudge.Application/"]
COPY ["Services/CoreJudge/CoreJudge.Domain/CoreJudge.Domain.csproj", "Services/CoreJudge/CoreJudge.Domain/"]
RUN dotnet restore "Services/CoreJudge/CoreJudge.API/CoreJudge.API.csproj"
COPY . .
WORKDIR "/src/Services/CoreJudge/CoreJudge.API"
RUN dotnet build "./CoreJudge.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoreJudge.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreJudge.API.dll"]
