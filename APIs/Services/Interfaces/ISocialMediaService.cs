using System;
using BusinessObjects.Models;
using BusinessObjects.Models.Utils;

namespace APIs.Services.Interfaces
{
	public interface ISocialMediaService
	{
        Task<int> AddUserTargetCategoryByListAsync(List<Guid> cateIdList, Guid userId);
        Task<List<UserTargetedCategory>> GetUTCByUserIdAsync(Guid userId);
        Task<int> RemoveUserTargetedCategoryAsync(Guid userId, Guid cateId);
        Task<bool> IsAlreadyTargetedAsync(Guid userId, Guid tagId);
        Task<List<Category>> GetAllSavedPostTagsAsync(Guid userId);

    }
}

