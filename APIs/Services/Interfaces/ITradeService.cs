using APIs.Utils.Paging;
using BusinessObjects.Enums;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Trading;

namespace APIs.Services.Interfaces
{
    public interface ITradeService
    {
        Task<int> AddNewTradeDetailsAsync(TradeDetails data);
        Task<int> SetTradeStatus(TradeStatus status, Guid recordId);
        Task<List<TradeDetails>> GetTradeDetailsByPostIdAsync(Guid postId);
        Task<int> UpdateRatingRecordIdAsync(Guid ratingRecordId, Guid tradeDetailId, Guid ratingId);
        Task<List<(Guid OwnerId, Guid InteresterId)>> GetTraderIdsByPostIdAsync(Guid postId);
        Task<PagedList<(Guid PostId, TradeStatus TradeStatus)>> GetLockedPostsByUserId(Guid userId, TraderType type, PagingParams @params);
        Task<int> UpdateTradeDetails(TradeDetails details);
        Task<TradeDetails?> GetTradeDetailsById(Guid tradeDetailsId);
        Task<Post?> GetPostByTradeDetailsId(Guid tradeDetailsId);
        Task<bool> IsTradeDetailsOwner(Guid userId, Guid tradeDetailsId);
        Task<PagedList<Post>> GetAllTradePostForMiddle(PagingParams @params);
   
    }
}