using Microsoft.Extensions.DependencyInjection;

using Insania.Shared.Models.Responses.Base;

using Insania.Sociology.Contracts.BusinessLogic;
using Insania.Sociology.Tests.Base;

namespace Insania.Sociology.Tests.BusinessLogic;

/// <summary>
/// Тесты сервиса работы с бизнес-логикой фракций
/// </summary>
[TestFixture]
public class FactionsBLTests : BaseTest
{
    #region Поля
    /// <summary>
    /// Сервис работы с бизнес-логикой фракций
    /// </summary>
    private IFactionsBL FactionsBL { get; set; }
    #endregion

    #region Общие методы
    /// <summary>
    /// Метод, вызываемый до тестов
    /// </summary>
    [SetUp]
    public void Setup()
    {
        //Получение зависимости
        FactionsBL = ServiceProvider.GetRequiredService<IFactionsBL>();
    }

    /// <summary>
    /// Метод, вызываемый после тестов
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        // Очистка ресурсов (при необходимости)
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
            BaseResponseList? result = await FactionsBL.GetList();

            //Проверка результата
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);
                Assert.That(result.Items, Is.Not.Null);
                Assert.That(result.Items, Is.Not.Empty);
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