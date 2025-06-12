using Microsoft.Extensions.DependencyInjection;

using Insania.Shared.Contracts.DataAccess;

using Insania.Sociology.Contracts.DataAccess;
using Insania.Sociology.Entities;
using Insania.Sociology.Tests.Base;

namespace Insania.Sociology.Tests.DataAccess;

/// <summary>
/// Тесты сервиса инициализации данных в бд социологии
/// </summary>
[TestFixture]
public class InitializationDAOTests : BaseTest
{
    #region Поля
    /// <summary>
    /// Сервис инициализации данных в бд социологии
    /// </summary>
    private IInitializationDAO InitializationDAO { get; set; }

    /// <summary>
    /// Сервис работы с данными стран
    /// </summary>
    private IFactionsDAO FactionsDAO { get; set; }
    #endregion

    #region Общие методы
    /// <summary>
    /// Метод, вызываемый до тестов
    /// </summary>
    [SetUp]
    public void Setup()
    {
        //Получение зависимости
        InitializationDAO = ServiceProvider.GetRequiredService<IInitializationDAO>();
        FactionsDAO = ServiceProvider.GetRequiredService<IFactionsDAO>();
    }

    /// <summary>
    /// Метод, вызываемый после тестов
    /// </summary>
    [TearDown]
    public void TearDown()
    {

    }
    #endregion

    #region Методы тестирования
    /// <summary>
    /// Тест метода инициализации данных
    /// </summary>
    [Test]
    public async Task InitializeTest()
    {
        try
        {
            //Выполнение метода
            await InitializationDAO.Initialize();

            //Получение сущностей
            List<Faction> factions = await FactionsDAO.GetList();

            //Проверка результата
            Assert.Multiple(() =>
            {
                Assert.That(factions, Is.Not.Empty);
            });
        }
        catch (Exception)
        {
            //Проброс исключения
            throw;
        }
    }
    #endregion
}