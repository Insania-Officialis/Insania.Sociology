using Microsoft.Extensions.DependencyInjection;

using Insania.Sociology.Contracts.BusinessLogic;
using Insania.Sociology.DataAccess;

namespace Insania.Sociology.BusinessLogic;

/// <summary>
/// Расширение для внедрения зависимостей сервисов работы с бизнес-логикой в зоне социологии
/// </summary>
public static class Extension
{
    /// <summary>
    /// Метод внедрения зависимостей сервисов работы с бизнес-логикой в зоне социологии
    /// </summary>
    /// <param cref="IServiceCollection" name="services">Исходная коллекция сервисов</param>
    /// <returns cref="IServiceCollection">Модифицированная коллекция сервисов</returns>
    public static IServiceCollection AddSociologyBL(this IServiceCollection services) =>
        services
            .AddSociologyDAO() //сервисы работы с данными в зоне социологии
            .AddScoped<IFactionsBL, FactionsBL>() //сервис работы с бизнес-логикой фракций
        ;
}