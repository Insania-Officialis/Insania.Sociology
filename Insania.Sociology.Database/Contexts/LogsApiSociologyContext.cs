using Microsoft.EntityFrameworkCore;

using Insania.Sociology.Entities;

namespace Insania.Sociology.Database.Contexts;

/// <summary>
/// Контекст базы данных логов сервиса социологии
/// </summary>
public class LogsApiSociologyContext : DbContext
{
    #region Конструкторы
    /// <summary>
    /// Простой конструктор контекста логов сервиса социологии
    /// </summary>
    public LogsApiSociologyContext() : base()
    {

    }

    /// <summary>
    /// Конструктор контекста логов сервиса социологии с опциями
    /// </summary>
    /// <param cref="DbContextOptions{LogsApiSociologyContext}" name="options">Параметры</param>
    public LogsApiSociologyContext(DbContextOptions<LogsApiSociologyContext> options) : base(options)
    {

    }
    #endregion

    #region Поля
    /// <summary>
    /// Логи API социологии
    /// </summary>
    public virtual DbSet<LogApiSociology> Logs { get; set; }
    #endregion

    #region Методы
    /// <summary>
    /// Метод при создании моделей
    /// </summary>
    /// <param cref="ModelBuilder" name="modelBuilder">Конструктор моделей</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Установка схемы базы данных
        modelBuilder.HasDefaultSchema("insania_logs_api_sociology");

        // Добавление gin-индекса на поле с входными данными логов
        modelBuilder.Entity<LogApiSociology>().HasIndex(x => x.DataIn).HasMethod("gin");

        // Добавление gin-индекса на поле с выходными данными логов
        modelBuilder.Entity<LogApiSociology>().HasIndex(x => x.DataOut).HasMethod("gin");
    }
    #endregion
}