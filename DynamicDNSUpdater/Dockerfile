#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim-arm32v7 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["DynamicDNSUpdater/DynamicDNSUpdater.csproj", "DynamicDNSUpdater/"]
RUN dotnet restore -r linux-arm "DynamicDNSUpdater/DynamicDNSUpdater.csproj"
COPY . .
WORKDIR "/src/DynamicDNSUpdater"
RUN dotnet build "DynamicDNSUpdater.csproj" -c DockerRelease -r linux-arm -o /app/build

FROM build AS publish
RUN dotnet publish "DynamicDNSUpdater.csproj" -c DockerRelease -r linux-arm -o /app/publish

FROM base AS final
WORKDIR /app
VOLUME /data
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DynamicDNSUpdater.dll"]