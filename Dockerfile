FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VioletManager.API/VioletManager.API.csproj", "VioletManager.API/"]
COPY ["VioletManager.Application/VioletManager.Application.csproj", "VioletManager.Application/"]
COPY ["VioletManager.Domain/VioletManager.Domain.csproj", "VioletManager.Domain/"]
RUN dotnet restore "VioletManager.API/VioletManager.API.csproj"
COPY . .
WORKDIR "/src/VioletManager.API"
RUN dotnet build "./VioletManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VioletManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VioletManager.API.dll"]
