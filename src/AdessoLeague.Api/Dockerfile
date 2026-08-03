# Build context is the repository root, not this folder: the build needs the central
# Directory.*.props files and every referenced project.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/AdessoLeague.Domain/AdessoLeague.Domain.csproj src/AdessoLeague.Domain/
COPY src/AdessoLeague.Application/AdessoLeague.Application.csproj src/AdessoLeague.Application/
COPY src/AdessoLeague.Infrastructure/AdessoLeague.Infrastructure.csproj src/AdessoLeague.Infrastructure/
COPY src/AdessoLeague.Api/AdessoLeague.Api.csproj src/AdessoLeague.Api/
RUN dotnet restore src/AdessoLeague.Api/AdessoLeague.Api.csproj

COPY .editorconfig ./
COPY src/ src/
RUN dotnet publish src/AdessoLeague.Api/AdessoLeague.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# The alpine image ships without ICU and runs in globalization-invariant mode; the league data is
# Turkish, so full culture support is installed instead.
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish ./

EXPOSE 8080
USER $APP_UID

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "AdessoLeague.Api.dll"]
