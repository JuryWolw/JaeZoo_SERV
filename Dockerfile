# --- Базовый рантайм ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# --- Сборка приложения ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "JaeZooServer.csproj"
RUN dotnet build "JaeZooServer.csproj" -c Release -o /app/build
RUN dotnet publish "JaeZooServer.csproj" -c Release -o /app/publish

# --- Финальный рантайм ---
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JaeZooServer.dll"]
