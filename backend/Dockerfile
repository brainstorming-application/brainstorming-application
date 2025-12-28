# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY BrainstormingApp.Core/BrainstormingApp.Core.csproj BrainstormingApp.Core/
COPY BrainstormingApp.Application/BrainstormingApp.Application.csproj BrainstormingApp.Application/
COPY BrainstormingApp.Infrastructure/BrainstormingApp.Infrastructure.csproj BrainstormingApp.Infrastructure/
COPY BrainstormingApp.API/BrainstormingApp.API.csproj BrainstormingApp.API/

RUN dotnet restore BrainstormingApp.API/BrainstormingApp.API.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish BrainstormingApp.API/BrainstormingApp.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Expose port
EXPOSE 5081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5081
ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "BrainstormingApp.API.dll"]
