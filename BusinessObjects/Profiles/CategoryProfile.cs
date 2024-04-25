using System;
using AutoMapper;
using BusinessObjects.DTO;
using BusinessObjects.Models;

namespace BusinessObjects.Profiles
{
	public class CategoryProfile: Profile
	{
		public CategoryProfile()
		{
			CreateMap<Category, CateNameAndIdDTO>();
			//.ForMember(des => des.CateId, mem => mem.MapFrom(src => src.CateId))
			//.ForMember(des => des.CateName, mem => mem.MapFrom(src => src.CateName));
        }
	}
}

