# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy backend files
COPY backend/ChatBackend ./backend/ChatBackend

# Restore dependencies and publish
WORKDIR /src/backend/ChatBackend
RUN dotnet restore ChatBackend.csproj
RUN dotnet publish ChatBackend.csproj -c Release -o /app/publish

# Stage 2: Serve application
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ChatBackend.dll"]
