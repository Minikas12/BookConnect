using BusinessObjects.Models.Trading;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO.Trading
{
    public class PostInterestDAO
    {
        private readonly AppDbContext _context;
        public PostInterestDAO()
        {
            _context = new AppDbContext();
        }

        //Get postInterest by post id
        public async Task<List<PostInterester>> GetPostInterestByPostIdAsync(Guid postId)
        => await _context.PostInteresters.Where(p => p.PostId == postId).ToListAsync();

        //Add Interest
        public async Task<int> AddNewPostInterestAsync(PostInterester postInterest)
        {
            if(!await IsAlreadyInterestedAsync(postInterest.InteresterId, postInterest.PostId))
            {
                await _context.PostInteresters.AddAsync(postInterest);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> IsAlreadyInterestedAsync(Guid userId, Guid postId)
        {
            return await _context.PostInteresters.AnyAsync(p => p.InteresterId == userId && p.PostId == postId);
        }

        //Update PostInterest
        public int UpdatePostInterest(PostInterester postInterest)
        {
            _context.Update(postInterest);
            return _context.SaveChanges();
        }

        //Delete PostInterest by id
        public async Task<int> DeletePostInterestByIdAsync(Guid postInterestId)
        {
            PostInterester? postInterest = await _context.PostInteresters.SingleOrDefaultAsync(c => c.PostInterestId == postInterestId);
            if (postInterest != null)
            {
                _context.PostInteresters.Remove(postInterest);
            }
            return await _context.SaveChangesAsync();

        }

        public async Task<int> SetIsChosenAsync(bool choice, Guid postInterestId)
        {
            PostInterester? record = await _context.PostInteresters.SingleOrDefaultAsync(c => c.PostInterestId == postInterestId);
            if(record != null)
            {
                record.IsChosen = choice;
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<Guid?> GetLockedRecordIdAsync(Guid traderId, Guid postId)
        {
            PostInterester? record = await _context.PostInteresters.SingleOrDefaultAsync(p => p.PostId == postId && p.InteresterId == traderId);

            if (record != null)
            {
                return record.PostInterestId;
            }

            var query = from r in _context.PostInteresters
                        join p in _context.Posts on r.PostId equals p.PostId
                        where r.PostId.Equals(postId)
                        select new { r.PostInterestId };

            var result = await query.SingleOrDefaultAsync();

            return result?.PostInterestId;
        }

    }
}
