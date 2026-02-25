# 1. Используем официальный .NET SDK для сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 2. Копируем файлы проекта и восстанавливаем зависимости
COPY *.csproj ./
RUN dotnet restore

# 3. Копируем весь проект и собираем его
COPY . ./
RUN dotnet publish -c Release -o out

# 4. Используем легковесный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# 5. Порт приложения
EXPOSE 8080

# 6. Запуск приложения
ENTRYPOINT ["dotnet", "ConferenceApi.dll"]