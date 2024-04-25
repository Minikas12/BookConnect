using APIs.Services.Interfaces;
using APIs.Utils.Paging;
using BusinessObjects.Models;
using DataAccess.DAO;

namespace APIs.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly CategoryDAO _cateDAO;
        public CategoryService()
        {
            _cateDAO = new CategoryDAO();
        }
        public async Task<int> AddCategoryAsync(Category cate) => await _cateDAO.AddCategoryAsync(cate);

        public async Task<int> DeleteCategoryByIdAsync(Guid cateId) => await _cateDAO.DeleteCategoryByIdAsync(cateId);
        

        public async Task<PagedList<Category>> GetAllCategoryAsync(PagingParams param)
        {
            return PagedList<Category>.ToPagedList((await _cateDAO.GetAllCategoryAsync()).OrderBy(c => c.CateName).AsQueryable(), param.PageNumber, param.PageSize);
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid cateId) => await _cateDAO.GetCategoryByIdAsync(cateId);
        public async Task<string> GetOldImgPathAsync(Guid cateId) => await _cateDAO.GetOldImgPathAsync(cateId);

        public async Task<Guid?> GetSocialTagIdByName(string tagName) => await _cateDAO.GetSocialTagIdByName(tagName);

        public async Task<PagedList<Category>> GetCategoryByNameAsync(string inputString, PagingParams param)
        {
            return PagedList<Category>.ToPagedList((await _cateDAO.GetCategoryByNameAsync(inputString)).OrderBy(c => c.CateName).AsQueryable(), param.PageNumber, param.PageSize);
        }
        //----------------------------------- DATDQ ---------------------------------------------------------\\
        public int AddCategory(Category cate) => new CategoryDAO().AddCategory(cate);

        public int DeleteCategory(Guid cateId) => new CategoryDAO().DeleteCategoryById(cateId);
        public int DeleteCategoryList(Guid bookId) => new CategoryDAO().DeleteCategoryList(bookId);
        public PagedList<Category> GetAllCategory(PagingParams param)
        {
            return PagedList<Category>.ToPagedList(new CategoryDAO().GetAllCategory().OrderBy(c => c.CateName).AsQueryable(), param.PageNumber, param.PageSize);
        }

        public int UpdateCategory(Category cate) => new CategoryDAO().UpdateCategory(cate);

        public Category GetCategoryById(Guid cateId) => new CategoryDAO().GetCategoryById(cateId);
        public Guid GetCateIdByName(string name) => new CategoryDAO().GetCateIdByName(name);
        public string GetOldImgPath(Guid cateId) => new CategoryDAO().GetOldImgPath(cateId);

        public PagedList<Category> GetCategoryByName(string inputString, PagingParams param)
        {
            return PagedList<Category>.ToPagedList(new CategoryDAO().GetCategoryByName(inputString).OrderBy(c => c.CateName).AsQueryable(), param.PageNumber, param.PageSize);
        }
        //----------------------------------- END DATDQ -----------------------------------------------------\\
    }
}

