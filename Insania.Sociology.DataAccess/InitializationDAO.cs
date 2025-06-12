using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using Insania.Shared.Contracts.DataAccess;
using Insania.Shared.Contracts.Services;

using Insania.Sociology.Database.Contexts;
using Insania.Sociology.Entities;
using Insania.Sociology.Models.Settings;

using ErrorMessagesShared = Insania.Shared.Messages.ErrorMessages;
using InformationMessages = Insania.Shared.Messages.InformationMessages;

using ErrorMessagesSociology = Insania.Sociology.Messages.ErrorMessages;

namespace Insania.Sociology.DataAccess;

/// <summary>
/// Сервис инициализации данных в бд социологии
/// </summary>
/// <param cref="ILogger{InitializationDAO}" name="logger">Сервис логгирования</param>
/// <param cref="SociologyContext" name="sociologyContext">Контекст базы данных социологии</param>
/// <param cref="LogsApiSociologyContext" name="logsApiSociologyContext">Контекст базы данных логов сервиса социологии</param>
/// <param cref="IOptions{InitializationDataSettings}" name="settings">Параметры инициализации данных</param>
/// <param cref="ITransliterationSL" name="transliteration">Сервис транслитерации</param>
/// <param cref="IConfiguration" name="configuration">Конфигурация приложения</param>
public class InitializationDAO(ILogger<InitializationDAO> logger, SociologyContext sociologyContext, LogsApiSociologyContext logsApiSociologyContext, IOptions<InitializationDataSettings> settings, ITransliterationSL transliteration, IConfiguration configuration) : IInitializationDAO
{
    #region Поля
    private readonly string _username = "initializer";
    #endregion

    #region Зависимости
    /// <summary>
    /// Сервис логгирования
    /// </summary>
    private readonly ILogger<InitializationDAO> _logger = logger;

    /// <summary>
    /// Контекст базы данных социологии
    /// </summary>
    private readonly SociologyContext _sociologyContext = sociologyContext;

    /// <summary>
    /// Контекст базы данных логов сервиса социологии
    /// </summary>
    private readonly LogsApiSociologyContext _logsApiSociologyContext = logsApiSociologyContext;

    /// <summary>
    /// Параметры инициализации данных
    /// </summary>
    private readonly IOptions<InitializationDataSettings> _settings = settings;

    /// <summary>
    /// Сервис транслитерации
    /// </summary>
    private readonly ITransliterationSL _transliteration = transliteration;

    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    private readonly IConfiguration _configuration = configuration;
    #endregion

    #region Методы
    /// <summary>
    /// Метод инициализации данных
    /// </summary>
    /// <exception cref="Exception">Исключение</exception>
    public async Task Initialize()
    {
        try
        {
            //Логгирование
            _logger.LogInformation(InformationMessages.EnteredInitializeMethod);

            //Инициализация структуры
            if (_settings.Value.InitStructure == true)
            {
                //Логгирование
                _logger.LogInformation("{text}", InformationMessages.InitializationStructure);

                //Инициализация баз данных в зависимости от параметров
                if (_settings.Value.Databases?.Sociology == true)
                {
                    //Формирование параметров
                    string connectionServer = _configuration.GetConnectionString("SociologySever") ?? throw new Exception(ErrorMessagesShared.EmptyConnectionString);
                    string patternDatabases = @"^databases_sociology_\d+\.sql$";
                    string connectionDatabase = _configuration.GetConnectionString("SociologyEmpty") ?? throw new Exception(ErrorMessagesShared.EmptyConnectionString);
                    string patternSchemes = @"^schemes_sociology_\d+\.sql$";

                    //Создание базы данных
                    await CreateDatabase(connectionServer, patternDatabases, connectionDatabase, patternSchemes);
                }
                if (_settings.Value.Databases?.LogsApiSociology == true)
                {
                    //Формирование параметров
                    string connectionServer = _configuration.GetConnectionString("LogsApiSociologyServer") ?? throw new Exception(ErrorMessagesShared.EmptyConnectionString);
                    string patternDatabases = @"^databases_logs_api_sociology_\d+\.sql$";
                    string connectionDatabase = _configuration.GetConnectionString("LogsApiSociologyEmpty") ?? throw new Exception(ErrorMessagesShared.EmptyConnectionString);
                    string patternSchemes = @"^schemes_logs_api_sociology_\d+\.sql$";

                    //Создание базы данных
                    await CreateDatabase(connectionServer, patternDatabases, connectionDatabase, patternSchemes);
                }

                //Выход
                return;
            }

            //Накат миграций
            if (_sociologyContext.Database.IsRelational()) await _sociologyContext.Database.MigrateAsync();
            if (_logsApiSociologyContext.Database.IsRelational()) await _logsApiSociologyContext.Database.MigrateAsync();

            //Проверки
            if (string.IsNullOrWhiteSpace(_settings.Value.ScriptsPath)) throw new Exception(ErrorMessagesShared.EmptyScriptsPath);

            //Инициализация данных в зависимости от параметров
            if (_settings.Value.Tables?.Factions == true)
            {
                //Открытие транзакции
                IDbContextTransaction transaction = _sociologyContext.Database.BeginTransaction();

                try
                {
                    //Создание коллекции сущностей
                    List<Faction> entities =
                    [
                        new(_transliteration, 1, _username, "Удалённая", "", DateTime.UtcNow),
                        new(_transliteration, 2, _username, "Правительство", "Правители государств, судебные, законодательные и исполнительные органы (если они есть), представители в других государствах, который определяют внутреннюю и внешнюю политику, управляют различными сферами государства, осуществляют контроль за исполнением приказов и судебные дела высшего уровня", null),
                        new(_transliteration, 3, _username, "Знать", "Знать -", null),
                        new(_transliteration, 4, _username, "Духовенство", "Духовенство -", null),
                        new(_transliteration, 5, _username, "Маги", "Маги -", null),
                        new(_transliteration, 6, _username, "Военные", "Военные -", null),
                        new(_transliteration, 7, _username, "Купечество", "Купечество -", null),
                        new(_transliteration, 8, _username, "Преступность", "Преступность -", null),
                        new(_transliteration, 9, _username, "Интеллигенция", "Интеллигенция -", null),
                        new(_transliteration, 10, _username, "Бесфракционники", "Бесфракционники -", null),
                    ];

                    //Проход по коллекции сущностей
                    foreach (var entity in entities)
                    {
                        //Добавление сущности в бд при её отсутствии
                        if (!_sociologyContext.Factions.Any(x => x.Id == entity.Id)) await _sociologyContext.Factions.AddAsync(entity);
                    }

                    //Сохранение изменений в бд
                    await _sociologyContext.SaveChangesAsync();

                    //Создание шаблона файла скриптов
                    string pattern = @"^t_factions_\d+.sql";

                    //Проходим по всем скриптам
                    foreach (var file in Directory.GetFiles(_settings.Value.ScriptsPath!).Where(x => Regex.IsMatch(Path.GetFileName(x), pattern)))
                    {
                        //Выполняем скрипт
                        await ExecuteScript(file, _sociologyContext);
                    }

                    //Фиксация транзакции
                    transaction.Commit();
                }
                catch (Exception)
                {
                    //Откат транзакции
                    transaction.Rollback();

                    //Проброс исключения
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            //Логгирование
            _logger.LogError("{text}: {error}", ErrorMessagesShared.Error, ex.Message);

            //Проброс исключения
            throw;
        }
    }

    /// <summary>
    /// Метод создание базы данных
    /// </summary>
    /// <param name="connectionServer">Строка подключения к серверу</param>
    /// <param name="patternDatabases">Шаблон файлов создания базы данных</param>
    /// <param name="connectionDatabase">Строка подключения к базе данных</param>
    /// <param name="patternSchemes">Шаблон файлов создания схемы</param>
    /// <returns></returns>
    private async Task CreateDatabase(string connectionServer, string patternDatabases, string connectionDatabase, string patternSchemes)
    {
        //Проход по всем скриптам в директории и создание баз данных
        foreach (var file in Directory.GetFiles(_settings.Value.ScriptsPath!).Where(x => Regex.IsMatch(Path.GetFileName(x), patternDatabases)))
        {
            //Выполнение скрипта
            await ExecuteScript(file, connectionServer);
        }

        //Проход по всем скриптам в директории и создание схем
        foreach (var file in Directory.GetFiles(_settings.Value.ScriptsPath!).Where(x => Regex.IsMatch(Path.GetFileName(x), patternSchemes)))
        {
            //Выполнение скрипта
            await ExecuteScript(file, connectionDatabase);
        }
    }

    /// <summary>
    /// Метод выполнения скрипта со строкой подключения
    /// </summary>
    /// <param cref="string" name="filePath">Путь к скрипту</param>
    /// <param cref="string" name="connectionString">Строка подключения</param>
    private async Task ExecuteScript(string filePath, string connectionString)
    {
        //Логгирование
        _logger.LogInformation("{text} {params}", InformationMessages.ExecuteScript, filePath);

        try
        {
            //Создание соединения к бд
            using NpgsqlConnection connection = new(connectionString);

            //Открытие соединения
            connection.Open();

            //Считывание запроса
            string sql = File.ReadAllText(filePath);

            //Создание sql-запроса
            using NpgsqlCommand command = new(sql, connection);

            //Выполнение команды
            await command.ExecuteNonQueryAsync();

            //Логгирование
            _logger.LogInformation("{text} {params}", InformationMessages.ExecutedScript, filePath);
        }
        catch (Exception ex)
        {
            //Логгирование
            _logger.LogError("{text} {params} из-за ошибки {ex}", ErrorMessagesShared.NotExecutedScript, filePath, ex);
        }
    }

    /// <summary>
    /// Метод выполнения скрипта с контекстом
    /// </summary>
    /// <param cref="string" name="filePath">Путь к скрипту</param>
    /// <param cref="DbContext" name="context">Контекст базы данных</param>
    private async Task ExecuteScript(string filePath, DbContext context)
    {
        //Логгирование
        _logger.LogInformation("{text} {params}", InformationMessages.ExecuteScript, filePath);

        try
        {
            //Считывание запроса
            string sql = File.ReadAllText(filePath);

            //Выполнение sql-команды
            await context.Database.ExecuteSqlRawAsync(sql);

            //Логгирование
            _logger.LogInformation("{text} {params}", InformationMessages.ExecutedScript, filePath);
        }
        catch (Exception ex)
        {
            //Логгирование
            _logger.LogError("{text} {params} из-за ошибки {ex}", ErrorMessagesShared.NotExecutedScript, filePath, ex);
        }
    }
    #endregion
}