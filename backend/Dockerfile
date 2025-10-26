# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ./ChatBackend ./ChatBackend
WORKDIR /src/ChatBackend

RUN dotnet restore ChatBackend.csproj
RUN dotnet publish ChatBackend.csproj -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# ✅ DB yazılabilir klasör
RUN mkdir -p /app/data

COPY --from=build /app/publish .

# ✅ Render için doğru port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ChatBackend.dll"]
