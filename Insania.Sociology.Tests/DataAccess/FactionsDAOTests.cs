using Microsoft.Extensions.DependencyInjection;

using Insania.Sociology.Contracts.DataAccess;
using Insania.Sociology.Entities;
using Insania.Sociology.Tests.Base;

namespace Insania.Sociology.Tests.DataAccess;

/// <summary>
/// Тесты сервиса работы с данными фракций
/// </summary>
[TestFixture]
public class FactionsDAOTests : BaseTest
{
    #region Поля
    /// <summary>
    /// Сервис работы с данными фракций
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
    /// Тест метода получения списка фракций
    /// </summary>
    [Test]
    public async Task GetListTest()
    {
        try
        {
            //Получение результата
            List<Faction>? result = await FactionsDAO.GetList();

            //Проверка результата
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
        }
        catch (Exception)
        {
            //Проброс исключения
            throw;
        }
    }
    #endregion
}