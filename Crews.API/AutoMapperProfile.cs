using AutoMapper;
using Crews.API.Data;
using Crews.API.Data.Entities;
using Crews.API.Models;

namespace Crews.API;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, UserViewModel>()
            .ReverseMap()
            .ForSourceMember(model => model.Password, option => option.DoNotValidate());

        CreateMap<Role, RoleViewModel>().ReverseMap();

        CreateMap<Crew, CrewViewModel>()
            .ForSourceMember(entity => entity.LogoId, option => option.DoNotValidate())
            .ForSourceMember(entity => entity.DateTimeAdd, option => option.DoNotValidate())
            .ReverseMap();
        
        CreateMap<Training, TrainingViewModel>()
            .ReverseMap()
            .ForMember(entity => entity.DateTimeAdd, option => option.Ignore())
            .ForMember(entity => entity.Crew, option => option.Ignore());
    }
}