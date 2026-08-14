FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Контракты
COPY ["YAP_middle_csharp.Contracts/YAP_middle_csharp.Contracts/YAP_middle_csharp.Contracts.csproj", "YAP_middle_csharp.Contracts/YAP_middle_csharp.Contracts/"]

# 2. Проекты Events
COPY ["YAP_middle-csharp_Events/YAP_middle-csharp_Events/YAP_middle-csharp_Events.csproj", "YAP_middle-csharp_Events/YAP_middle-csharp_Events/"]
COPY ["YAP_middle-csharp_Events/YAP_middle-csharp_Events.Application/YAP_middle-csharp_Events.Application.csproj", "YAP_middle-csharp_Events/YAP_middle-csharp_Events.Application/"]
COPY ["YAP_middle-csharp_Events/YAP_middle-csharp_Events.Domain/YAP_middle-csharp_Events.Domain.csproj", "YAP_middle-csharp_Events/YAP_middle-csharp_Events.Domain/"]
COPY ["YAP_middle-csharp_Events/YAP_middle-csharp_Events.Infrastructure/YAP_middle-csharp_Events.Infrastructure.csproj", "YAP_middle-csharp_Events/YAP_middle-csharp_Events.Infrastructure/"]

RUN dotnet restore "YAP_middle-csharp_Events/YAP_middle-csharp_Events/YAP_middle-csharp_Events.csproj"

COPY . .
WORKDIR "/src/YAP_middle-csharp_Events/YAP_middle-csharp_Events"
RUN dotnet build "YAP_middle-csharp_Events.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "YAP_middle-csharp_Events.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YAP_middle-csharp_Events.dll"]