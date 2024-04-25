using System;
using BusinessObjects;
using BusinessObjects.Models.Trading;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO.Trading
{
	public class CommentOfPostDAO
	{
		private readonly AppDbContext _context;
		public CommentOfPostDAO()
        {
            _context = new AppDbContext();
        }

        public async Task<int> AddNewRecordAsync(Guid cmtId, Guid postId)
		{
			await _context.CommentOfPosts.AddAsync(new CommentOfPost
			{
				Id = Guid.NewGuid(),
				PostId = postId,
				CommentId = cmtId
			});
			return await _context.SaveChangesAsync();
		}

        public async Task<int> DeleteRecordAsync(Guid cmtId)
		{
			Comment? cmt = await _context.Comments.SingleOrDefaultAsync(c => c.CommentId == cmtId);
			if (cmt != null) _context.Remove(cmt);
			return await _context.SaveChangesAsync();
		}

    }
}

