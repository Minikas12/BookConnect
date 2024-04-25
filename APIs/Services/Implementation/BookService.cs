using APIs.Services.Interfaces;
using BusinessObjects.Models;
using DataAccess.DAO;
using BusinessObjects.DTO;
using BusinessObjects;
using DataAccess.DAO.Ecom;
using BusinessObjects.Models.Utils;
using BusinessObjects.Models.Ecom.Rating;
using APIs.Utils.Paging;

namespace APIs.Services
{
    public class BookService : IBookService
    {

        public PagedList<Book> SearchBooks(BookSearchCriteria criteria, PagingParams param)  //SONDB 
        {
            return PagedList<Book>.ToPagedList(new BookDAO().SearchBooks(criteria).OrderBy(c => c.Name).AsQueryable(), param.PageNumber, param.PageSize);
        }

        public PagedList<Book> GetBooksSoldByQuantityDescendingWithinTime(PagingParams param) //SONDB 
        {
            return PagedList<Book>.ToPagedList(new BookDAO().GetBooksSoldByQuantityDescendingWithinTime().OrderBy(c => c.Name).AsQueryable(), param.PageNumber, param.PageSize);
        }

        public List<SEODTO> ListSEO(string searchTerm)
        {
            List<SEODTO> result = new List<SEODTO>();
            try
            {
                using (var context = new AppDbContext()) // Assuming you use Entity Framework
                {
                    var searchResult = context.Books.Where(b => b.Name.Contains(searchTerm)).ToList();

                    foreach (Book book in searchResult)
                    {
                        result.Add(new SEODTO
                        {
                            BookId = book.ProductId,
                            Title = book.Name,
                        });
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        //-----------------------------------DATDQ------------------------------------------------// 

        public PagedList<Book> GetAllBook(PagingParams param)
        {
            {
                return PagedList<Book>.ToPagedList(new BookDAO().GetAllBook().OrderBy(b => b.Name).AsQueryable(), param.PageNumber, param.PageSize);
            }
        }


        public Book GetBookById(Guid bookId) => new BookDAO().GetBookById(bookId);


        public Book GetBookByName(string name) => new BookDAO().GetBookByName(name);


        public int AddNewBook(Book book) => new BookDAO().AddNewBook(book);


        public int UpdateBook(Book book) => new BookDAO().UpdateBook(book);
        public string GetOldBookImgPath(Guid productId) => new BookDAO().GetOldBookImgPath(productId);
        public string GetOldBackgroundImgPath(Guid productId) => new BookDAO().GetOldBackgroundImgPath(productId);

        public int AddNewRating(Rating rating) => new RatingDAO().AddNewRating(rating);

        public int DeleteBook(Guid bookId) => new BookDAO().DeleteBook(bookId);

        public BookDetailsDTO GetBookDetailsById(Guid bookId) => new BookDAO().GetBookDetailsById(bookId);

        public List<Book> GetBookListById(List<Guid> bookIds) => new BookDAO().GetBookListById(bookIds);
        public Guid GetRatingId(Guid productId) => new RatingDAO().GetRatingId(productId);
        public Guid GetBestSellerProductIdByNumberOfBookSold(Guid Id) => new BookDAO().GetBestSellerProductIdByNumberOfBookSold(Id);
        public Guid GetBestSellerProductIdByNumberOfUnitSold(Guid Id) => new BookDAO().GetBestSellerProductIdByNumberOfUnitSold(Id);
        public Guid GetBestSellerProductIdByRevenue(Guid Id) => new BookDAO().GetBestSellerProductIdByRevenue(Id);
        OrderDAO o = new OrderDAO();
        public PagedList<BookDetailsDTO> GetBestSellerProductIdByNumberOfBookSold(Guid agencyId, PagingParams param)
        {
            try
            {
                // Initialize instances of OrderDAO and BookDAO
                OrderDAO o = new OrderDAO();
                BookDAO b = new BookDAO();

                // Get all books in inventory for the given agencyId
                var books = b.GetAllBookInInventory(agencyId);

                // Order the books by the number of books sold (descending)
                var orderedBooks = books.OrderByDescending(b => o.GetNumberOfBooksSoldByProductId(b.ProductId));

                // Convert the ordered books to a paged list using the provided paging parameters
                var pagedList = PagedList<BookDetailsDTO>.ToPagedList(orderedBooks.AsQueryable(), param.PageNumber, param.PageSize);

                return pagedList;
            }
            catch (Exception e)
            {
                // Handle exceptions appropriately
                throw new Exception("Error occurred while fetching best-selling products.", e);
            }
        }

        public PagedList<BookDetailsDTO> GetBestSellerProductIdByNumberOfUnitSold(Guid agencyId, PagingParams param)
        {
            try
            {
                // Initialize instances of OrderDAO and BookDAO
                OrderDAO o = new OrderDAO();
                BookDAO b = new BookDAO();

                // Get all books in inventory for the given agencyId
                var books = b.GetAllBookInInventory(agencyId);

                // Order the books by the number of books sold (descending)
                var orderedBooks = books.OrderByDescending(b => o.GetNumberOfUnitSoldByProductId(b.ProductId));

                // Convert the ordered books to a paged list using the provided paging parameters
                var pagedList = PagedList<BookDetailsDTO>.ToPagedList(orderedBooks.AsQueryable(), param.PageNumber, param.PageSize);

                return pagedList;
            }
            catch (Exception e)
            {
                // Handle exceptions appropriately
                throw new Exception("Error occurred while fetching best-selling products.", e);
            }
        }

        public PagedList<BookDetailsDTO> GetBestSellerProductIdByRevenue(Guid agencyId, PagingParams param)
        {
            try
            {
                // Initialize instances of OrderDAO and BookDAO
                OrderDAO o = new OrderDAO();
                BookDAO b = new BookDAO();

                // Get all books in inventory for the given agencyId
                var books = b.GetAllBookInInventory(agencyId);

                // Order the books by the number of books sold (descending)
                var orderedBooks = books.OrderByDescending(b => o.GetRevenueByProductId(b.ProductId));

                // Convert the ordered books to a paged list using the provided paging parameters
                var pagedList = PagedList<BookDetailsDTO>.ToPagedList(orderedBooks.AsQueryable(), param.PageNumber, param.PageSize);

                return pagedList;
            }
            catch (Exception e)
            {
                // Handle exceptions appropriately
                throw new Exception("Error occurred while fetching best-selling products.", e);
            }
        }

        //-----------------------------------Book category------------------------------------------------//

        public int AddBookToCategory(CategoryList categoryList) => new BookDAO().AddBookToCategory(categoryList);


        public bool IsBookAlreadyInCate(Guid bookId, Guid cateId) => new BookDAO().IsBookAlreadyInCate(bookId, cateId);


        public int RemoveBookFromCate(Guid bookId, Guid cateId) => new BookDAO().RemoveBookFromCate(bookId, cateId);

        public List<Category> GetAllCategoryOfBook(Guid productId) => new BookDAO().GetAllCategoryOfBook(productId);
        public PagedList<Category> GetAllCategoryOfBook(Guid bookId, PagingParams param)
        {
            return PagedList<Category>.ToPagedList(new BookDAO().GetAllCategoryOfBook(bookId).OrderBy(c => c.CateName).AsQueryable(), param.PageNumber, param.PageSize);
        }

        public List<string> GetAllCategoryNameOfBook(Guid productId) => new BookDAO().GetAllCategoryNameOfBook(productId);

        //-----------------------------------Book Group------------------------------------------------//

        public int AddBookToBookGroup(ListBookGroup bookGroup) => new BookDAO().AddBookToBookGroup(bookGroup);


        public bool IsBookAlreadyInBookGroup(Guid productId, Guid bookGroupIds) => new BookDAO().IsBookAlreadyInBookGroup(productId, bookGroupIds);
        public bool IsBookGroupValid(Guid agencyId, Guid id) => new BookDAO().IsBookGroupValid(agencyId, id);


        public int RemoveBookFromBookGroup(Guid productId, Guid bookGroupIds) => new BookDAO().RemoveBookFromBookGroup(productId, bookGroupIds);


        public PagedList<BookGroup> GetAllBookGroupOfBook(Guid productId, PagingParams param)
        {
            return PagedList<BookGroup>.ToPagedList(new BookDAO().GetAllBookGroupOfBook(productId).OrderBy(bg => bg.BookGroupName).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public PagedList<BookDetailsDTO> GetAllBookOfBookGroup(Guid bookGroupId, PagingParams param)
        {
            return PagedList<BookDetailsDTO>.ToPagedList(new BookDAO().GetAllBookOfBookGroup(bookGroupId).OrderBy(b => b.Name).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public PagedList<BookDetailsDTO> GetBookOfBookGroupByName(Guid bookGroupId, string name, PagingParams param)
        {
            return PagedList<BookDetailsDTO>.ToPagedList(new BookDAO().GetBookOfBookGroupByName(bookGroupId, name).OrderBy(b => b.Name).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public PagedList<BookGroup> GetAllBookGroupsByAgency(Guid agencyId, PagingParams param)
        {
            return PagedList<BookGroup>.ToPagedList(new BookDAO().GetAllBookGroupsByAgency(agencyId).OrderBy(bg => bg.BookGroupName).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public PagedList<BookGroup> GetBookGroupsByAgencyAndName(Guid agencyId, string name, PagingParams param)
        {
            return PagedList<BookGroup>.ToPagedList(new BookDAO().GetBookGroupsByAgencyAndName(agencyId, name).OrderBy(bg => bg.BookGroupName).AsQueryable(), param.PageNumber, param.PageSize);
        }

        public PagedList<BookDetailsDTO> GetAllBookOfBookGroupByBookId(Guid productId, PagingParams param)  //New
        {
            return PagedList<BookDetailsDTO>.ToPagedList(new BookDAO().GetAllBookOfBookGroupByBookId(productId).OrderBy(b => b.Name).AsQueryable(), param.PageNumber, param.PageSize);
        }
        //-----------------------------------Inventory ------------------------------------------------//

        public PagedList<BookDetailsDTO> SearchBookInInventory(Guid agencyId, string name, string author, string category, string type, DateTime startDate, DateTime endDate, PagingParams param) //Update
        {
            return PagedList<BookDetailsDTO>.ToPagedList(new BookDAO().SearchBookInInventory(agencyId, name, author, category, type, startDate, endDate).OrderBy(b => b.Name).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public int RemoveBookFromInventory(Guid productId) => new BookDAO().RemoveBookFromInventory(productId);
        public bool IsBookOwnedByAgency(Guid productId, Guid agencyId) => new BookDAO().IsBookOwnedByAgency(productId, agencyId);

        public PagedList<BookDetailsDTO> GetAllBookInInventory(Guid agencyId, PagingParams param) //Update
        {
            return PagedList<BookDetailsDTO>.ToPagedList(new BookDAO().GetAllBookInInventory(agencyId).OrderBy(b => b.Name).AsQueryable(), param.PageNumber, param.PageSize);
        }
        public int GetTotalQuantityByAgencyId(Guid id) => new BookDAO().GetTotalQuantityByAgencyId(id);

        //New ----------------------------------------------------------------------------------------------------------------------//
        public List<Book> GetAllBookInInventoryByType(Guid agencyId, string type) => new BookDAO().GetAllBookInInventoryByType(agencyId, type);
        public List<Book> GetAllBookInInventoryByCategory(Guid agencyId, string cate) => new BookDAO().GetAllBookInInventoryByCategory(agencyId, cate);
        public List<Book> GetAllBookInInvent(Guid agencyId) => new BookDAO().GetAllBookInInvent(agencyId);

        //-----------------------------------END DATDQ------------------------------------------------//

    }
}
