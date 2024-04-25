using BusinessObjects;
using BusinessObjects.Models.E_com.Trading;
using BusinessObjects.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
	public class UserTargetedCategoryDAO
	{
		private readonly AppDbContext _context;
		public UserTargetedCategoryDAO()
		{
			_context = new AppDbContext();
		}

		public async Task<int> AddUserTargetCategoryByListAsync(List<Guid> cateIdList, Guid userId)
		{
			foreach(Guid c in cateIdList)
			{
				await _context.UserTargetedCategories.AddAsync(new UserTargetedCategory()
				{
					Id = Guid.NewGuid(),
					UserId = userId,
					CategoryId = c,
				});
			}
			return await _context.SaveChangesAsync();
		}

		public async Task<List<UserTargetedCategory>> GetUTCByUserIdAsync(Guid userId)
		=> await _context.UserTargetedCategories.Where(r => r.UserId == userId).ToListAsync();

		public async Task<bool> IsAlreadyTargetedAsync(Guid userId, Guid tagId)
		=> await _context.UserTargetedCategories.AnyAsync(r => r.UserId == userId && r.CategoryId == tagId);

		public async Task<int> RemoveUserTargetedCategoryAsync(Guid userId, Guid cateId)
		{
			UserTargetedCategory? record 
		    = await _context.UserTargetedCategories.SingleOrDefaultAsync(r => r.UserId == userId && r.CategoryId == cateId);
			if(record != null)
			{
              _context.UserTargetedCategories.Remove(record);
            } return await _context.SaveChangesAsync();
        }

	}
}

