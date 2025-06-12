using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Insania.Shared.Entities;

namespace Insania.Sociology.Entities;

/// <summary>
/// Модель сущности лога сервиса социологии
/// </summary>
[Table("r_logs_api_sociology")]
[Comment("Логи сервиса социологии")]
public class LogApiSociology : Log
{
    #region Конструкторы
    /// <summary>
    /// Простой конструктор модели сущности лога сервиса социологии
    /// </summary>
    public LogApiSociology() : base()
    {

    }

    /// <summary>
    /// Конструктор модели сущности лога сервиса социологии без идентификатора
    /// </summary>
    /// <param cref="string" name="username">Логин пользователя, выполняющего действие</param>
    /// <param cref="bool" name="isSystem">Признак системной записи</param>
    /// <param cref="string" name="method">Наименование вызываемого метода</param>
    /// <param cref="string" name="type">Тип вызываемого метода</param>
    /// <param cref="string" name="dataIn">Данные на вход</param>
    /// <param cref="DateTime?" name="dateDeleted">Дата удаления</param>
    public LogApiSociology(string username, bool isSystem, string method, string type, string? dataIn = null, DateTime? dateDeleted = null) : base(username, isSystem, method, type, dataIn, dateDeleted)
    {

    }

    /// <summary>
    /// Конструктор модели сущности лога сервиса социологии с идентификатором
    /// </summary>
    /// <param cref="long" name="id">Первичный ключ таблицы</param>
    /// <param cref="string" name="username">Логин пользователя, выполняющего действие</param>
    /// <param cref="bool" name="isSystem">Признак системной записи</param>
    /// <param cref="string" name="method">Наименование вызываемого метода</param>
    /// <param cref="string" name="type">Тип вызываемого метода</param>
    /// <param cref="string" name="dataIn">Данные на вход</param>
    /// <param cref="DateTime?" name="dateDeleted">Дата удаления</param>
    public LogApiSociology(long id, string username, bool isSystem, string method, string type, string? dataIn = null, DateTime? dateDeleted = null) : base(id, username, isSystem, method, type, dataIn, dateDeleted)
    {

    }
    #endregion
}