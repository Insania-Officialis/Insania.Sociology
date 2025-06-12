using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Insania.Sociology.Contracts.DataAccess;
using Insania.Sociology.Database.Contexts;
using Insania.Sociology.Entities;
using Insania.Sociology.Messages;

using ErrorMessages = Insania.Shared.Messages.ErrorMessages;

namespace Insania.Sociology.DataAccess;

/// <summary>
/// Сервис работы с данными фракций
/// </summary>
/// <param cref="ILogger{FactionsDAO}" name="logger">Сервис логгирования</param>
/// <param cref="SociologyContext" name="context">Контекст базы данных социологии</param>
public class FactionsDAO(ILogger<FactionsDAO> logger, SociologyContext context) : IFactionsDAO
{
    #region Зависимости
    /// <summary>
    /// Сервис логгирования
    /// </summary>
    private readonly ILogger<FactionsDAO> _logger = logger;

    /// <summary>
    /// Контекст базы данных социологии
    /// </summary>
    private readonly SociologyContext _context = context;
    #endregion

    #region Методы
    /// <summary>
    /// Метод получения списка фракций
    /// </summary>
    /// <returns cref="List{Faction}">Список фракций</returns>
    /// <exception cref="Exception">Исключение</exception>
    public async Task<List<Faction>> GetList()
    {
        try
        {
            //Логгирование
            _logger.LogInformation(InformationMessages.EnteredGetListFactionsMethod);

            //Получение данных из бд
            List<Faction> data = await _context.Factions.Where(x => x.DateDeleted == null).ToListAsync();

            //Возврат результата
            return data;
        }
        catch (Exception ex)
        {
            //Логгирование
            _logger.LogError("{text}: {error}", ErrorMessages.Error, ex.Message);

            //Проброс исключения
            throw;
        }
    }
    #endregion
}