using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Insania.Shared.Messages;

using Insania.Sociology.Contracts.Services;
using Insania.Sociology.Database.Contexts;
using Insania.Sociology.Entities;

namespace Insania.Sociology.Services;

/// <summary>
/// Сервис фонового логгирования в бд
/// </summary>
public class LoggingSL: ILoggingSL, IHostedService, IDisposable
{
    #region Конструкторы
    /// <summary>
    /// Конструктор фонового сервиса логгирования в бд
    /// </summary>
    /// <param cref="IServiceScopeFactory" name="scopeFactory">Фабрика сервисов для получения зависимостей</param>
    /// <param cref="ILogger{LoggingService}" name="logger">Сервис логгирования</param>
    public LoggingSL(IServiceScopeFactory scopeFactory, ILogger<LoggingSL> logger)
    {
        //Получение зависимостей
        _scopeFactory = scopeFactory;
        _logger = logger;

        //Настройка канала
        var options = new BoundedChannelOptions(1000) //размер очереди
        {
            FullMode = BoundedChannelFullMode.Wait, //ожидание свободного места при заполнения очереди
            SingleReader = true, //одна фоновая задача чтения
            SingleWriter = false //множество фоновых задач записи
        };

        //Создание канала
        _channel = Channel.CreateBounded<LogApiSociology>(options);
    }
    #endregion

    #region Зависимости
    /// <summary>
    /// Канал для синхронизации данных
    /// </summary>
    private readonly Channel<LogApiSociology> _channel;

    /// <summary>
    /// Фабрика сервисов для получения зависимостей
    /// </summary>
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Сервис логгирования
    /// </summary>
    private readonly ILogger<LoggingSL> _logger;
    #endregion

    #region Поля
    /// <summary>
    /// Фоновая задача для обработки логов из канала
    /// </summary>
    private Task? _backgroundTask;

    /// <summary>
    /// Источник токена отмены
    /// </summary>
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Пачка записей
    /// </summary>
    private readonly List<LogApiSociology> _batch = [];

    /// <summary>
    /// Размер пачки записей
    /// </summary>
    private readonly int _batchSize = 50;

    /// <summary>
    /// Таймаут пачки записей
    /// </summary>
    private readonly TimeSpan _batchTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Таймер пачки записей
    /// </summary>
    private Timer? _batchTimer;
    #endregion

    #region Методы
    /// <summary>
    /// Метод запуска задания
    /// </summary>
    /// <param cref="CancellationToken" name="cancellationToken">Токен отмены</param>
    /// <returns cref="Task">Задание</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        //Запуск фоновой задачи
        _backgroundTask = ProcessLogsAsync(_cancellationTokenSource.Token);

        //Возврат завершённой задачи
        return Task.CompletedTask;
    }

    /// <summary>
    /// Метод остановки сервиса
    /// </summary>
    /// <param cref="CancellationToken" name="cancellationToken">Токен отмены</param>
    /// <returns cref="Task">Задание</returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //Завершение записи в канал
        _channel.Writer.Complete();

        //Ожидание заверешния фонового задания
        if (_backgroundTask != null) await _backgroundTask;
    }

    /// <summary>
    /// Метод постановки лога в очередь на обработку
    /// </summary>
    /// <param cref="LogApiSociology" name="log">Лог для записи</param>
    /// <returns cref="ValueTask">Задание</returns>
    public ValueTask QueueLogAsync(LogApiSociology log) => _channel.Writer.WriteAsync(log, _cancellationTokenSource.Token);

    /// <summary>
    /// Освобождение ресурсов при уничтожении сервиса
    /// </summary>
    public void Dispose()
    {
        //Освобождение управляемых ресурсов
        _cancellationTokenSource?.Dispose();

        //Сообщение сборщику мусора, что не нужна финализация
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Внутренние методы
    /// <summary>
    /// Основной метод обработки логов из канала
    /// </summary>
    /// <param cref="CancellationToken" name="cancellationToken">Токен отмены</param>
    /// <returns cref="Task">Задание</returns>
    private async Task ProcessLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            //Асинхронный перебор всех сообщений из канала
            await foreach (var log in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    //Добавление лога в пачку записей
                    _batch.Add(log);

                    //Сохранение пачки по количеству или таймеру
                    if (_batch.Count >= _batchSize) await SaveBatchAsync(cancellationToken);
                    else _batchTimer ??= new Timer(async _ => await SaveBatchAsync(cancellationToken), null, _batchTimeout, Timeout.InfiniteTimeSpan);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ErrorMessages.Error);
                }
            }

            //Сохранение оставшихся записей
            if (_batch.Count > 0) await SaveBatchAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.Error);
        }
    }

    /// <summary>
    /// Метод сохранения пачки записей
    /// </summary>
    /// <param cref="CancellationToken" name="cancellationToken">Токен отмены</param>
    /// <returns cref="Task">Задание</returns>
    private async Task SaveBatchAsync(CancellationToken cancellationToken)
    {
        //Выход при отстутсвии записей
        if (_batch.Count == 0) return;

        try
        {
            //Создание новой области видимости
            using IServiceScope scope = _scopeFactory.CreateScope();

            //Получение нового экземпляра базы данных
            LogsApiSociologyContext context = scope.ServiceProvider.GetRequiredService<LogsApiSociologyContext>();

            //Добавление логов в базу данных
            context.Logs.AddRange(_batch);

            //Сохранение лога
            await context.SaveChangesAsync(cancellationToken);

            //Очистка пачки записей
            _batch.Clear();
        }
        catch (Exception ex)
        {
            //Логгирование ошибки
            _logger.LogError(ex, ErrorMessages.Error);
        }
        finally
        {
            //Сброс таймера
            _batchTimer?.Dispose();
            _batchTimer = null;
        }
    }
    #endregion
}