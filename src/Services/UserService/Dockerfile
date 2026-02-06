# Build from repo root so Directory.Packages.props and Directory.Build.props are available:
#   docker build -f src/Services/UserService/Dockerfile -t user-service .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy entire Services folder so Directory.Packages.props and Directory.Build.props are found
COPY src/Services ./src/Services
WORKDIR /app/src/Services

RUN dotnet restore UserService/UserService.csproj && \
    dotnet publish UserService/UserService.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "UserService.dll"]