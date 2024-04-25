using BusinessObjects;
using BusinessObjects.DTO;
using BusinessObjects.Models;
using BusinessObjects.Models.Ecom.Rating;
using BusinessObjects.Models.Utils;
using DataAccess.DAO.Ecom;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace DataAccess.DAO
{
    public class BookDAO
    {
        //Get all book
        public List<Book> GetAllBook()
        {

            List<Book> bookList = new List<Book>();
            try
            {
                using (var context = new AppDbContext())
                {
                    bookList = context.Books.ToList();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return bookList;
        }

        public List<Book> GetBooksSoldByQuantityDescendingWithinTime()
        {
            using (var context = new AppDbContext())
            {
                // Calculate the start date of the past week
                DateTime startDate = DateTime.Today.AddDays(-360);

                // Get all books sold within the past week along with their quantities
                var booksSoldByQuantity = context.Baskets
                    .Where(b => b.Order != null && b.Order.CreatedDate >= startDate) // Filter baskets with orders created within the past week
                    .GroupBy(b => b.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(b => b.Quantity) })
                    .ToList();

                var bookIds = booksSoldByQuantity.Select(b => b.ProductId).ToList();

                // Get all books within the bookIds list
                var books = context.Books
                    .Where(b => bookIds.Contains(b.ProductId))
                    .ToList();

                // Sort the books based on the total quantity sold
                books = books
                    .OrderByDescending(b => booksSoldByQuantity.FirstOrDefault(x => x.ProductId == b.ProductId)?.TotalQuantity ?? 0)
                    .ToList();

                return books;
            }
        }


        public decimal GetTotalAmount(List<ProductOptionDTO> dto)
        {
            try
            {
                decimal result = 0;
                using (var context = new AppDbContext())
                {
                    foreach (ProductOptionDTO c in dto)
                    {
                        Book? temp = context.Books.Where(b => b.ProductId == c.ProductId).FirstOrDefault();
                        if (temp != null)
                        {
                            result += temp.Price * c.Quantity;
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        //Get book details by id

        public BookDetailsDTO GetBookDetailsById(Guid bookId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var book = context.Books.Include(b => b.Rating).SingleOrDefault(b => b.ProductId == bookId);
                    if (book == null)
                    {
                        throw new Exception("Book not found");
                    }

                    var quantity = new AgencyDAO().GetProductQuantity(bookId);
                    var orderDAO = new OrderDAO();
                    var numberOfBooksSold = orderDAO.GetNumberOfBooksSoldByProductId(bookId);
                    var bookRevenue = orderDAO.GetRevenueByProductId(bookId);
                    var numberOfUnitSold = orderDAO.GetNumberOfUnitSoldByProductId(bookId);
                    var agency = new AgencyDAO().GetNameAndId(bookId);
                    var category = GetAllCategoryNameOfBook(bookId);
                    Guid owner = new AgencyDAO().GetOwnerIdFromBookId(bookId);
                    PointandIdDTO ratingInfo = new RatingDAO().GetRatingInfo(bookId);// Add ratingId  SONDB
                    return new BookDetailsDTO
                    {
                        ProductId = bookId,
                        Name = book.Name,
                        Description = book.Description,
                        Author = book.Author,
                        Price = book.Price,
                        CreatedDate = book.CreatedDate,
                        PublishDate = book.PublishDate,
                        Type = book.Type,
                        Stock = quantity,
                        NumberOfBookSold = numberOfBooksSold,
                        BookRevenue = bookRevenue,
                        NumberOfUnitSold = numberOfUnitSold,
                        BookImg = book.BookDir,
                        BackgroundImg = book.BackgroundDir,
                        Category = category,
                        Rating = ratingInfo.OverallRating, // Use null-conditional operator
                        RatingId = ratingInfo.RatingId,
                        UserId = owner,
                        AgencyId = agency.AgencyId,
                        AgencyName = agency.AgencyName
                    };
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while fetching book details", e);
            }
        }


        //Get book by id

        public Book GetBookById(Guid bookId)
        {
            Book? book = new Book();
            try
            {
                using (var context = new AppDbContext())
                {
                    book = context.Books.Where(b => b.ProductId == bookId).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            if (book != null) return book;
            else throw new NullReferenceException();
        }

        //Get book by name
        public Book GetBookByName(string name)
        {
            Book? book = new Book();
            try
            {
                using (var context = new AppDbContext())
                {
                    book = context.Books.Where(b => b.Name == name).FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            if (book != null) return book;
            else throw new NullReferenceException();
        }


        //Add new book
        public int AddNewBook(Book book)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Books.Add(book);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Modify book
        public int UpdateBook(Book book)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Books.Update(book);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetOldBookImgPath(Guid productId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Book? book = context.Books.Where(c => c.ProductId == productId).SingleOrDefault();
                    string result = (book != null && book.BookDir != null) ?
                        book.BookDir : "";
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetOldBackgroundImgPath(Guid productId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Book? book = context.Books.Where(c => c.ProductId == productId).SingleOrDefault();
                    string result = (book != null && book.BackgroundDir != null) ?
                        book.BackgroundDir : "";
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //Delete book by id
        public int DeleteBook(Guid bookId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    string deleteBookQuery = $"delete from Books where ProductId = @productId";
                    string deleteInvQuery = $"delete from Inventories where ProductId = @productId";
                    string deleteCateQuery = $"delete from CategoryLists where ProductId = @productId";
                    string deleteGroupQuery = $"delete from ListBookGroups where ProductId = @productId";
                    // Execute the second query to delete the inventory entry
                    int result = context.Database.ExecuteSqlRaw(deleteInvQuery,
                        new SqlParameter("@productId", bookId));
                    // Execute the third query to delete category entries
                    result += context.Database.ExecuteSqlRaw(deleteCateQuery,
                        new SqlParameter("@productId", bookId));
                    // Execute the first query to delete the book
                    result += context.Database.ExecuteSqlRaw(deleteBookQuery,
                        new SqlParameter("@productId", bookId));
                    result += context.Database.ExecuteSqlRaw(deleteGroupQuery,
                        new SqlParameter("@productId", bookId));
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<Book> GetBookListById(List<Guid> bookIds)
        {
            List<Book> listBooks = new List<Book>();
            try
            {
                using (var context = new AppDbContext())
                {
                    foreach (Guid i in bookIds)
                    {
                        listBooks.Add(context.Books.Where(b => b.ProductId == i).FirstOrDefault());

                    }
                    return listBooks;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        //-----------------------------------Book category------------------------------------------------//
        public int AddBookToCategory(CategoryList book)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.CategoryLists.Add(book);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<Category> GetAllCategoryOfBook(Guid bookId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<Category> result = new List<Category>();
                    List<CategoryList> listCate = context.CategoryLists.Where(o => o.ProductId == bookId).ToList();
                    foreach (CategoryList c in listCate)
                    {
                        var cate = context.Categories.Where(d => d.CateId == c.CategoryId).SingleOrDefault();
                        if (cate != null)
                        {
                            result.Add(cate);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<string> GetAllCategoryNameOfBook(Guid bookId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<string> result = new List<string>();
                    List<CategoryList> listCate = context.CategoryLists.Where(o => o.ProductId == bookId).ToList();
                    foreach (CategoryList c in listCate)
                    {
                        var cate = context.Categories.Where(d => d.CateId == c.CategoryId).SingleOrDefault();
                        if (cate != null)
                        {
                            result.Add(cate.CateName);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public bool IsBookAlreadyInCate(Guid bookId, Guid cateId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    return context.CategoryLists.Any(c => c.ProductId == bookId && c.CategoryId == cateId);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public int RemoveBookFromCate(Guid bookId, Guid cateId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    string insertQuery = $"delete from CategoryLists where CategoryId = @cateId and  BookId = @bookId";

                    int result = context.Database.ExecuteSqlRaw(insertQuery,
                        new SqlParameter("@cateId", cateId),
                        new SqlParameter("@bookId", bookId));
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //-----------------------------------Book group-----------------------------------------------//

        public int AddBookToBookGroup(ListBookGroup book)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.ListBookGroups.Add(book);
                    return context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<BookGroup> GetAllBookGroupOfBook(Guid productId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<BookGroup> result = new List<BookGroup>();
                    List<ListBookGroup> listBookGroup = context.ListBookGroups.Where(o => o.ProductId == productId).ToList();
                    foreach (ListBookGroup lbg in listBookGroup)
                    {
                        var bookGroup = context.BookGroups.Where(bg => bg.BookGroupId == lbg.BookGroupId).SingleOrDefault();
                        if (bookGroup != null)
                        {
                            result.Add(bookGroup);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<BookDetailsDTO> GetAllBookOfBookGroup(Guid bookListId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<BookDetailsDTO> result = new List<BookDetailsDTO>();
                    List<ListBookGroup> listBookGroup = context.ListBookGroups.Where(lbg => lbg.BookGroupId == bookListId).ToList();
                    foreach (ListBookGroup lbg in listBookGroup)
                    {
                        var book = GetBookDetailsById((Guid)lbg.ProductId);
                        if (book != null)
                        {
                            result.Add(book);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<BookDetailsDTO> GetAllBookOfBookGroupByBookId(Guid productId) //New
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<BookDetailsDTO> result = new List<BookDetailsDTO>();

                    // Fetch all BookGroup entities related to the productId
                    var bookGroups = GetAllBookGroupOfBook(productId).ToList();

                    // Fetch all ListBookGroup entities related to the fetched BookGroup entities
                    var listBookGroup = context.ListBookGroups
                        .Where(lbg => bookGroups.Select(bg => bg.BookGroupId).Contains(lbg.BookGroupId))
                        .ToList();

                    // Extract unique ProductIds from listBookGroup
                    var list = listBookGroup.Select(lbg => lbg.ProductId).Distinct().ToList();

                    // Fetch book details for each unique ProductId
                    foreach (var l in list)
                    {
                        var book = GetBookDetailsById((Guid)l);
                        if (book != null)
                        {
                            result.Add(book);
                        }

                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                // Log the exception or handle it in a way suitable for your application
                throw new Exception("An error occurred while retrieving book details: " + e.Message);
            }
        }


        public List<BookDetailsDTO> GetBookOfBookGroupByName(Guid bookListId, string? name) //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<BookDetailsDTO> result = new List<BookDetailsDTO>();
                    List<Book> book = new List<Book>();
                    List<ListBookGroup> listBookGroup = context.ListBookGroups.Where(lbg => lbg.BookGroupId == bookListId).ToList();
                    foreach (ListBookGroup lbg in listBookGroup)
                    {
                        if (name == null)
                        {
                            // If name is null, return all books associated with the bookListId
                            var books = context.Books.Where(b => b.ProductId == lbg.ProductId).ToList();
                            book.AddRange(books);
                        }
                        else
                        {
                            // If name is provided, filter by both productId and name
                            var books = context.Books.FirstOrDefault(b => b.ProductId == lbg.ProductId && b.Name.Contains(name));
                            if (books != null)
                            {
                                book.Add(books);
                            }
                        }
                    }
                    foreach (Book b in book)
                    {
                        BookDetailsDTO det = GetBookDetailsById(b.ProductId);
                        result.Add(det);
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public List<BookDetailsDTO> GetAllBookInInventory(Guid agencyId) //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<BookDetailsDTO> result = new List<BookDetailsDTO>();
                    List<Inventory> list = context.Inventories.Where(i => i.AgencyId == agencyId).ToList();
                    foreach (Inventory l in list)
                    {
                        var book = GetBookDetailsById(l.ProductId);
                        if (book != null)
                        {
                            result.Add(book);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // New ------------------------------------------------------------------------------------------------
        public List<Book> GetAllBookInInvent(Guid agencyId) //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<Book> result = new List<Book>();
                    List<Inventory> list = context.Inventories.Where(i => i.AgencyId == agencyId).ToList();
                    foreach (Inventory l in list)
                    {
                        var book = context.Books.Where(b => b.ProductId == l.ProductId).FirstOrDefault();
                        if (book != null)
                        {
                            result.Add(book);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<Book> GetAllBookInInventoryByType(Guid agencyId, string type)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<Book> result = new List<Book>();
                    List<Inventory> list = context.Inventories.Where(i => i.AgencyId == agencyId).ToList();
                    foreach (Inventory l in list)
                    {
                        var book = context.Books.Where(b => b.ProductId == l.ProductId).FirstOrDefault();
                        if (book != null && book.Type == type)
                        {
                            result.Add(book);
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<Book> GetAllBookInInventoryByCategory(Guid agencyId, string cate)  //Update
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<Book> result = new List<Book>();
                    List<Inventory> list = context.Inventories.Where(i => i.AgencyId == agencyId).ToList();
                    foreach (Inventory l in list)
                    {
                        var book = context.Books.Where(b => b.ProductId == l.ProductId).FirstOrDefault();
                        var category = context.Categories.Where(c => c.CateName == cate).FirstOrDefault();
                        var cateList = context.CategoryLists.Where(cl => cl.CategoryId == category.CateId && book.ProductId == cl.ProductId).FirstOrDefault();
                        if (book != null && category != null && cateList != null)
                        {
                            result.Add(book);
                        }

                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // End ------------------------------------------------------------------------------------------------


        public List<BookDetailsDTO> SearchBookInInventory(Guid agencyId, string name, string author, string category, string type, DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<BookDetailsDTO> result = new List<BookDetailsDTO>();
                    List<BookDetailsDTO> temp = new List<BookDetailsDTO>();
                    var inventories = context.Inventories.Where(i => i.AgencyId == agencyId).ToList();
                    foreach (var inventory in inventories)
                    {
                        var query = context.Books.Where(b => b.ProductId == inventory.ProductId);


                        if (!string.IsNullOrEmpty(name))
                        {
                            query = query.Where(b => b.Name.Contains(name));
                        }
                       
                        if (!string.IsNullOrEmpty(author))
                        {
                            query = query.Where(b => b.Author.Contains(author));
                        }

                        if (!string.IsNullOrEmpty(type))
                        {
                            query = query.Where(b => b.Type.Contains(type));
                        }

                        if (startDate != DateTime.MinValue && endDate != DateTime.MinValue)
                        {
                            query = query.Where(b => b.CreatedDate >= startDate && b.CreatedDate <= endDate);
                        }

                        var book = query.FirstOrDefault();
                        if (book != null)
                        {
                            var bookDetails = GetBookDetailsById(book.ProductId);
                            if (bookDetails != null)
                            {
                                temp.Add(bookDetails);
                            }
                        }
                    }

                    foreach (var det in temp)
                    {
                        if (!string.IsNullOrEmpty(category))
                        {
                            if(det.Category.Contains(category))
                            {
                                result.Add(det);
                            }
                            
                        }
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while fetching books with quantity in inventory.", e);
            }
        }




        public int GetTotalQuantityByAgencyId(Guid id)
        {
            try
            {
                int result = 0;
                List<Book> list = GetAllBookInInvent(id);
                using (var context = new AppDbContext())
                {
                    foreach (Book b in list)
                    {
                        Inventory? temp = context.Inventories.Where(i => i.ProductId == b.ProductId).FirstOrDefault();
                        if (temp != null)
                        {
                            result += temp.Quantity;
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<BookGroup> GetAllBookGroupsByAgency(Guid agencyId) //DATDQ
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    List<BookGroup> result = new List<BookGroup>();

                    // Find all ListBookGroup entries with the given AgencyId
                    List<BookGroup> listBookGroup = context.BookGroups
                        .Where(lbg => lbg.AgencyId == agencyId)
                        .ToList();

                    // Create a HashSet to store unique BookListIds
                    HashSet<Guid> uniqueBookGroupIds = new HashSet<Guid>();

                    // For each entry, retrieve the corresponding BookListing
                    foreach (BookGroup lbg in listBookGroup)
                    {
                        // Check if the BookGroupId is unique
                        if (!uniqueBookGroupIds.Contains(lbg.BookGroupId))
                        {
                            var bookGroup = context.BookGroups
                                .FirstOrDefault(bg => bg.BookGroupId == lbg.BookGroupId);

                            if (bookGroup != null)
                            {
                                result.Add(bookGroup);

                                // Add the BookGroupId to the HashSet
                                uniqueBookGroupIds.Add(lbg.BookGroupId);
                            }
                        }
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public List<BookGroup> GetBookGroupsByAgencyAndName(Guid agencyId, string name)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var query = context.BookGroups.Where(bg => bg.AgencyId == agencyId);

                    // If the name parameter is not null or empty, filter the results by name
                    if (!string.IsNullOrEmpty(name))
                    {
                        query = query.Where(bg => bg.BookGroupName.Contains(name));
                    }

                    // Retrieve the filtered BookGroups directly
                    List<BookGroup> result = query.ToList();

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occurred while retrieving BookGroups.", e);
            }
        }


        public bool IsBookAlreadyInBookGroup(Guid productId, Guid bookGroupId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    return context.ListBookGroups.Any(lbg => lbg.ProductId == productId && lbg.BookGroupId == bookGroupId);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public bool IsBookGroupValid(Guid agencyId, Guid id)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Retrieve the BookGroup with the given id
                    BookGroupDAO d = new BookGroupDAO();
                    BookGroup bg = d.GetBookGroupById(id);

                    // Check if the retrieved BookGroup belongs to the specified agency
                    if (bg != null)
                    {
                        return bg.AgencyId == agencyId;
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int RemoveBookFromBookGroup(Guid productId, Guid bookGroupId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    string insertQuery = $"delete from ListBookGroups where BookGroupId = @bookGroupId and  ProductId = @productId";

                    int result = context.Database.ExecuteSqlRaw(insertQuery,
                        new SqlParameter("@bookGroupId", bookGroupId),
                        new SqlParameter("@productId", productId));
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public int RemoveBookFromInventory(Guid productId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    string insertQuery = $"delete from Inventories where ProductId = @productId";

                    int result = context.Database.ExecuteSqlRaw(insertQuery,
                        new SqlParameter("@productId", productId));
                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public bool IsBookOwnedByAgency(Guid productId, Guid agencyId)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    Inventory invent = context.Inventories.FirstOrDefault(i => i.AgencyId == agencyId);

                    // Retrieve the book with the given productId and check if it belongs to the specified agency
                    Book book = context.Books.FirstOrDefault(b => b.ProductId == productId && b.ProductId == invent.ProductId);

                    // If the book is found and belongs to the specified agency, return true; otherwise, return false
                    return book != null;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public Guid GetBestSellerProductIdByNumberOfUnitSold(Guid agencyId)
        {
            try
            {
                List<BookDetailsDTO> books = GetAllBookInInventory(agencyId);
                Guid mostFrequentProductId = Guid.Empty;
                using (var context = new AppDbContext())
                {
                    mostFrequentProductId = context.Baskets
                       .Where(b => books.Select(book => book.ProductId).Contains(b.ProductId))
                       .GroupBy(b => b.ProductId)
                       .OrderByDescending(g => g.Count())
                       .Select(g => g.Key)
                       .FirstOrDefault();

                    return mostFrequentProductId;
                }
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the most frequent product ID.", e);
            }
        }

        public Guid GetBestSellerProductIdByNumberOfBookSold(Guid productId)
        {
            try
            {
                List<BookDetailsDTO> books = GetAllBookInInventory(productId);
                int best = 0;
                Guid bestSellerProductId = Guid.Empty;

                foreach (var book in books)
                {
                    using (var context = new AppDbContext())
                    {
                        OrderDAO o = new OrderDAO();
                        int compare = o.GetNumberOfBooksSoldByProductId(book.ProductId);
                        if (compare > best)
                        {
                            best = compare;
                            bestSellerProductId = book.ProductId;
                        }
                    }
                }

                return bestSellerProductId;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the best seller product ID.", e);
            }
        }

        public Guid GetBestSellerProductIdByRevenue(Guid productId)
        {
            try
            {
                List<BookDetailsDTO> books = GetAllBookInInventory(productId);
                Decimal best = 0;
                Guid bestSellerProductId = Guid.Empty;

                foreach (var book in books)
                {
                    using (var context = new AppDbContext())
                    {
                        OrderDAO o = new OrderDAO();
                        Decimal compare = o.GetRevenueByProductId(book.ProductId);
                        if (compare > best)
                        {
                            best = compare;
                            bestSellerProductId = book.ProductId;
                        }
                    }
                }

                return bestSellerProductId;
            }
            catch (Exception e)
            {
                // Handle the exception appropriately
                throw new Exception("Error occurred while getting the best seller product ID.", e);
            }
        }

        //-----------------------------------Book category------------------------------------------------//

        //-----------------------------------Book group-----------------------------------------------//



        public List<(Book book, int quantity)> GetAllBookInInventoryAndQuantity(Guid agencyId) //DATDQ
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var result = new List<(Book book, int quantity)>();
                    var inventories = context.Inventories.Where(i => i.AgencyId == agencyId).ToList();

                    foreach (var inventory in inventories)
                    {
                        var book = context.Books.FirstOrDefault(b => b.ProductId == inventory.ProductId);
                        if (book != null)
                        {
                            result.Add((book, inventory.Quantity));
                        }
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while fetching books with quantity in inventory.", e);
            }
        }

        public List<(Book book, int quantity)> GetAllBookInInventoryByName(Guid agencyId, string name) //DATDQ
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var result = new List<(Book book, int quantity)>();
                    var inventories = context.Inventories.Where(i => i.AgencyId == agencyId).ToList();

                    foreach (var inventory in inventories)
                    {
                        var book = context.Books.FirstOrDefault(b => b.ProductId == inventory.ProductId && b.Name.Contains(name));
                        if (book != null)
                        {
                            result.Add((book, inventory.Quantity));
                        }
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error occurred while fetching books with quantity in inventory.", e);
            }
        }

        public List<Book> SearchBooks(BookSearchCriteria criteria) //Fix cateid to string SONDB
        {
            using (var context = new AppDbContext())
            {
                var query = context.Books.AsQueryable();

                // Apply search criteria for book name
                if (!string.IsNullOrEmpty(criteria.Name))
                {
                    var lowerCaseName = criteria.Name.ToLower();
                    query = query.Where(b => b.Name.ToLower().Contains(lowerCaseName));
                }

                if (criteria.CategoryIds != null && criteria.CategoryIds.Any())
                {
                    // Convert string representations to Guid
                    var categoryGuids = criteria.CategoryIds
                        .Split(',') // Split comma-separated string into array of strings
                        .Distinct()
                        .Where(id => !string.IsNullOrEmpty(id)) // Filter out empty strings
                        .Select(id => Guid.TryParse(id, out Guid guid) ? guid : Guid.Empty)
                        .Where(guid => guid != Guid.Empty) // Filter out Guid.Empty
                        .ToList();

                    // Retrieve book IDs in specified categories only
                    var booksByCategories = context.CategoryLists
                        .Where(cl => categoryGuids.Contains(cl.CategoryId)) // Filter by input category IDs
                        .GroupBy(cl => cl.ProductId) // Group by BookId
                        .Where(group => group.Count() == categoryGuids.Count) // Ensure the book belongs to all specified categories
                        .Select(group => group.Key) // Select BookIds
                        .ToList();

                    // Filter query based on book IDs in specified categories
                    query = query.Where(b => booksByCategories.Contains(b.ProductId));
                }
                // Apply search criteria for book type
                if (!string.IsNullOrEmpty(criteria.Type))
                {
                    var lowerCaseType = criteria.Type.ToLower();
                    query = query.Where(b => b.Type.ToLower().Contains(lowerCaseType));
                }

                // Convert and apply search criteria for price range
                if (!string.IsNullOrEmpty(criteria.MinPrice) && decimal.TryParse(criteria.MinPrice, out decimal minPrice))
                {
                    query = query.Where(b => b.Price >= minPrice);
                }

                if (!string.IsNullOrEmpty(criteria.MaxPrice) && decimal.TryParse(criteria.MaxPrice, out decimal maxPrice))
                {
                    query = query.Where(b => b.Price <= maxPrice);
                }

                // Convert and apply search criteria for rating (5 stars or lower)
                if (!string.IsNullOrEmpty(criteria.OverRating) && double.TryParse(criteria.OverRating, out double minRating))
                {
                    query = query.Where(b => b.Rating.OverallRating >= minRating);
                }

                // Execute the query and return the results as a list
                return query.ToList();
            }
        }
        public async Task<List<Book>> GetBooksByAgencyId(Guid agencyId) //SONDB
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var inventories = await context.Inventories
                                               .Where(i => i.AgencyId == agencyId)
                                               .ToListAsync();

                    var bookIds = inventories.Select(i => i.Book.ProductId).Distinct();

                    var books = await context.Books
                                                .Where(b => bookIds.Contains(b.ProductId))
                                                .ToListAsync();

                    return books;
                }
            }

            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}

