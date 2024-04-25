using BusinessObjects;
using BusinessObjects.DTO.Trading;
using BusinessObjects.Enums;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Trading;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO.Trading
{
	public class TradeDetailsDAO
	{
		private readonly AppDbContext _context;
		public TradeDetailsDAO()
		{
			_context = new AppDbContext();
		}

		public async Task<int> AddNewTradeDetailsAsync(TradeDetails data)
		{
			await _context.TradeDetails.AddAsync(data);
			return await _context.SaveChangesAsync();
		}

        public async Task<List<Post>> GetAllTradePostForMiddle()
        {
            var query = from p in _context.Posts
                        join pi in _context.PostInteresters on p.PostId equals pi.PostId
                        join td in _context.TradeDetails on pi.PostInterestId equals td.LockedRecordId
                        where td.Status == TradeStatus.OnDeliveryToMiddle || td.Status == TradeStatus.MiddleReceived || td.Status == TradeStatus.WaitFoeChecklistConfirm
                        select p;
            return await query.ToListAsync();
        }

        public async Task<int> SetTradeStatus(TradeStatus status, Guid recordId)
		{
			TradeDetails? record = await _context.TradeDetails.SingleOrDefaultAsync(r => r.TradeDetailId == recordId);
			if(record != null)
			{
				record.Status = status;
			}
			return await _context.SaveChangesAsync();
		}

        public async Task<TradeDetails?> GetTradeDetailsById(Guid tradeDetailsId)
        {
           return await _context.TradeDetails.SingleOrDefaultAsync(td => td.TradeDetailId == tradeDetailsId);
        }

		public async Task<List<TradeDetails>> GetTradeDetailsByPostIdAsync(Guid postId)
		{
			var query = from td in _context.TradeDetails
						join pi in _context.PostInteresters
						on td.LockedRecordId equals pi.PostInterestId
						where pi.PostId == postId
						select td;
			return await query.ToListAsync();
		}

        public async Task<List<(Guid PostId, TradeStatus TradeStatus)>> GetLockedPostsByUserId(Guid userId, TraderType type)
        {
            if (type == TraderType.Interester)
            {
                var query = (from pi in _context.PostInteresters
                            join td in _context.TradeDetails on pi.PostInterestId equals td.LockedRecordId
                            where pi.InteresterId == userId
                            select new { pi.PostId, td.Status }).Distinct();
                            var result = await query.ToListAsync();
                return result.Select(x => (x.PostId, x.Status)).ToList();
            }
            else
            {
                var query = (from pi in _context.PostInteresters
                            join td in _context.TradeDetails on pi.PostInterestId equals td.LockedRecordId
                            join p in _context.Posts on pi.PostId equals p.PostId
                            where p.UserId == userId
                            select  new { pi.PostId, td.Status }).Distinct();

                var result = await query.ToListAsync();
                return result.Select(x => (x.PostId, x.Status)).ToList();
            }
        }
   
        public async Task<List<(Guid OwnerId, Guid InteresterId)>> GetTraderIdsByPostIdAsync(Guid postId)
        {
            var query = from p in _context.Posts
                        join pi in _context.PostInteresters on p.PostId equals pi.PostId
                        where p.PostId == postId && pi.IsChosen == true
                        select new { p.UserId, pi.InteresterId };

            var result = await query.ToListAsync();
            return result.Select(x => (x.UserId, x.InteresterId)).ToList();
        }

        public async Task<int> UpdateRatingRecordIdAsync(Guid ratingRecordId, Guid tradeDetailId)
		{
			var tradeDetails = await _context.TradeDetails.FindAsync(tradeDetailId);
			if(tradeDetails != null)
			{
                tradeDetails.RatingRecordId = ratingRecordId;
            }
            return await _context.SaveChangesAsync();
		}

        public async Task<int> UpdateTradeDetails(TradeDetails details)
        {
            try
            {
                var record = await _context.TradeDetails.FindAsync(details.TradeDetailId);

                int result = 0;
                if (record != null)
                {
                    var updatedRecord = new UpdateTradeDetailsDTO
                    {
                        TradeDetailId = details.TradeDetailId,
                        AddressId = details.AddressId,
                        Phone = details.Phone,
                        Note = details.Note,
                        Status = details.Status
                    };
                    _context.Entry(record).CurrentValues.SetValues(updatedRecord);
                    result += await _context.SaveChangesAsync(); // Return 1 to indicate success
                }
                return result;
            }
            catch (DbUpdateException e)
            {
                var errorMessage = e.InnerException?.Message ?? e.Message;
                throw new Exception("Failed to update trade details: " + errorMessage);
            }
        }

        public async Task<Post?> GetPostOfTradeDetails(Guid tradeDetailsId)
        {
            var query = (from p in _context.Posts
                         join pi in _context.PostInteresters
                         on p.PostId equals pi.PostId
                         join td in _context.TradeDetails
                         on pi.PostInterestId equals td.LockedRecordId
                         where td.TradeDetailId == tradeDetailsId
                         select p);
            return await query.SingleOrDefaultAsync();
        }

        public async Task<bool> IsTradeDetailsOwner(Guid userId, Guid tradeDetailsId)
        {
            var query = await (from p in _context.Posts
                                     join pi in _context.PostInteresters
                                     on p.PostId equals pi.PostId
                                     join td in _context.TradeDetails
                                     on pi.PostInterestId equals td.LockedRecordId
                                     where td.TradeDetailId == tradeDetailsId
                                     select new { p.UserId, td.IsPostOwner }).SingleOrDefaultAsync();
            var isPostOwner = query?.UserId == userId;
            return isPostOwner == query?.IsPostOwner;
        }
    }
}

