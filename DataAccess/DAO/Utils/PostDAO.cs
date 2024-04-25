using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Social;
using BusinessObjects.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO.E_com
{
    public class PostDAO
    {
        private readonly AppDbContext _context;
        public PostDAO()
        {
            _context = new AppDbContext();
        }

        public async Task<List<Post>> GetAllPostAsync()
        {
            var posts = await _context.Posts.ToListAsync();
            return posts;
        }
        public async Task<List<Post>> GetPostsByUserIdAsync(Guid userId)
        => await _context.Posts.Where(p => p.UserId == userId).ToListAsync();

        public async Task<List<Post>> GetPostInterestedInByUserIdAsync(Guid userId)
        {
            var query = from p in _context.Posts
                        join pi in _context.PostInteresters
                        on p.PostId equals pi.PostId
                        where pi.InteresterId == userId
                        select p;
            return await query.ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(Guid postId) => await _context.Posts.SingleOrDefaultAsync(p => p.PostId == postId);

        public async Task<Post?> GetPostByTradeDetailsId(Guid tradeDetailsId)
        {
            var query = await (from td in _context.TradeDetails
                        join lr in _context.PostInteresters on td.LockedRecordId equals lr.PostInterestId
                        join p in _context.Posts on lr.PostId equals p.PostId
                        where td.TradeDetailId == tradeDetailsId
                        select p).SingleOrDefaultAsync();
            return query;
        }

        public async Task<int> AddNewPostAsync(Post post)
        {
            await _context.Posts.AddAsync(post);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdatePostAsync(Post post)
        {
            try
            {
                var existingPost = await _context.Posts.FindAsync(post.PostId);

                if (existingPost == null)
                {
                    return 0;
                }

                _context.Entry(existingPost).CurrentValues.SetValues(post);
                _context.Entry(existingPost).Property(p => p.CreatedAt).IsModified = false;

                foreach (var property in _context.Entry(existingPost).Properties)
                {
                    if (property.IsModified && property.CurrentValue == null)
                    {
                        property.IsModified = false;
                    }
                }

                return await _context.SaveChangesAsync();
            }
            catch(DbUpdateException e)
            {
                throw e.InnerException;
            }
           
        }
        //var existingPost = await _context.Posts.FindAsync(post.PostId);
        //if (existingPost != null)
        //{
        //    existingPost = post;
        //    return await _context.SaveChangesAsync();
        //} return 0;

        public async Task<int> DeletePostByIdAsync(Guid postId)
        {
            if(await IsTradePostAsync(postId))
            {
                var postInteresters = _context.PostInteresters.Where(pi => pi.PostId == postId).ToList();
                _context.PostInteresters.RemoveRange(postInteresters);
            }
            Post? post = await GetPostByIdAsync(postId);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<string> GetOldImgPathAsync(Guid postId)
        {
            Post? post = await _context.Posts.SingleOrDefaultAsync(c => c.PostId == postId);
            string result = (post != null && post.ImageDir != null) ?
            post.ImageDir : "";
            return result;
        }

        public async Task<string> GetOldVideoPathAsync(Guid postId)
        {
            Post? post = await _context.Posts.SingleOrDefaultAsync(c => c.PostId == postId);
            string result = (post != null && post.VideoDir != null) ?
            post.VideoDir : "";
            return result;
        }

        public async Task<bool> IsTradePostAsync(Guid postId)
        {
            Post? record = await _context.Posts.SingleOrDefaultAsync(p => p.PostId == postId);
            bool result = (record != null) ? record.IsTradePost : false;
            return result;
        }

        public async Task<bool> IsLockedPostAsync(Guid postId)
        {
            Post? record = await _context.Posts.SingleOrDefaultAsync(p => p.PostId == postId);
            bool result = (record != null) ? record.IsLock : false;
            return result;
        }
        //For trade
        public async Task<int> SetLockPostAsync(bool choice, Guid postId)
        {
            Post? post = await _context.Posts.SingleOrDefaultAsync(p => p.PostId == postId);
            if(post != null)
            {
                post.IsLock = choice;
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> IsPostOwnerAsync(Guid postId, Guid userId)
        => await _context.Posts.AnyAsync(p => p.PostId == postId && p.UserId == userId);

        public async Task<bool> IsPostExisted(Guid postId)
        => await _context.Posts.AnyAsync(p => p.PostId == postId);

        /*------------------------------------Post categories------------------------------------*/
        public async Task<int> AddPostToCategoriesAsync(Guid postId, List<Guid> cateIds)
        {
            foreach(Guid c in cateIds)
            {
                await _context.CategoryLists.AddAsync(new CategoryList()
                {
                    CategoryListId = Guid.NewGuid(),
                    CategoryId = c,
                    PostId = postId,
                });
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Category>> GetAllCategoryOfPostAsync(Guid postId)
        {
            var query = from c in _context.Categories
                        join cl in _context.CategoryLists on c.CateId equals cl.CategoryId
                        where cl.PostId == postId
                        select c;
            return await query.ToListAsync();
        }

        public async Task<int> RemovePostfromCategoryAsync(Guid postId, Guid cateId)
        {
            CategoryList? record = await _context.CategoryLists.SingleOrDefaultAsync(r => r.CategoryId == cateId && r.PostId == postId);
            if(record != null)
            {
                _context.CategoryLists.Remove(record);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<List<Post>> GetPostByUserTargetedCategoriesAsync(Guid userId)
        {
            var query = from p in _context.Posts
                        join cp in _context.CategoryLists on p.PostId equals cp.PostId
                        join utc in _context.UserTargetedCategories on cp.CategoryId equals utc.CategoryId
                        where utc.UserId == userId
                        select p;
            return await query.ToListAsync();
        }

        ///*------------------------------------User Saved Post------------------------------------*/
        public async Task<int> AddNewUserSavedPostAsync(Guid userId, Guid postId)
        {
            await _context.UserSavedPosts.AddAsync(new UserSavedPost
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId
            });
           return await _context.SaveChangesAsync();
        }

        public async Task<List<Post>> GetUserSavedPostAsync(Guid userId)
        {
            var query = from p in _context.Posts
                        join usp in _context.UserSavedPosts
                        on p.PostId equals usp.PostId
                        where usp.UserId == userId
                        select p;
            return await query.ToListAsync();
        }

        public async Task<int> RemoveUserSavedPostAsync(Guid userId, Guid postId)
        {
            UserSavedPost? record
            = await _context.UserSavedPosts.SingleOrDefaultAsync(r => r.UserId == userId && r.PostId == postId);
            if(record != null)
            {
                _context.UserSavedPosts.Remove(record);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> IsAlreadySavedAsync(Guid userId, Guid postId)
        => await _context.UserSavedPosts.AnyAsync(r => r.UserId == userId && r.PostId == postId);

        public async Task<List<Category>> GetAllSavedPostTagsAsync(Guid userId) {
            var query = (from usp in _context.UserSavedPosts
                        join cl in _context.CategoryLists on usp.PostId equals cl.PostId 
                        join c in _context.Categories on cl.CategoryId equals c.CateId
                        where usp.UserId == userId && c.IsSocialTag == true
                        select c).Distinct();
            return await query.ToListAsync();
        }
        //public async Task<int> BanPostAsync(Guid postId)
        //{
        //    try
        //    {
        //        var post = await _context.Posts.FindAsync(postId);

        //        if (post == null)
        //        {
        //            // Post not found
        //            return 0;
        //        }

        //        post.IsBanned = true;

        //        return await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException e)
        //    {
        //        // Handle any exceptions
        //        throw e.InnerException;
        //    }
        //}
    }
}
