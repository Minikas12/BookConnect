using APIs.Services.Interfaces;
using BusinessObjects.Models;
using BusinessObjects.Models.Utils;
using DataAccess.DAO;
using DataAccess.DAO.E_com;

namespace APIs.Services
{
	public class SocialMediaService: ISocialMediaService
	{
        private readonly UserTargetedCategoryDAO _utcDAO;
        private readonly PostDAO _postDAO;
		public SocialMediaService()
		{
            _postDAO = new PostDAO();
            _utcDAO = new UserTargetedCategoryDAO();
		}

        public async Task<int> AddUserTargetCategoryByListAsync(List<Guid> cateIdList, Guid userId)
        => await _utcDAO.AddUserTargetCategoryByListAsync(cateIdList, userId);

        public async Task<List<Category>> GetAllSavedPostTagsAsync(Guid userId)
        => await _postDAO.GetAllSavedPostTagsAsync(userId);

        public async Task<List<UserTargetedCategory>> GetUTCByUserIdAsync(Guid userId)
        => await _utcDAO.GetUTCByUserIdAsync(userId);

        public async Task<bool> IsAlreadyTargetedAsync(Guid userId, Guid tagId)
        => await _utcDAO.IsAlreadyTargetedAsync(userId, tagId);

        public async Task<int> RemoveUserTargetedCategoryAsync(Guid userId, Guid cateId)
        => await _utcDAO.RemoveUserTargetedCategoryAsync(userId, cateId);

      
    }
}

