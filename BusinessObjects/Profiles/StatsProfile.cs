using System;
using AutoMapper;
using BusinessObjects.DTO;
using BusinessObjects.Models.Utils;

namespace BusinessObjects.Profiles
{
	public class StatsProfile: Profile
	{
		public StatsProfile()
		{
            CreateMap<NewStatDTO, Statistic>()
            .ForMember(des => des.BookId, mem => mem.MapFrom(src => src.BookId))
            .ForMember(des => des.PostId, mem => mem.MapFrom(src => src.PostId))
            .ForMember(des => des.BookId, mem => mem.MapFrom(src => src.BookId))
            .ForMember(des => des.Interested, mem => mem.MapFrom(src => src.Interested))
            .ForMember(des => des.View, mem => mem.MapFrom(src => src.View))
            .ForMember(des => des.Hearts, mem => mem.MapFrom(src => src.Hearts))
            .ForMember(des => des.Search, mem => mem.MapFrom(src => src.Search));

            CreateMap<UpdateStatDTO, Statistic>()
            .ForMember(des => des.StatId, mem => mem.MapFrom(src => src.StatId))
            .ForMember(des => des.BookId, mem => mem.MapFrom(src => src.BookId))
            .ForMember(des => des.PostId, mem => mem.MapFrom(src => src.PostId))
            .ForMember(des => des.BookId, mem => mem.MapFrom(src => src.BookId))
            .ForMember(des => des.Interested, mem => mem.MapFrom(src => src.Interested))
            .ForMember(des => des.View, mem => mem.MapFrom(src => src.View))
            .ForMember(des => des.Hearts, mem => mem.MapFrom(src => src.Hearts))
            .ForMember(des => des.Search, mem => mem.MapFrom(src => src.Search)); ;
        }
	}
}

