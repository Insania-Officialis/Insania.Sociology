using AutoMapper;

using Insania.Shared.Models.Responses.Base;

using Insania.Sociology.Entities;

namespace Insania.Sociology.Models.Mapper;

/// <summary>
/// Сервис преобразования моделей
/// </summary>
public class SociologyMappingProfile : Profile
{
    /// <summary>
    /// Конструктор сервиса преобразования моделей
    /// </summary>
    public SociologyMappingProfile()
    {
        //Преобразование модели сущности фракции в базовую модель элемента ответа списком
        CreateMap<Faction, BaseResponseListItem>();
    }
}