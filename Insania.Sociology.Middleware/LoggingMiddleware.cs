using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

using Insania.Shared.Models.Requests.Logs;

using Insania.Sociology.Database.Contexts;
using Insania.Sociology.Entities;

namespace Insania.Sociology.Middleware;

/// <summary>
/// Сервис логгирования конвейера запросов
/// </summary>
/// <param name="next">Делегат следующего метода</param>
public class LoggingMiddleware(RequestDelegate next)
{
    #region Поля
    /// <summary>
    /// Делегат следующего метода
    /// </summary>
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Успешные статусы
    /// </summary>
    private readonly static List<string> _successCodes = ["200", "204"];

    /// <summary>
    /// Исключения, которым не нужно фиксировать тело запроса и ответа
    /// </summary>
    private readonly static List<string> _exceptions = ["swagger"];
    #endregion

    #region Основные методы
    /// <summary>
    /// Метод перехватывания запросов
    /// </summary>
    /// <param name="context">Контекст запроса</param>
    /// <param name="contextDB">Контекст базы данных</param>
    public async Task Invoke(HttpContext context, LogsApiSociologyContext contextDB)
    {
        //Получение параметров запроса
        var method = context.Request.Path; //адрес запроса
        var type = context.Request.Method; //тип запроса
        var request = await GetRequest(context.Request); //тело и query параметры запроса

        //Запись в базу данных начала выполнения
        LogApiSociology log = new("system", true, method, type, request, null);
        contextDB.Logs.Add(log);
        await contextDB.SaveChangesAsync();

        //Определение успешности ответа
        var success = _successCodes.Any(x => x == context.Response.StatusCode.ToString());

        //Объявление переменной ответа
        string? response = null;

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

            //Запись результата выполнения в лог
            log.SetEnd(success, response);
            contextDB.Logs.Update(log);
            await contextDB.SaveChangesAsync();

            //Возвращение в ответ оригинального потока
            await responseBody.CopyToAsync(originalBodyStream);
        }
        else
        {
            //Переход к следующему элементу
            await _next(context);

            //Запись результата выполнения в лог
            log.SetEnd(success, response);
            contextDB.Logs.Update(log);
            await contextDB.SaveChangesAsync();
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

            //Снова устанавливаем поток в начало
            request.Body.Position = 0;

            //Создание модели запроса
            LogRequest logRequest = new(request.Headers.FirstOrDefault(x => x.Key == "authentication").Value, request.QueryString.ToString(), bodyString);

            //Возврат результата
            return JsonConvert.SerializeObject(logRequest);
        }
        else
        {
            //Создание модели запроса
            LogRequest logRequest = new(request.Headers.FirstOrDefault(x => x.Key == "authentication").Value, request.QueryString.ToString(), null);

            //Возврат результата
            return JsonConvert.SerializeObject(logRequest);
        }
    }

    /// <summary>
    /// Метод получения ответа
    /// </summary>
    /// <param name="response">Ответ</param>
    /// <returns cref="string">Строка ответа</returns>
    private static async Task<string> GetResponse(HttpResponse response)
    {
        //Установка потока в начало
        response.Body.Seek(0, SeekOrigin.Begin);

        //Считывание потока
        string bodyString = await new StreamReader(response.Body).ReadToEndAsync();

        //Установка потока в начало
        response.Body.Seek(0, SeekOrigin.Begin);

        //Возврат результата
        return JsonConvert.SerializeObject(bodyString);
    }
    #endregion
}