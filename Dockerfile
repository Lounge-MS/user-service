FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .

RUN dotnet publish src/GrpcUserService/GrpcUserService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Копирование конфигурационных файлов
COPY src/GrpcUserService/appsettings*.json ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "GrpcUserService.dll"]

