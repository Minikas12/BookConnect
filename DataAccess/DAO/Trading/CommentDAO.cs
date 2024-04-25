using BusinessObjects;
using BusinessObjects.DTO.Trading;
using BusinessObjects.Models.Trading;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO.Trading
{
    public class CommentDAO
    {
        private readonly AppDbContext _context;
        public CommentDAO()
        {
            _context = new AppDbContext();
        }
        //Get comment by post id
        public async Task<List<CommentDetailsDTO>> GetCommentByPostIdAsync(Guid postId)
        {
            var query = from c in _context.Comments
                        join pc in _context.CommentOfPosts on c.CommentId equals pc.CommentId
                        join u in _context.AppUsers on c.CommenterId equals u.UserId
                        where pc.PostId.Equals(postId)
                        select new CommentDetailsDTO
                        {
                            CommentId = c.CommentId,
                            CreateDate = c.CreateDate,
                            Content = c.Content,
                            PostId = pc.PostId,
                            CommenterId = c.CommenterId,
                            Username = u.Username,
                            AvatarDir = u.AvatarDir
                        };
            return await query.ToListAsync();
        }

        //Add Comment
        public async Task<int> AddCommentAsync(Comment comment)
        {
           await _context.Comments.AddAsync(comment);
           return await _context.SaveChangesAsync();
        }

        //Update Comment
        //public int UpdateComment(Comment comment)
        //{
        //    int result = 0;
        //    try
        //    {
        //        using (var context = new AppDbContext())
        //        {
        //            context.Update(comment);
        //            result = context.SaveChanges();
        //        }
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //}

        //Delete Comment by id
        public async Task<int> DeleteCommentByIdAsync(Guid commentId)
        {
                    Comment? comment = await _context.Comments.SingleOrDefaultAsync(c => c.CommentId == commentId);
                    if (comment != null)
                    {
                        _context.Comments.Remove(comment);
                    }
                    return await _context.SaveChangesAsync();
        }
    }
}
