using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Insania.Shared.Messages;
using Insania.Shared.Models.Requests.Logs;

using Insania.Sociology.Contracts.Services;
using Insania.Sociology.Database.Contexts;
using Insania.Sociology.Entities;

namespace Insania.Sociology.Middleware;

/// <summary>
/// Конвейер запросов логгирования
/// </summary>
/// <param cref="RequestDelegate" name="next">Делегат следующего метода</param>
/// <param cref="ILogger{LoggingMiddleware}" name="logger">Сервис логгирования</param>
public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
{
    #region Зависимости
    /// <summary>
    /// Делегат следующего метода
    /// </summary>
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Сервис логгирования
    /// </summary>
    private readonly ILogger<LoggingMiddleware> _logger = logger;
    #endregion

    #region Поля
    /// <summary>
    /// Успешные статусы
    /// </summary>
    private readonly static List<int> _successCodes = [200, 204];

    /// <summary>
    /// Исключения, которым не нужно фиксировать тело запроса и ответа
    /// </summary>
    private readonly static List<string> _exceptions = ["swagger"];
    #endregion

    #region Основные методы
    /// <summary>
    /// Метод перехватывания запросов
    /// </summary>
    /// <param cref="HttpContext" name="context">Контекст запроса</param>
    /// <param cref="ILoggingSL" name="loggingSL">Сервис логгирования в бд</param>
    public async Task Invoke(HttpContext context, ILoggingSL loggingSL)
    {
        //Получение параметров запроса
        var method = context.Request.Path; //адрес запроса
        var type = context.Request.Method; //тип запроса
        var request = await GetRequest(context.Request); //тело и query параметры запроса

        //Запись в базу данных начала выполнения
        LogApiSociology log = new("system", true, method, type, request, null);
        _ = Task.Run(async () =>
        {
            try
            {
                await loggingSL.QueueLogAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessages.Error);
            }
        });

        //Объявление переменной ответа
        string? response = null;

        try
        {
            //Проверка исключений
            if (!_exceptions.Any(x => method.ToString().Contains(x)))
            {
                //Получение оригинального потока ответа
                var originalBodyStream = context.Response.Body;

                //Перехват тела ответа
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;
                await _next(context);
                response = await GetResponse(context.Response);

                //Получение кода статуса
                var statusCode = context.Response.StatusCode;

                //Определение успешности ответа
                var success = _successCodes.Any(x => x == statusCode);

                //Запись результата выполнения в лог
                log.SetEnd(success, response, statusCode);

                //Возвращение в ответ оригинального потока
                await responseBody.CopyToAsync(originalBodyStream);
            }
            else
            {
                //Переход к следующему элементу
                await _next(context);

                //Получение кода статуса
                var statusCode = context.Response.StatusCode;

                //Определение успешности ответа
                var success = _successCodes.Any(x => x == statusCode);

                //Запись результата выполнения в лог
                log.SetEnd(success, response, statusCode);
            }
        }
        catch (Exception ex)
        {
            //Логгирование ошибки
            log.SetEnd(false, ex.Message, 500);
        }
        finally
        {
            //Запись в бд окончания выполнения
            _ = Task.Run(async () =>
            {
                try
                {
                    await loggingSL.QueueLogAsync(log);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ErrorMessages.Error);
                }
            });
        }
    }
    #endregion

    #region Вспомогательные методы
    /// <summary>
    /// Метод получения запроса
    /// </summary>
    /// <param cref="HttpRequest" name="request">Запрос</param>
    /// <returns cref="string">Строка запроса</returns>
    private static async Task<string> GetRequest(HttpRequest request)
    {
        //Проверка исключений
        if (!_exceptions.Any(x => request.Path.ToString().Contains(x)))
        {
            //Включение возможности чтения тела запроса несколько раз
            request.EnableBuffering();

            //Установка потока в начало
            request.Body.Position = 0;

            //Считывание потока
            var reader = new StreamReader(request.Body);
            var bodyString = await reader.ReadToEndAsync();

            //Установка потока в начало
            request.Body.Position = 0;

            //Создание модели запроса
            var logRequest = new
            {
                Token = request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value.ToString(),
                QueryParams = request.QueryString.ToString(),
                Body = string.IsNullOrEmpty(bodyString) ? null : IsJsonRequest(request) ? JsonConvert.DeserializeObject(bodyString) : bodyString
            };

            //Настройки сериализации для избежания экранирования
            var settings = new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };

            //Возврат результата
            return JsonConvert.SerializeObject(logRequest, settings);
        }
        else
        {
            //Создание модели запроса
            LogRequest logRequest = new(request.Headers.FirstOrDefault(x => x.Key == "Authorization").Value, request.QueryString.ToString(), null);

            //Возврат результата
            return JsonConvert.SerializeObject(logRequest);
        }
    }

    /// <summary>
    /// Метод получения ответа
    /// </summary>
    /// <param cref="HttpResponse" name="response">Ответ</param>
    /// <returns cref="string">Строка ответа</returns>
    private static async Task<string> GetResponse(HttpResponse response)
    {
        //Установка потока в начало
        response.Body.Seek(0, SeekOrigin.Begin);

        //Считывание потока
        string bodyString = await new StreamReader(response.Body).ReadToEndAsync();

        //Установка потока в начало
        response.Body.Seek(0, SeekOrigin.Begin);

        //Возврат самого тела для json
        if (IsJsonResponse(response)) return bodyString;
        //Сериализация в json в ином случае
        else return JsonConvert.SerializeObject(bodyString);
    }

    /// <summary>
    /// Метод проверки, является ли ответ JSON
    /// </summary>
    /// <param cref="HttpResponse" name="response">Ответ</param>
    /// <returns cref="bool">True если JSON</returns>
    private static bool IsJsonResponse(HttpResponse response)
    {
        return response.ContentType?.Contains("application/json") == true ||
               response.ContentType?.Contains("text/json") == true ||
               response.ContentType?.Contains("+json") == true;
    }

    /// <summary>
    /// Метод проверки, является ли запрос JSON
    /// </summary>
    /// <param cref="HttpRequest" name="request">Запрос</param>
    /// <returns cref="bool">True если JSON</returns>
    private static bool IsJsonRequest(HttpRequest request)
    {
        return request.ContentType?.Contains("application/json") == true ||
               request.ContentType?.Contains("text/json") == true ||
               request.ContentType?.Contains("+json") == true;
    }
    #endregion
}