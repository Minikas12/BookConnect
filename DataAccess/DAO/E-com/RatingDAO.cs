using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.E_com.Rating;
using BusinessObjects.Models.Ecom.Rating;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO.Ecom
{
    public class RatingDAO
    {
        private readonly AppDbContext _context;
        public RatingDAO()
        {
            _context = new AppDbContext();
        }

        public async Task<int> AddNewUserRatingAsync(Guid userId)
        {
            await _context.Ratings.AddAsync(new Rating
            {
                RatingId = Guid.NewGuid(),
                OverallRating = 0,
                RevieweeId = userId,
                TotalReview = 0
            });
            return await _context.SaveChangesAsync();
        }

        public async Task<double?> GetOverallUserById(Guid userId)
        {
            double? result = null;
            Rating? rate = await _context.Ratings.SingleOrDefaultAsync(r => r.RevieweeId == userId);
            if (rate != null)
            {
                result = rate?.OverallRating;
            }
            return result;
        }

        public async Task<Guid?> GetRatingIdByRevieweeIdAsync(Guid revieweeId)
        {
            Rating? rating = await _context.Ratings.SingleOrDefaultAsync(r => r.RevieweeId == revieweeId);
            return (rating != null && rating.RevieweeId != null) ? rating.RatingId : null;
        }

        public async Task<int> AddNewUserRatingRecordAsync(RatingRecord record)
        {
            try
            {
                await _context.RatingRecords.AddAsync(record);
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                string innerExceptionMessage = "";
                // Handle the database exception and retrieve the inner exception
                if (ex.InnerException != null)
                {
                    // Log or handle the inner exception details
                   innerExceptionMessage  = ex.InnerException.Message;
                    // Perform appropriate actions based on the inner exception details
                } throw new Exception(innerExceptionMessage);
            }
        }

        public async Task<int> UpdateUserRatingAsync(RatingRecord record)
        {
            _context.RatingRecords.Update(record);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteUserRatingAsync(Guid recordId)
        {
            RatingRecord? record = await _context.RatingRecords.SingleOrDefaultAsync(r => r.RatingRecordId == recordId);
            if (record != null)
            {
                _context.RatingRecords.Remove(record);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> RefreshOverallRating(Guid ratingId)
        {
            var sumValue = 0;
            var totalRecord = 0;
            foreach (var records in await _context.RatingRecords.Where(rr => rr.RatingId == ratingId).ToListAsync())
            {
                sumValue += records.RatingPoint;
                totalRecord += 1;
            }
            Rating? rating = await _context.Ratings.SingleOrDefaultAsync(r => r.RatingId == ratingId);
            if (rating != null)
            {
                rating.OverallRating = sumValue / totalRecord;
                rating.TotalReview = totalRecord;
            }
            return await _context.SaveChangesAsync();
        }

        public Guid GetRatingId(Guid productId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Query the database to find the rating ID associated with the given product ID
                    Book book = context.Books.FirstOrDefault(b => b.ProductId == productId);

                    // Return the inventory ID if found, otherwise return Guid.Empty
                    return book?.RatingId ?? Guid.Empty;
                }
            }
            catch (Exception e)
            {
                // Log the exception and handle it accordingly
                throw new Exception("Error occurred while fetching agency ID: " + e.Message);
            }
        }

        public int AddNewRating(Rating rating)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Ratings.Add(rating);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public PointandIdDTO GetRatingInfo(Guid productId) //SONDB 
        {
            using (var context = new AppDbContext())
            {
                var book = context.Books.FirstOrDefault(b => b.ProductId == productId);

                if (book != null && book.RatingId != null)
                {
                    var rating = context.Ratings.FirstOrDefault(r => r.RatingId == book.RatingId);

                    if (rating != null)
                    {
                        return new PointandIdDTO
                        {
                            RatingId = rating.RatingId,
                            OverallRating = rating.OverallRating
                        };
                    }
                    else
                    {
                        // Handle if rating not found for the provided RatingId
                        throw new Exception("Rating not found for the provided RatingId.");
                    }
                }
                else
                {
                    // Handle if no RatingId found for the provided ProductId
                    return null; // Or handle it as required, e.g., return a default DTO with null values
                }
            }
        }

        public async Task<Book> GetBookByRatingId(Guid ratingId) //SONDB
        {
            try
            {
                // Find the book with the given ratingId
                var book = await _context.Books.FirstOrDefaultAsync(b => b.RatingId == ratingId);

                return book;
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving book with RatingId {ratingId}: {e.Message}");
            }
        }

        public async Task<int> RateAndCommentAsync(RatingRecord ratingRecord) //SONDB 
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.RatingRecords.Add(ratingRecord);
                    return await context.SaveChangesAsync(); // Use SaveChangesAsync method
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<RatingRecord>> GetCommentsByBookId(Guid bookId) //SONDB 
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Retrieve the RatingId of the specified book asynchronously
                    var ratingId = await context.Books
                        .Where(b => b.ProductId == bookId)
                        .Select(b => b.RatingId)
                        .FirstOrDefaultAsync();

                    if (ratingId != null)
                    {
                        // Retrieve RatingRecords based on the RatingId asynchronously
                        var comments = await context.RatingRecords
                            .Where(rr => rr.RatingId == ratingId && !string.IsNullOrEmpty(rr.Comment))
                            .ToListAsync();

                        return comments;
                    }
                    else
                    {
                        // No rating found for the book
                        return new List<RatingRecord>();
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving comments for book ID {bookId}: {e.Message}");
            }
        }

        public async Task<List<Reply>> GetRepliesForRatingRecordId(Guid ratingRecordId) //SONDB 
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    return await context.Replies
                        .Where(r => r.RatingRecordId == ratingRecordId)
                        .ToListAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving comments: {e.Message}");
            }
        }

        public async Task<Reply> GetReplyByIdAsync(Guid replyId) //SONDB 
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    return await context.Replies.FirstOrDefaultAsync(p => p.ReplyId == replyId);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> AddNewReplyAsync(Reply reply) //SONDB 
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Replies.Add(reply);
                    return await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while saving changes to the database:");
                Console.WriteLine("Inner Exception Message: " + e.InnerException?.Message);
                Console.WriteLine("Stack Trace: " + e.InnerException?.StackTrace);

                throw new Exception("An error occurred while saving changes to the database.", e);
            }
        }

        public async Task<int> UpdateReplyAsync(Reply reply) //SONDB 
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Replies.Update(reply);
                    return await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<int> DeleteReplyByIdAsync(Guid replyId) //SONDB 
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var reply = await GetReplyByIdAsync(replyId);
                    if (reply != null)
                    {
                        context.Replies.Remove(reply);
                    }
                    return await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public async Task<double> CalculateOverallReplyPercentageForAgencyAsync(Guid agencyId)
        {
            // Get the list of books owned by the agency asynchronously
            var books = await _context.Inventories
                .Where(i => i.AgencyId == agencyId)
                .Select(i => i.ProductId)
                .ToListAsync();

            int totalReviews = 0;
            int repliedReviews = 0;

            foreach (var bookId in books)
            {
                // Get the ratingId of the book asynchronously
                Guid? ratingId = await _context.Books
                    .Where(p => p.ProductId == bookId)
                    .Select(p => p.RatingId)
                    .FirstOrDefaultAsync();

                if (ratingId == null)
                {
                    // If the book doesn't have a rating, skip it
                    continue;
                }

                // Get the total number of reviews for the specified ratingId asynchronously
                totalReviews += await _context.RatingRecords
                    .CountAsync(rr => rr.RatingId == ratingId);

                // Get the number of reviews that have been replied to by the agency asynchronously
                repliedReviews += await _context.Replies
                    .Where(reply => reply.RatingRecord != null && reply.RatingRecord.RatingId == ratingId && reply.AgencyId == agencyId)
                    .CountAsync();
            }

            // Calculate the overall reply percentage
            double overallReplyPercentage = totalReviews == 0 ? 0 : (double)repliedReviews / totalReviews * 100;

            return overallReplyPercentage;
        }

        public async Task<List<RatingRecord>> GetReviewsByAgencyId(Guid agencyId, ReviewSearchCriteria searchCriteria) //SONDB
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var query = context.Inventories
                        .Where(i => i.AgencyId == agencyId)
                        .Select(i => i.ProductId);

                    var ratingIdsQuery = context.Books
                        .Where(book => query.Contains(book.ProductId))
                        .Select(book => book.RatingId);

                    if (!string.IsNullOrEmpty(searchCriteria.BookName))
                    {
                        ratingIdsQuery = context.Books
                            .Where(book => query.Contains(book.ProductId) && book.Name.Contains(searchCriteria.BookName))
                            .Select(book => book.RatingId);
                    }

                    var ratingIds = await ratingIdsQuery.ToListAsync();

                    var reviewsQuery = context.RatingRecords
                        .Where(rr => ratingIds.Contains(rr.RatingId) && !string.IsNullOrEmpty(rr.Comment));
                    if (!string.IsNullOrEmpty(searchCriteria.RecentDays))
                    {
                        int recentDays = int.Parse(searchCriteria.RecentDays); // Convert string to int
                        var thresholdDate = DateTime.UtcNow.AddDays(recentDays * -1);
                        reviewsQuery = reviewsQuery.Where(rr => rr.CreatedDate >= thresholdDate);
                    }
                    // Filter by rating point if provided
                    if (!string.IsNullOrEmpty(searchCriteria.RatingPoint) && int.TryParse(searchCriteria.RatingPoint, out int ratingPoint) && ratingPoint >= 0)
                    {
                        reviewsQuery = reviewsQuery.Where(rr => rr.RatingPoint == ratingPoint);
                    }

                    if (searchCriteria.HasReplied == "1")
                    {
                        // Return reviews with replies
                        reviewsQuery = reviewsQuery.Where(rr => context.Replies.Any(r => r.RatingRecordId == rr.RatingRecordId));
                    }
                    else if (searchCriteria.HasReplied == null)
                    {
                        // Return all books without filtering based on replies
                    }
                    else
                    {
                        // Return reviews without replies
                        reviewsQuery = reviewsQuery.Where(rr => !context.Replies.Any(r => r.RatingRecordId == rr.RatingRecordId));
                    }







                    var reviews = await reviewsQuery.ToListAsync();

                    return reviews;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while retrieving reviews for agency ID {agencyId}. Error Details: {e.Message}. Stack Trace: {e.StackTrace}");
            }
        }

    }
}


