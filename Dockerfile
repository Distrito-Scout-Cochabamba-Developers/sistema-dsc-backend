# API DSC (.NET 10). Build desde el directorio sistema-dsc-backend.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY src/DSC.Domain/DSC.Domain.csproj src/DSC.Domain/
COPY src/DSC.Application/DSC.Application.csproj src/DSC.Application/
COPY src/DSC.Infrastructure/DSC.Infrastructure.csproj src/DSC.Infrastructure/
COPY src/DSC.Api/DSC.Api.csproj src/DSC.Api/

RUN dotnet restore src/DSC.Api/DSC.Api.csproj

COPY src/ src/
RUN dotnet publish src/DSC.Api/DSC.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DSC.Api.dll"]
