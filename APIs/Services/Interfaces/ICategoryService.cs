using APIs.Utils.Paging;
using BusinessObjects.Models;

namespace APIs.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<int> AddCategoryAsync(Category cate);
        Task<PagedList<Category>> GetAllCategoryAsync(PagingParams param);
        Task<int> DeleteCategoryByIdAsync(Guid cateId);
        Task<Category?> GetCategoryByIdAsync(Guid cateId);
        Task<string> GetOldImgPathAsync(Guid cateId);
        Task<Guid?> GetSocialTagIdByName(string tagName);
        Task<PagedList<Category>> GetCategoryByNameAsync(string inputString, PagingParams param);
        //------------------------------------- DATDQ ---------------------------\\
        int AddCategory(Category cate);
        PagedList<Category> GetAllCategory(PagingParams param);
        int UpdateCategory(Category cate);
        int DeleteCategory(Guid cateId);
        int DeleteCategoryList(Guid bookId);
        Category GetCategoryById(Guid cateId);
        Guid GetCateIdByName(string name);
        string GetOldImgPath(Guid cateId);
        PagedList<Category> GetCategoryByName(string inputString, PagingParams param);
        //------------------------------------- END DATDQ -----------------------\\
    }
}

