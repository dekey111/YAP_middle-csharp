FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["YAP_middle-csharp_Auth/YAP_middle-csharp_Auth/YAP_middle-csharp_Auth.csproj", "YAP_middle-csharp_Auth/YAP_middle-csharp_Auth/"]
COPY ["YAP_middle-csharp_Auth/YAP_middle-csharp_Auth.Domain/YAP_middle-csharp_Auth.Domain.csproj", "YAP_middle-csharp_Auth/YAP_middle-csharp_Auth.Domain/"]
COPY ["YAP_middle-csharp_Auth/YAP_middle-csharp_Auth.Infrastructure/YAP_middle-csharp_Auth.Infrastructure.csproj", "YAP_middle-csharp_Auth/YAP_middle-csharp_Auth.Infrastructure/"]
COPY ["YAP_middle-csharp_Auth/YAP_middle-csharp_Auth.Application/YAP_middle-csharp_Auth.Application.csproj", "YAP_middle-csharp_Auth/YAP_middle-csharp_Auth_Application/"]

RUN dotnet restore "YAP_middle-csharp_Auth/YAP_middle-csharp_Auth/YAP_middle-csharp_Auth.csproj"

COPY . .
WORKDIR "/src/YAP_middle-csharp_Auth/YAP_middle-csharp_Auth"
RUN dotnet build "YAP_middle-csharp_Auth.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "YAP_middle-csharp_Auth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YAP_middle-csharp_Auth.dll"]