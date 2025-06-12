using Microsoft.Extensions.DependencyInjection;

using Insania.Sociology.Contracts.DataAccess;

namespace Insania.Sociology.DataAccess;

/// <summary>
/// Расширение для внедрения зависимостей сервисов работы с данными в зоне социологии
/// </summary>
public static class Extension
{
    /// <summary>
    /// Метод внедрения зависимостей сервисов работы с данными в зоне социологии
    /// </summary>
    /// <param cref="IServiceCollection" name="services">Исходная коллекция сервисов</param>
    /// <returns cref="IServiceCollection">Модифицированная коллекция сервисов</returns>
    public static IServiceCollection AddSociologyDAO(this IServiceCollection services) =>
        services
            .AddScoped<IFactionsDAO, FactionsDAO>() //сервис работы с данными фракций
        ;
}