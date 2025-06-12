#Использование образа 9 dotnet
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

#Задание рабочего каталога
WORKDIR /app

#Проброс порта
EXPOSE 8082

#Использование образа sdk 9 dotnet
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

#Создание переменной конфигурации сборки
ARG BUILD_CONFIGURATION=Release

#Задание рабочего каталога
WORKDIR /src

#Копирование файлов в контейнер
COPY ["Insania.Sociology", ""]

#Восстановление проекта
RUN dotnet restore "./Insania.Sociology.ApiRead/Insania.Sociology.ApiRead.csproj"

#Задание рабочего каталога
WORKDIR /src/Insania.Sociology.ApiRead

#Сборка проекта
RUN dotnet build "./Insania.Sociology.ApiRead.csproj" -c $BUILD_CONFIGURATION -o /app/build

#Использование образа сборки для публикации
FROM build AS publish

#Создание переменной конфигурации сборки
ARG BUILD_CONFIGURATION=Release

#Публикация проекта
RUN dotnet publish "./Insania.Sociology.ApiRead.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

#Использование базового образа для финальных действий
FROM base AS final

#Задание рабочего каталога
WORKDIR /app

#Копирование файлов публикации
COPY --from=publish /app/publish .

#Запуск приложения
ENTRYPOINT ["dotnet", "Insania.Sociology.ApiRead.dll"]