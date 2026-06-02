# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore (improves layer caching)
COPY SmartWorkspaceManager.API/SmartWorkspaceManager.API.csproj ./SmartWorkspaceManager.API/
COPY SmartWorkspaceManager.Application/SmartWorkspaceManager.Application.csproj ./SmartWorkspaceManager.Application/
COPY SmartWorkspaceManager.Domain/SmartWorkspaceManager.Domain.csproj ./SmartWorkspaceManager.Domain/
COPY SmartWorkspaceManager.Infrastructure/SmartWorkspaceManager.Infrastructure.csproj ./SmartWorkspaceManager.Infrastructure/
COPY SmartWorkspaceManager.Persistence/SmartWorkspaceManager.Persistence.csproj ./SmartWorkspaceManager.Persistence/

# Copy the rest of the API project and publish
RUN dotnet restore ./SmartWorkspaceManager.API/SmartWorkspaceManager.API.csproj

COPY . .

WORKDIR /src/SmartWorkspaceManager.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SmartWorkspaceManager.API.dll"]