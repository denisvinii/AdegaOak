# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all project files
COPY AdegaOak.Api/AdegaOak.Api.csproj AdegaOak.Api/
COPY AdegaOak.Data/AdegaOak.Data.csproj AdegaOak.Data/
COPY AdegaOak.Models/AdegaOak.Models.csproj AdegaOak.Models/
COPY AdegaOak.Services/AdegaOak.Services.csproj AdegaOak.Services/

# Restore dependencies
RUN dotnet restore AdegaOak.Api/AdegaOak.Api.csproj

# Copy source code (excludes obj/ and bin/ via .dockerignore)
COPY AdegaOak.Api/ AdegaOak.Api/
COPY AdegaOak.Data/ AdegaOak.Data/
COPY AdegaOak.Models/ AdegaOak.Models/
COPY AdegaOak.Services/ AdegaOak.Services/

# Publish
RUN dotnet publish AdegaOak.Api/AdegaOak.Api.csproj -c Release -o /app/out --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

# ASPNETCORE_URLS is set at runtime by Railway via PORT env var
# The app reads PORT directly in Program.cs

ENTRYPOINT ["dotnet", "AdegaOak.Api.dll"]
