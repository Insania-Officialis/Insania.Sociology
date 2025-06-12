using Microsoft.AspNetCore.Mvc;

using Insania.Shared.Messages;
using Insania.Shared.Models.Responses.Base;

using Insania.Sociology.Contracts.BusinessLogic;

namespace Insania.Sociology.ApiRead.Controllers;

/// <summary>
/// Контроллер работы с фракциями
/// </summary>
/// <param name="logger">Сервис логгирования</param>
/// <param name="factionsService">Сервис работы с бизнес-логикой фракций</param>
[Route("factions")]
public class FactionsController(ILogger<FactionsController> logger, IFactionsBL factionsService) : Controller
{
    #region Зависимости
    /// <summary>
    /// Сервис логгирования
    /// </summary>
    private readonly ILogger<FactionsController> _logger = logger;

    /// <summary>
    /// Сервис работы с бизнес-логикой фракций
    /// </summary>
    private readonly IFactionsBL _factionsService = factionsService;
    #endregion

    #region Методы
    /// <summary>
    /// Метод получения списка фракций
    /// </summary>
    /// <returns cref="OkResult">Список фракций</returns>
    /// <returns cref="BadRequestResult">Ошибка</returns>
    [HttpGet]
    [Route("list")]
    public async Task<IActionResult> GetList()
    {
        try
        {
            //Получение результата
            BaseResponse? result = await _factionsService.GetList();

            //Возврат ответа
            return Ok(result);
        }
        catch (Exception ex)
        {
            //Логгирование
            _logger.LogError("{text} {ex}", ErrorMessages.Error, ex);

            //Возврат ошибки
            return BadRequest(new BaseResponseError(ex.Message));
        }
    }
    #endregion
}