using BusinessObjects.Models;
using BusinessObjects.DTO;
using BusinessObjects.Models.Utils;
using BusinessObjects.Models.Ecom.Rating;
using APIs.Utils.Paging;

namespace APIs.Services.Interfaces
{
    public interface IBookService
    {
        PagedList<Book> GetAllBook(PagingParams param);
        public Book GetBookByName(string name);
        List<SEODTO> ListSEO(string searchTerm);
        //public Guid GetBestSellerProductId(Guid Id);
        PagedList<Book> SearchBooks(BookSearchCriteria criteria, PagingParams param); //SONDB 
        PagedList<Book> GetBooksSoldByQuantityDescendingWithinTime(PagingParams param);  //SONDB 

        //-----------------------------------DATDQ------------------------------------------------//
        public BookDetailsDTO GetBookDetailsById(Guid bookId);
        public Book GetBookById(Guid bookId);
        public int AddNewRating(Rating rating);
        public int AddNewBook(Book book);
        public int UpdateBook(Book book);
        string GetOldBookImgPath(Guid productId);
        string GetOldBackgroundImgPath(Guid productId);
        public int DeleteBook(Guid bookId);
        public List<Book> GetBookListById(List<Guid> bookId);
        public Guid GetRatingId(Guid productId);
        public Guid GetBestSellerProductIdByNumberOfBookSold(Guid Id);
        public Guid GetBestSellerProductIdByNumberOfUnitSold(Guid Id);
        public Guid GetBestSellerProductIdByRevenue(Guid Id);
        PagedList<BookDetailsDTO> GetBestSellerProductIdByNumberOfBookSold(Guid agencyId, PagingParams param);
        PagedList<BookDetailsDTO> GetBestSellerProductIdByNumberOfUnitSold(Guid agencyId, PagingParams param);
        PagedList<BookDetailsDTO> GetBestSellerProductIdByRevenue(Guid agencyId, PagingParams param);


        //-----------------------------------Book category------------------------------------------------//
        int AddBookToCategory(CategoryList categoryList);
        bool IsBookAlreadyInCate(Guid bookId, Guid cateId);
        int RemoveBookFromCate(Guid bookId, Guid cateId);
        PagedList<Category> GetAllCategoryOfBook(Guid bookId, PagingParams param);
        List<string> GetAllCategoryNameOfBook(Guid bookId);
        List<Category> GetAllCategoryOfBook(Guid productId);

        //-----------------------------------Book listing------------------------------------------------//
        int AddBookToBookGroup(ListBookGroup bookGroup);
        bool IsBookAlreadyInBookGroup(Guid productId, Guid bookGroupId);
        bool IsBookGroupValid(Guid agencyId, Guid id);
        int RemoveBookFromBookGroup(Guid productId, Guid bookGroupId);
        PagedList<BookGroup> GetAllBookGroupOfBook(Guid productId, PagingParams param);
        PagedList<BookGroup> GetBookGroupsByAgencyAndName(Guid agencyId, string name, PagingParams param);
        PagedList<BookDetailsDTO> GetAllBookOfBookGroup(Guid bookGroupId, PagingParams param);
        PagedList<BookDetailsDTO> GetBookOfBookGroupByName(Guid bookGroupId, string name, PagingParams param);
        PagedList<BookGroup> GetAllBookGroupsByAgency(Guid agencyId, PagingParams param);
        PagedList<BookDetailsDTO> GetAllBookOfBookGroupByBookId(Guid productId, PagingParams param);

        //-----------------------------------Inventory------------------------------------------------//
        PagedList<BookDetailsDTO> SearchBookInInventory(Guid agencyId, string name, string author, string category, string type, DateTime startDate, DateTime endDate, PagingParams param); //Update
        PagedList<BookDetailsDTO> GetAllBookInInventory(Guid agencyId, PagingParams param);
        List<Book> GetAllBookInInvent(Guid agencyId);
        int RemoveBookFromInventory(Guid productId);
        bool IsBookOwnedByAgency(Guid productId, Guid agencyId);
        int GetTotalQuantityByAgencyId(Guid id);
        //New ----------------------------------------------------------------------------------------------//
        List<Book> GetAllBookInInventoryByType(Guid agencyId, string type);
        List<Book> GetAllBookInInventoryByCategory(Guid agencyId, string cate);

        //-----------------------------------END DATDQ------------------------------------------------//



    }
}


