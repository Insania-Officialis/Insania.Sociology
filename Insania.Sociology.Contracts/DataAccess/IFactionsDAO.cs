using Insania.Sociology.Entities;

namespace Insania.Sociology.Contracts.DataAccess;

/// <summary>
/// Интерфейс работы с данными фракций
/// </summary>
public interface IFactionsDAO
{
    /// <summary>
    /// Метод получения списка фракций
    /// </summary>
    /// <returns cref="List{Faction}">Список фракций</returns>
    /// <exception cref="Exception">Исключение</exception>
    Task<List<Faction>> GetList();
}