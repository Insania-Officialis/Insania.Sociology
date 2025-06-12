using Insania.Shared.Models.Responses.Base;

namespace Insania.Sociology.Contracts.BusinessLogic;

/// <summary>
/// Интерфейс работы с бизнес-логикой фракций
/// </summary>
public interface IFactionsBL
{
    /// <summary>
    /// Метод получения списка фракций
    /// </summary>
    /// <returns cref="BaseResponseList">Стандартный ответ</returns>
    /// <remarks>Список фракций</remarks>
    /// <exception cref="Exception">Исключение</exception>
    Task<BaseResponseList> GetList();
}