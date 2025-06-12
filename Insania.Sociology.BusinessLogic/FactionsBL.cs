using Microsoft.Extensions.Logging;

using AutoMapper;

using Insania.Shared.Models.Responses.Base;
using Insania.Sociology.Contracts.BusinessLogic;
using Insania.Sociology.Contracts.DataAccess;
using Insania.Sociology.Entities;

using ErrorMessages = Insania.Shared.Messages.ErrorMessages;
using InformationMessages = Insania.Sociology.Messages.InformationMessages;

namespace Insania.Sociology.BusinessLogic;

/// <summary>
/// Сервис работы с бизнес-логикой фракций
/// </summary>
/// <param name="logger">Сервис логгирования</param>
/// <param name="mapper">Сервис преобразования моделей</param>
/// <param name="factionsDAO">Сервис работы с данными фракций</param>
public class FactionsBL(ILogger<FactionsBL> logger, IMapper mapper, IFactionsDAO factionsDAO) : IFactionsBL
{
    #region Зависимости
    /// <summary>
    /// Сервис логгирования
    /// </summary>
    private readonly ILogger<FactionsBL> _logger = logger;

    /// <summary>
    /// Сервис преобразования моделей
    /// </summary>
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Сервис работы с данными фракций
    /// </summary>
    private readonly IFactionsDAO _factionsDAO = factionsDAO;
    #endregion

    #region Методы
    /// <summary>
    /// Метод получения списка фракций
    /// </summary>
    /// <returns cref="BaseResponseList">Стандартный ответ</returns>
    /// <remarks>Список фракций</remarks>
    /// <exception cref="Exception">Исключение</exception>
    public async Task<BaseResponseList> GetList()
    {
        try
        {
            //Логгирование
            _logger.LogInformation(InformationMessages.EnteredGetListFactionsMethod);

            //Получение данных
            List<Faction>? data = await _factionsDAO.GetList();

            //Формирование ответа
            BaseResponseList? response = null;
            if (data == null) response = new(false, null);
            else response = new(true, data?.Select(_mapper.Map<BaseResponseListItem>).ToList());

            //Возврат ответа
            return response;
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