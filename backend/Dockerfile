# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["BrainstormingApp.API/BrainstormingApp.API.csproj", "BrainstormingApp.API/"]
COPY ["BrainstormingApp.Application/BrainstormingApp.Application.csproj", "BrainstormingApp.Application/"]
COPY ["BrainstormingApp.Core/BrainstormingApp.Core.csproj", "BrainstormingApp.Core/"]
COPY ["BrainstormingApp.Infrastructure/BrainstormingApp.Infrastructure.csproj", "BrainstormingApp.Infrastructure/"]

RUN dotnet restore "BrainstormingApp.API/BrainstormingApp.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/BrainstormingApp.API"
RUN dotnet build "BrainstormingApp.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BrainstormingApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Expose port
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BrainstormingApp.API.dll"]
