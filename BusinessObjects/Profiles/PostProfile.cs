using AutoMapper;
using BusinessObjects.DTO.Trading;
using BusinessObjects.Models.E_com.Trading;

namespace BusinessObjects.Profiles
{
	public class PostProfile: Profile
	{
		public PostProfile()
		{
			CreateMap<UpdatePostDTOs, Post>()
			.ForMember(des => des.PostId, mem => mem.MapFrom(src => src.PostId))
			.ForMember(des => des.Title, mem => mem.MapFrom(src => src.Title))
			.ForMember(des => des.Content, mem => mem.MapFrom(src => src.Content))
			.ForMember(des => des.IsLock, mem => mem.MapFrom(src => src.IsLock));
		}
	}
}

