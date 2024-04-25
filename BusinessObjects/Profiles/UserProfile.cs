using AutoMapper;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.Utils;

namespace BusinessObjects.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<AppUser, UserProfileDTO>()
            .ForMember(des => des.UserId, mem => mem.MapFrom(src => src.UserId))
            .ForMember(des => des.Username, mem => mem.MapFrom(src => src.Username))
            .ForMember(des => des.Email, mem => mem.MapFrom(src => src.Email))
            .ForMember(des => des.IsValidated, mem => mem.MapFrom(src => src.IsValidated))
            .ForMember(des => des.IsBanned, mem => mem.MapFrom(src => src.IsBanned));

            CreateMap<UserValidationDTO, NIC_Data>()
            .ForMember(des => des.Id, mem => mem.MapFrom(src => src.NicId))
             .ForMember(des => des.Fullname, mem => mem.MapFrom(src => src.NicName))
              .ForMember(des => des.Home, mem => mem.MapFrom(src => src.NicHome))
               .ForMember(des => des.Sex, mem => mem.MapFrom(src => src.NicSex))
                .ForMember(des => des.Nationality, mem => mem.MapFrom(src => src.NicNationality));
        }
    }
}

