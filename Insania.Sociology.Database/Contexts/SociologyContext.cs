using Microsoft.EntityFrameworkCore;

using Insania.Sociology.Entities;

namespace Insania.Sociology.Database.Contexts;

/// <summary>
/// Контекст базы данных социологии
/// </summary>
public class SociologyContext : DbContext
{
    #region Конструкторы
    /// <summary>
    /// Простой конструктор контекста
    /// </summary>
    public SociologyContext() : base()
    {

    }

    /// <summary>
    /// Конструктор контекста с опциями
    /// </summary>
    /// <param cref="DbContextOptions{SociologyContext}" name="options">Параметры</param>
    public SociologyContext(DbContextOptions<SociologyContext> options) : base(options)
    {

    }
    #endregion

    #region Поля
    /// <summary>
    /// Фракции
    /// </summary>
    public virtual DbSet<Faction> Factions { get; set; }
    #endregion

    #region Методы
    /// <summary>
    /// Метод при создании моделей
    /// </summary>
    /// <param cref="ModelBuilder" name="modelBuilder">Конструктор моделей</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Установка схемы базы данных
        modelBuilder.HasDefaultSchema("insania_sociology");

        // Создание ограничения уникальности на псевдоним фракции
        modelBuilder.Entity<Faction>().HasAlternateKey(x => x.Alias);
    }
    #endregion
}