FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["OrbitalGreenhouse.Api/OrbitalGreenhouse.Api.csproj", "OrbitalGreenhouse.Api/"]
RUN dotnet restore "OrbitalGreenhouse.Api/OrbitalGreenhouse.Api.csproj"
COPY . .
WORKDIR /src/OrbitalGreenhouse.Api
RUN dotnet publish "OrbitalGreenhouse.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "OrbitalGreenhouse.Api.dll"]
