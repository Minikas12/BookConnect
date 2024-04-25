using AutoMapper;
using BusinessObjects.DTO.Trading;
using BusinessObjects.Models.Trading;

namespace BusinessObjects.Profiles
{
	public class TradeProfile: Profile
	{
		public TradeProfile()
		{
			CreateMap<CheckListUpdateDTO, BookCheckList>()
			.ForMember(des => des.Id, mem => mem.MapFrom(src => src.Id))
			.ForMember(des => des.Target, mem => mem.MapFrom(src => src.Target))
			.ForMember(des => des.TradeDetailsId, mem => mem.MapFrom(src => src.TradeDetailsId));

        }
	}
}

