using System;
using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.E_com.Rating;
using BusinessObjects.Models.Ecom.Rating;
using DataAccess.DAO;
using DataAccess.DAO.Ecom;

namespace APIs.Services
{
    public class RatingService : IRatingService
    {
        private readonly RatingDAO _ratingDAO;
        public RatingService()
        {
            _ratingDAO = new RatingDAO();
        }

        public Task<int> AddNewUserRatingAsync(Guid userId) => _ratingDAO.AddNewUserRatingAsync(userId);
        public Task<int> AddNewUserRatingRecordAsync(RatingRecord record) => _ratingDAO.AddNewUserRatingRecordAsync(record);

        public Task<int> DeleteUserRatingAsync(Guid recordId) => _ratingDAO.DeleteUserRatingAsync(recordId);


        public Task<int> UpdateUserRatingAsync(RatingRecord record) => _ratingDAO.UpdateUserRatingAsync(record);

        public Task<Guid?> GetRatingIdByRevieweeIdAsync(Guid revieweeId) => _ratingDAO.GetRatingIdByRevieweeIdAsync(revieweeId);

        public async Task<List<Reply>> GetRepliesForRatingRecordId(Guid ratingRecordId) => await _ratingDAO.GetRepliesForRatingRecordId(ratingRecordId);

        public async Task<int> AddNewReplyAsync(Reply reply) => await _ratingDAO.AddNewReplyAsync(reply);

        public async Task<int> UpdateReplyAsync(Reply reply) => await _ratingDAO.UpdateReplyAsync(reply);

        public async Task<int> DeleteReplyByIdAsync(Guid replyId) => await _ratingDAO.DeleteReplyByIdAsync(replyId);
        public async Task<int> RateAndCommentAsync(RatingRecord ratingRecord) => await _ratingDAO.RateAndCommentAsync(ratingRecord);
        public async Task<PagedList<RatingRecord>> GetCommentsByBookId(Guid bookId, PagingParams param)
        {
            return PagedList<RatingRecord>.ToPagedList((await _ratingDAO.GetCommentsByBookId(bookId))?.OrderBy(c => c.Comment).AsQueryable(), param.PageNumber, param.PageSize);

        }

        public async Task<PagedList<RatingRecord>> GetReviewsByAgencyId(Guid agencyId, ReviewSearchCriteria searchCriteria, PagingParams param)  //SONDB
        {
            return PagedList<RatingRecord>.ToPagedList((await _ratingDAO.GetReviewsByAgencyId(agencyId, searchCriteria))?.OrderBy(c => c.Comment).AsQueryable(), param.PageNumber, param.PageSize);

        }
        public async Task<List<Book>> GetBooksByAgencyId(Guid agencyId) => await new BookDAO().GetBooksByAgencyId(agencyId); //SONDB
        public async Task<Book> GetBookByRatingId(Guid ratingId) => await _ratingDAO.GetBookByRatingId(ratingId); //SONDB
    }
}

