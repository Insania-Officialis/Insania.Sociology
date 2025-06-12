using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Insania.Shared.Contracts.Services;
using Insania.Shared.Entities;

namespace Insania.Sociology.Entities;

/// <summary>
/// Модель сущности фракция
/// </summary>
[Table("d_factions")]
[Comment("Фракции")]
public class Faction : Compendium
{
    #region Конструкторы
    /// <summary>
    /// Простой конструктор модели сущности фракция
    /// </summary>
    public Faction() : base()
    {
        Description = string.Empty;
    }

    /// <summary>
    /// Конструктор модели сущности фракция без идентификатора
    /// </summary>
    /// <param cref="ITransliterationSL" name="transliteration">Сервис транслитерации</param>
    /// <param cref="string" name="username">Логин пользователя, выполняющего действие</param>
    /// <param cref="string" name="name">Наименование</param>
    /// <param cref="string" name="description">Описание</param>
    /// <param cref="DateTime?" name="dateDeleted">Дата удаления</param>
    public Faction(ITransliterationSL transliteration, string username, string name, string description, DateTime? dateDeleted = null) : base(transliteration, username, name, dateDeleted)
    {
        Description = description;
    }

    /// <summary>
    /// Конструктор модели сущности фракция с идентификатором
    /// </summary>
    /// <param cref="ITransliterationSL" name="transliteration">Сервис транслитерации</param>
    /// <param cref="long?" name="id">Идентификатор пользователя</param>
    /// <param cref="string" name="username">Логин пользователя, выполняющего действие</param>
    /// <param cref="string" name="name">Наименование</param>
    /// <param cref="string" name="description">Описание</param>
    /// <param cref="DateTime?" name="dateDeleted">Дата удаления</param>
    public Faction(ITransliterationSL transliteration, long id, string username, string name, string description, DateTime? dateDeleted = null) : base(transliteration, id, username, name, dateDeleted)
    {
        Description = description;
    }
    #endregion

    #region Поля
    /// <summary>
    ///	Описание
    /// </summary>
    [Column("description")]
    [Comment("Описание")]
    public string Description { get; private set; }
    #endregion

    #region Методы
    /// <summary>
    /// Метод записи описания
    /// </summary>
    /// <param cref="string" name="description">Описание</param>
    public void SetDescription(string description) => Description = description;
    #endregion
}