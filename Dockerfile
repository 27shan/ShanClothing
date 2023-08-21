# Используем базовый образ с .NET 6 SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

# Устанавливаем рабочую директорию
WORKDIR /app

# Используйте образ с .NET SDK для сборки приложения
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Копируем файлы проектов и восстанавливаем зависимости
COPY ShanClothing/*.csproj ./ShanClothing/
COPY ShanClothing.DAL/*.csproj ./ShanClothing.DAL/
COPY ShanClothing.Domain/*.csproj ./ShanClothing.Domain/
COPY ShanClothing.Service/*.csproj ./ShanClothing.Service/

RUN dotnet restore ShanClothing/ShanClothing.csproj
RUN dotnet restore ShanClothing.DAL/ShanClothing.DAL.csproj
RUN dotnet restore ShanClothing.Domain/ShanClothing.Domain.csproj
RUN dotnet restore ShanClothing.Service/ShanClothing.Service.csproj

# Копируем остальные файлы и билдим приложение
COPY . ./
RUN dotnet publish ShanClothing/ShanClothing.csproj -c Release -o out

# Используем образ ASP.NET для развертывания
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/ShanClothing/out .

# Указываем порт, который будет слушать приложение
EXPOSE 80
ENTRYPOINT ["dotnet", "ShanClothing.dll"]