using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAO
{
	public class CategoryDAO
	{
        private readonly AppDbContext _context;
        public CategoryDAO()
        {
            _context = new AppDbContext();
        }
        public async Task<int> AddCategoryAsync(Category cate)
		{
		    await _context.Categories.AddAsync(cate);
		    return await _context.SaveChangesAsync();
		}

        public async Task<List<Category>> GetAllCategoryAsync()
        => await _context.Categories.ToListAsync();  

        public async Task<int> DeleteCategoryByIdAsync(Guid cateId)
        {
		    Category? cate = await _context.Categories.SingleOrDefaultAsync(c => c.CateId == cateId);

            if (cate != null)
			{
              _context.Categories.Remove(cate);
            }
             return await _context.SaveChangesAsync();
        
        }

		public async Task<Category?> GetCategoryByIdAsync(Guid cateId)
        => await _context.Categories.SingleOrDefaultAsync(c => c.CateId == cateId);

        public async Task<string> GetOldImgPathAsync(Guid cateId)
        {
           Category? cate = await _context.Categories.SingleOrDefaultAsync(c => c.CateId == cateId);
           return (cate != null && cate.ImageDir != null) ?
           cate.ImageDir : "";
        }

        public async Task<Guid?> GetSocialTagIdByName(string tagName)
        {
            Category? cate = await _context.Categories.SingleOrDefaultAsync(t => t.CateName == tagName);
            return (cate != null) ? cate.CateId : null;
        }

        public async Task<List<Category>> GetCategoryByNameAsync(string inputString)
        => await _context.Categories.Where(c => c.CateName.Contains(inputString)).ToListAsync();

        //-------------------------------- DATDQ ------------------------------------\\

        public int AddCategory(Category cate)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Categories.Add(cate);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<Category> GetAllCategory()
        {
            try
            {
                List<Category> result = new List<Category>();
                using (var context = new AppDbContext())
                {
                    if (context.Categories.Any())
                    {
                        result = context.Categories.ToList();
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int UpdateCategory(Category cate)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Categories.Update(cate);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int DeleteCategoryById(Guid cateId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Category? cate = context.Categories.Where(c => c.CateId == cateId).SingleOrDefault();

                    if (cate != null)
                    {
                        context.Categories.Remove(cate);
                    }
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int DeleteCategoryList(Guid bookId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    string deleteListCateQuery = $"delete from CategoryLists where ProductId = @productId";
                    // Execute the second query to delete the inventory entry
                    int result = context.Database.ExecuteSqlRaw(deleteListCateQuery,
                        new SqlParameter("@productId", bookId));
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Category GetCategoryById(Guid cateId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Category? result = context.Categories.Where(c => c.CateId == cateId).SingleOrDefault();
                    if (result != null)
                    {
                        return result;
                    }
                    return new Category();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public Guid GetCateIdByName(string name)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Category cate = context.Categories.FirstOrDefault(c => c.CateName == name);

                    return cate?.CateId ?? Guid.Empty;
                }
            }
            catch (Exception e)
            {
                // Log the exception and handle it accordingly
                throw new Exception("Error occurred while fetching agency ID: " + e.Message);
            }
        }

        public string GetOldImgPath(Guid cateId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Category? cate = context.Categories.Where(c => c.CateId == cateId).SingleOrDefault();
                    string result = (cate != null && cate.ImageDir != null) ?
                        cate.ImageDir : "";
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<Category> GetCategoryByName(string inputString)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<Category> result = new List<Category>();
                    var matchedCates = context.Categories
                    .Where(c => c.CateName.Contains(inputString))
                    .ToList();
                    if (matchedCates.Count > 0)
                    {
                        result = matchedCates;
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //-------------------------------- END DATDQ ------------------------------------\\

    }
}

