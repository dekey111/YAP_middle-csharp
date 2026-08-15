FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["YAP_middle_csharp.Contracts/YAP_middle_csharp.Contracts/YAP_middle_csharp.Contracts.csproj", "YAP_middle_csharp.Contracts/YAP_middle_csharp.Contracts/"]

COPY ["YAP_middle-csharp_Booking/YAP_middle-csharp_Booking/YAP_middle-csharp_Booking.csproj", "YAP_middle-csharp_Booking/YAP_middle-csharp_Booking/"]
COPY ["YAP_middle-csharp_Booking/YAP_middle_csharp_Booking.Application/YAP_middle_csharp_Booking.Application.csproj", "YAP_middle-csharp_Booking/YAP_middle_csharp_Booking.Application/"]
COPY ["YAP_middle-csharp_Booking/YAP_middle_csharp_Booking.Domain/YAP_middle_csharp_Booking.Domain.csproj", "YAP_middle-csharp_Booking/YAP_middle_csharp_Booking.Domain/"]
COPY ["YAP_middle-csharp_Booking/YAP_middle_csharp_Booking.Infrastructure/YAP_middle_csharp_Booking.Infrastructure.csproj", "YAP_middle-csharp_Booking/YAP_middle_csharp_Booking.Infrastructure/"]

RUN dotnet restore "YAP_middle-csharp_Booking/YAP_middle-csharp_Booking/YAP_middle-csharp_Booking.csproj"

COPY . .
WORKDIR "/src/YAP_middle-csharp_Booking/YAP_middle-csharp_Booking"
RUN dotnet build "YAP_middle-csharp_Booking.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "YAP_middle-csharp_Booking.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YAP_middle-csharp_Booking.dll"]