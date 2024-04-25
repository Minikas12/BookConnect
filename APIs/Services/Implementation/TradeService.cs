using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects.Enums;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Trading;
using DataAccess.DAO.E_com;
using DataAccess.DAO.Ecom;
using DataAccess.DAO.Trading;

namespace APIs.Services
{ 
	public class TradeService: ITradeService
	{
		private readonly TradeDetailsDAO _tradeDetailsDAO;
		private readonly RatingDAO _ratingDAO;
		private readonly PostDAO _postDAO;

		public TradeService()
		{
			_tradeDetailsDAO = new TradeDetailsDAO();
			_ratingDAO = new RatingDAO();
			_postDAO = new PostDAO();
		}

        public async Task<int> AddNewTradeDetailsAsync(TradeDetails data) => await _tradeDetailsDAO.AddNewTradeDetailsAsync(data);

		public async Task<PagedList<(Guid PostId, TradeStatus TradeStatus)>> GetLockedPostsByUserId(Guid userId, TraderType type, PagingParams @params)
		{
            return PagedList<(Guid PostId, TradeStatus TradeStatus)>.ToPagedList((await _tradeDetailsDAO.GetLockedPostsByUserId(userId, type)).AsQueryable(), @params.PageNumber, @params.PageSize);
        }

		public async Task<TradeDetails?> GetTradeDetailsById(Guid tradeDetailsId)
		=> await _tradeDetailsDAO.GetTradeDetailsById(tradeDetailsId);

        public async Task<List<TradeDetails>> GetTradeDetailsByPostIdAsync(Guid postId) => await _tradeDetailsDAO.GetTradeDetailsByPostIdAsync(postId);

		public Task<List<(Guid OwnerId, Guid InteresterId)>> GetTraderIdsByPostIdAsync(Guid postId)
		=> _tradeDetailsDAO.GetTraderIdsByPostIdAsync(postId);

        public async Task<int> SetTradeStatus(TradeStatus status, Guid recordId) => await _tradeDetailsDAO.SetTradeStatus(status, recordId);

		public async Task<int> UpdateRatingRecordIdAsync(Guid ratingRecordId, Guid tradeDetailId, Guid ratingId)
		{
            int changes = await _tradeDetailsDAO.UpdateRatingRecordIdAsync(ratingRecordId, tradeDetailId);
			await _ratingDAO.RefreshOverallRating(ratingId);
			return changes;
        }

		public async Task<int> UpdateTradeDetails(TradeDetails details)
		=> await _tradeDetailsDAO.UpdateTradeDetails(details);

		public async Task<Post?> GetPostByTradeDetailsId(Guid tradeDetailsId)
		=> await _tradeDetailsDAO.GetPostOfTradeDetails(tradeDetailsId);

		public async Task<bool> IsTradeDetailsOwner(Guid userId, Guid tradeDetailsId)
		=> await _tradeDetailsDAO.IsTradeDetailsOwner(userId, tradeDetailsId);

		public async Task<PagedList<Post>> GetAllTradePostForMiddle(PagingParams @params)
		{
			return PagedList<Post>.ToPagedList((await _tradeDetailsDAO.GetAllTradePostForMiddle()).AsQueryable(), @params.PageNumber, @params.PageSize);
        }

        /*--------------------------------------Bookchecklist--------------------------------------*/
  //      public async Task AddBookCheckList(BookCheckList model)
		//=> await _bookCheckListDAO.Insert(model);

  //      public void DeleteCheckList(Guid id)
  //      => _bookCheckListDAO.DeleteById(id);

  //      public void UpdateCheckList(BookCheckList data)
  //      => _bookCheckListDAO.Update(data);

  //      public async Task<IEnumerable<BookCheckList>> GetCheckListByTradeDetailsId(Guid id)
  //      => await _bookCheckListDAO.GetCheckListByTradeDetailsId(id);

  //      public async Task AddMultipleCheckList(List<BookCheckList> checkLists)
  //      => await _bookCheckListDAO.AddMultipleCheckList(checkLists);

  //      public async Task<BookCheckList?> GetById(Guid id)
  //      => await _bookCheckListDAO.GetById(id);

  //      public async Task<bool> IsCheckListExisted(Guid tradeDetailsId)
		//=> await _bookCheckListDAO.IsCheckListExisted(tradeDetailsId);
    }
}

