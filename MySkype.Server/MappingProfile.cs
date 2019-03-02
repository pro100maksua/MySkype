using AutoMapper;
using MySkype.Server.Dto;
using MySkype.Server.Models;

namespace MySkype.Server
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterRequest, User>().ReverseMap();

            CreateMap<User, ResponseUserDto>();
        }
    }
}