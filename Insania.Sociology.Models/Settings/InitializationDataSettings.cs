namespace Insania.Sociology.Models.Settings;

/// <summary>
/// Модель параметров инициализации данных
/// </summary>
public class InitializationDataSettings
{
    /// <summary>
    /// Признак инициализации структуры
    /// </summary>
    /// <remarks>
    /// Нужен для запуска миграций, при true не происходит инициализация данных
    /// </remarks>
    public bool? InitStructure { get; set; }

    /// <summary>
    /// Путь к файлам скриптов
    /// </summary>
    public string? ScriptsPath { get; set; }

    /// <summary>
    /// Включение в инициализацию таблиц
    /// </summary>
    public InitializationDataSettingsIncludeTables? Tables { get; set; }

    /// <summary>
    /// Включение в инициализацию баз данных
    /// </summary>
    public InitializationDataSettingsIncludeDatabases? Databases { get; set; }
}

/// <summary>
/// Модель параметра включения в инициализацию таблиц
/// </summary>
public class InitializationDataSettingsIncludeTables
{
    /// <summary>
    /// Фракции
    /// </summary>
    public bool? Factions { get; set; }
}

/// <summary>
/// Модель параметра включения в инициализацию баз данных
/// </summary>
public class InitializationDataSettingsIncludeDatabases
{
    /// <summary>
    /// Социология
    /// </summary>
    public bool? Sociology { get; set; }

    /// <summary>
    /// Логи сервиса социологии
    /// </summary>
    public bool? LogsApiSociology { get; set; }
}