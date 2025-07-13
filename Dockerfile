# Используем официальный образ .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем CSPROJ и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# Копируем остальные файлы
COPY . ./

# Сборка проекта
RUN dotnet publish -c Release -o /app/publish

# Финальный образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Копируем опубликованные артефакты
COPY --from=build /app/publish ./

# Копируем appsettings вручную
COPY appsettings.json ./appsettings.json
COPY appsettings.Development.json ./appsettings.Development.json

# Открываем порт
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# 👇 Выведем текущий JWT-ключ для отладки (удали после теста)
ENTRYPOINT ["sh", "-c", "echo 'JWT Key: ' $(cat appsettings.json | grep Key); dotnet JaeZooServer.dll"]
