using System;
using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.E_com.Rating;
using BusinessObjects.Models.Ecom.Rating;

namespace APIs.Services.Interfaces
{
    public interface IRatingService
    {
        Task<int> AddNewUserRatingAsync(Guid userId);
        Task<int> AddNewUserRatingRecordAsync(RatingRecord record);
        Task<int> UpdateUserRatingAsync(RatingRecord record);
        Task<int> DeleteUserRatingAsync(Guid recordId);
        Task<Guid?> GetRatingIdByRevieweeIdAsync(Guid revieweeId);
        Task<List<Reply>> GetRepliesForRatingRecordId(Guid ratingRecordId); //SONDB 
        Task<int> RateAndCommentAsync(RatingRecord ratingRecord); //SONDB 
        Task<PagedList<RatingRecord>> GetCommentsByBookId(Guid bookId, PagingParams param); //SONDB 

        Task<int> AddNewReplyAsync(Reply reply); //SONDB 
        Task<int> UpdateReplyAsync(Reply reply); //SONDB 
        Task<int> DeleteReplyByIdAsync(Guid replyId); //SONDB 
        Task<PagedList<RatingRecord>> GetReviewsByAgencyId(Guid agencyId, ReviewSearchCriteria searchCriteria, PagingParams param); //SONDB
        Task<List<Book>> GetBooksByAgencyId(Guid agencyId); //SONDB
        Task<Book> GetBookByRatingId(Guid ratingId);//SONDB    }
    }
}
