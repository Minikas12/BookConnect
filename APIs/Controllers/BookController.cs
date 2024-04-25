using BusinessObjects.DTO;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Mvc;
using APIs.Services.Interfaces;
using BusinessObjects.Models.Ecom.Rating;
using BusinessObjects.Models.Utils;
using APIs.Utils.Paging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace APIs.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ICloudinaryService _clouinaryService;
        private readonly IAccountService _accService;
        private readonly IInventoryService _inventService;
        private readonly ICategoryService _cateService;
        public BooksController(IBookService bookServices, ICloudinaryService cloudinaryService, IAccountService accService, IInventoryService inventService, ICategoryService cateService)
        {
            _bookService = bookServices;
            _clouinaryService = cloudinaryService;
            _accService = accService;
            _inventService = inventService;
            _cateService = cateService;
        }

        [HttpGet("search")]
        public IActionResult SearchProductsByName([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest("Search term cannot be empty.");
            }

            try
            {
                List<SEODTO> searchResult = _bookService.ListSEO(searchTerm);

                if (searchResult.Count == 0)
                {
                    return NotFound("No products found with the provided search term.");
                }

                return Ok(searchResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Internal Server Error with message
            }
        }


        [HttpGet("get-book-by-all")] //SONDB 
        public IActionResult GetBookByAll([FromQuery] BookSearchCriteria criteria, [FromQuery] PagingParams @params)
        {
            try
            {
                var booksearch = _bookService.GetAllBook(@params);
                var searchResults = _bookService.SearchBooks(criteria, @params);

                if (searchResults != null)
                {
                    var metadata = new
                    {
                        booksearch.TotalCount,
                        booksearch.PageSize,
                        booksearch.CurrentPage,
                        booksearch.TotalPages,
                        booksearch.HasNext,
                        booksearch.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(searchResults);
                }
                else
                {
                    return BadRequest("No book!!!");
                }
            }
            catch (Exception ex)
            {
                // Implement proper error handling with logging and informative messages
                return StatusCode(500, ex.Message); // Return internal server error for unexpected exceptions
            }
        }

        [HttpGet("get-book-by-quantity")]  //SONDB 
        public IActionResult GetBestBuyBook([FromQuery] PagingParams @params)
        {
            try
            {
                var booksell = _bookService.GetBooksSoldByQuantityDescendingWithinTime(@params);

                if (booksell != null)
                {
                    var metadata = new
                    {
                        booksell.TotalCount,
                        booksell.PageSize,
                        booksell.CurrentPage,
                        booksell.TotalPages,
                        booksell.HasNext,
                        booksell.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(booksell);
                }
                else return BadRequest("No book!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //--------------------------------------------- DATDQ -------------------------------------------------------//

        [HttpGet("get-all-book")]
        public IActionResult GetAllBook([FromQuery] PagingParams @params)
        {
            try
            {
                var result = _bookService.GetAllBook(@params);

                if (result != null)
                {
                    var metadata = new
                    {
                        result.TotalCount,
                        result.PageSize,
                        result.CurrentPage,
                        result.TotalPages,
                        result.HasNext,
                        result.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(result);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("get-product-by-id")]
        public IActionResult GetBookDetailsById(Guid bookId)
        {
            try
            {
                return Ok(_bookService.GetBookDetailsById(bookId));
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred: {e.Message}", e);
            }

        }

        [HttpPost, Authorize]
        [Route("AddBook")]
        public IActionResult AddNewBook([FromForm] BookDTO dto)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id not found for the given owner");
                }

                if (ModelState.IsValid)
                {
                    List<Guid> cates = new List<Guid>();
                    foreach (string name in dto.Category)
                    {
                        Guid cate = _cateService.GetCateIdByName(name);
                        if (cate != Guid.Empty)
                        {
                            cates.Add(cate);
                        }
                        else
                        {
                            string cateResponse = $"Did not have category named " + name + " in database";
                            return BadRequest(cateResponse);
                        }
                    }
                    if (cates.Count > 0)
                    {
                        Rating rating = new Rating
                        {
                            RatingId = Guid.NewGuid(),
                            OverallRating = 5
                        };
                        int changesR = _bookService.AddNewRating(rating);

                        string bookImgUrl = string.Empty;

                        // Check if BookImgs is not null and has images
                        if (dto.BookImg != null && dto.BookImg.Count > 0)
                        {
                            // Check if the number of images exceeds the limit of 6
                            if (dto.BookImg.Count > 6)
                            {
                                return BadRequest("Maximum allowed images is 6.");
                            }

                            foreach (var img in dto.BookImg)
                            {
                                // Upload each image
                                CloudinaryResponseDTO cloudRspBook = _clouinaryService.UploadImage(img, "Books/Image");

                                // Check if image upload was successful
                                if (cloudRspBook.StatusCode == 200 && cloudRspBook.Data != null)
                                {
                                    // Concatenate the image URL with a comma
                                    bookImgUrl += (string.IsNullOrEmpty(bookImgUrl) ? "" : ",") + cloudRspBook.Data;
                                }
                                else
                                {
                                    // Handle failed image upload
                                    return BadRequest("Failed to upload images.");
                                }
                            }
                        }

                        string t = "";
                        if (dto.Type == "Old")
                        {
                            t = "Old";
                        }
                        else if (dto.Type == "New")
                        {
                            t = "New";
                        }
                        else { return BadRequest("Need to select types is Old or New"); }
                        Book book = new Book
                        {
                            ProductId = Guid.NewGuid(),
                            Name = dto.Name,
                            BookDir = bookImgUrl,
                            Author = dto.Author,
                            Price = dto.Price,
                            Type = t,
                            CreatedDate = DateTime.Now,
                            PublishDate = dto.PublishDate,
                            Description = dto.Description,
                            RatingId = rating.RatingId
                        };

                        int changesB = _bookService.AddNewBook(book);

                        Inventory list = new Inventory
                        {
                            InventoryId = Guid.NewGuid(),
                            CreatedDate = DateTime.Now,
                            LastModifed = DateTime.Now,
                            ProductId = book.ProductId,
                            AgencyId = agencyId,
                            Quantity = dto.Quantity
                        };
                        int changesL = _inventService.AddNewInventory(list);

                        if (changesB > 0 && changesR > 0 && changesL > 0)
                        {
                            foreach (Guid id in cates)
                            {
                                CategoryList cateList = new CategoryList
                                {
                                    CategoryListId = Guid.NewGuid(),
                                    ProductId = book.ProductId,
                                    CategoryId = id
                                };
                                _bookService.AddBookToCategory(cateList);
                            }
                        }
                        string response = $"RatingId: {rating.RatingId}, \nProductId: {book.ProductId}, \nInventoryId: {list.InventoryId}";
                        return Ok(response);

                    }
                    return BadRequest("Model invalid!");
                }
                else
                {
                    return BadRequest("Add new book or rating failed.");
                }
            }
            catch (Exception e)
            {
                // Log the exception and return a user-friendly error message
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpPut, Authorize] //Update
        [Route("UpdateBook")]
        public IActionResult UpdateBook([FromForm] UBookDTO dto)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }


                if (!ModelState.IsValid)
                {
                    return BadRequest("Model invalid!");
                }

                List<Guid> cates = new List<Guid>();
                foreach (string name in dto.Category)
                {
                    Guid cate = _cateService.GetCateIdByName(name);
                    if (cate != Guid.Empty)
                    {
                        cates.Add(cate);
                    }
                    else
                    {
                        string cateResponse = $"Did not have category named " + name + " in database";
                        return BadRequest(cateResponse);
                    }
                }
                if (cates.Count > 0)
                {
                    List<Book> b = _bookService.GetAllBookInInvent(agencyId);
                    int check = 0;
                    foreach (var x in b)
                    {
                        if (dto.ProductId == x.ProductId)
                        {
                            check++;
                        }
                    }
                    if (check == 0)
                    {
                        return BadRequest("You do not have permission to update this book.");
                    }
                    Book existingBook = _bookService.GetBookById(dto.ProductId);
                    // Update book images
                    string bookImgUrl = UpdateBookImage(dto.BookImg, _bookService.GetOldBookImgPath(dto.ProductId), "Books/Image");
                    string backgroundImgUrl = UpdateBackgroundImage(dto.BackgroundImg, _bookService.GetOldBackgroundImgPath(dto.ProductId), "Book/Background");
                    // Create updated book object
                    string t = "";
                    if (dto.Type == "Old")
                    {
                        t = "Old";
                    }
                    else if (dto.Type == "New")
                    {
                        t = "New";
                    }
                    else { t = existingBook.Type; }
                    Book book = new Book
                    {
                        ProductId = dto.ProductId,
                        Name = dto?.Name ?? existingBook.Name,
                        BookDir = bookImgUrl,
                        BackgroundDir = backgroundImgUrl,
                        Author = dto?.Author ?? existingBook.Author,
                        Price = dto?.Price ?? existingBook.Price,
                        CreatedDate = existingBook.CreatedDate,
                        Type = t,
                        PublishDate = dto?.PublishDate ?? existingBook.PublishDate,
                        Description = dto?.Description ?? existingBook.Description,
                        RatingId = existingBook.RatingId
                    };

                    // Update the book details
                    int changes = _bookService.UpdateBook(book);
                    Inventory existingInventory = _inventService.GetInventoryById(agencyId, dto.ProductId);
                    if (changes > 0)
                    {

                        Inventory invent = new Inventory
                        {
                            InventoryId = existingInventory.InventoryId,
                            LastModifed = DateTime.Now,
                            CreatedDate = existingInventory.CreatedDate,
                            ProductId = existingInventory.ProductId,
                            AgencyId = agencyId,
                            Quantity = dto?.Quantity ?? existingInventory.Quantity
                        };

                        int Changes = _inventService.UpdateInventory(invent);

                        if (Changes > 0)
                        {
                            _cateService.DeleteCategoryList(dto.ProductId);
                            foreach (Guid id in cates)
                            {
                                CategoryList cateList = new CategoryList
                                {
                                    CategoryListId = Guid.NewGuid(),
                                    ProductId = dto.ProductId,
                                    CategoryId = id
                                };
                                _bookService.AddBookToCategory(cateList);
                            }
                            return Ok("Book updated successfully.");
                        }
                        else
                        {
                            return BadRequest("Failed to update book .");
                        }
                    }
                    else
                    {
                        return BadRequest("Failed to update the book.");
                    }
                }
                // Check if the book belongs to the same agency
                return BadRequest("No categories provided.");
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }




        private string UpdateBookImage(List<IFormFile> images, string oldImageUrls, string folder)
        {
            try
            {
                // Initialize new image URLs with old image URLs
                List<string> newImageUrls = oldImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                // Check if new images are provided
                if (images != null && images.Count > 0)
                {
                    // Check if the total number of images exceeds the limit
                    int newImageCount = newImageUrls.Count + images.Count;
                    if (newImageCount > 6)
                    {
                        // Calculate the number of images to remove
                        int imagesToRemoveCount = newImageCount - 6;

                        // Remove the first imagesToRemoveCount images from the list
                        newImageUrls.RemoveRange(0, imagesToRemoveCount);
                    }

                    // Upload each new image and add its URL to the list
                    foreach (var img in images)
                    {
                        CloudinaryResponseDTO cloudRspBook = _clouinaryService.UploadImage(img, folder);
                        if (cloudRspBook.StatusCode == 200 && cloudRspBook.Data != null)
                        {
                            newImageUrls.Add(cloudRspBook.Data);
                        }
                        else
                        {
                            // Handle failed image upload
                            throw new Exception("Failed to upload images.");
                        }
                    }
                }

                // Combine the new image URLs into a single string separated by commas
                string updatedImageUrls = string.Join(",", newImageUrls);

                return updatedImageUrls;
            }
            catch (Exception e)
            {
                throw new Exception("Error updating book images", e);
            }
        }




        private string UpdateBackgroundImage(IFormFile image, string oldImageUrl, string folder)
        {
            try
            {
                string newImageUrl = oldImageUrl;
                if (image != null)
                {
                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        _clouinaryService.DeleteImage(oldImageUrl, folder);
                    }
                    CloudinaryResponseDTO cloudResponse = _clouinaryService.UploadImage(image, folder);
                    if (cloudResponse.StatusCode == 200 && cloudResponse.Data != null)
                    {
                        newImageUrl = cloudResponse.Data;
                    }
                }
                return newImageUrl;
            }
            catch (Exception e)
            {
                throw new Exception("Error updating book image", e);
            }
        }


        [HttpDelete, Authorize]
        [Route("DeleteBook")]
        public IActionResult DeleteBook(Guid productId)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                // Check if the book belongs to the same agency
                List<Book> b = _bookService.GetAllBookInInvent(agencyId);
                int check = 0;
                foreach (var x in b)
                {
                    if (productId == x.ProductId)
                    {
                        check++;
                    }
                }
                if (check == 0)
                {
                    return BadRequest("You do not have permission to update this book.");
                }
                // Remove the book from the inventories
                int Changes = _bookService.DeleteBook(productId);

                if (Changes > 0)
                {
                    return Ok("Delete successfully.");
                }
                else
                {
                    return BadRequest("Failed to delete book .");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        //-----------------------------------Book category------------------------------------------------//

        [HttpDelete("category/remove-book-from-category")]
        public IActionResult RemoveBookFromCate(Guid bookId, Guid cateId)
        {
            try
            {
                int changes = _bookService.RemoveBookFromCate(bookId, cateId);
                IActionResult result = (changes > 0) ? Ok("Successful!") : BadRequest("remove fail!");
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("category/get-all-cate-of-book")]
        public IActionResult GetAllCategoryOfBook(Guid bookId, [FromQuery] PagingParams @params)
        {
            try
            {
                return Ok(_bookService.GetAllCategoryOfBook(bookId, @params));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        //-----------------------------------Book Group------------------------------------------------//
        [HttpPost, Authorize]
        [Route("BookGroup/AddBookToBookGroup")]
        public IActionResult AddBookToBookGroup(AddBookToBookGroupDTO dto)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }


                if (!ModelState.IsValid)
                {
                    // Handle case when ModelState is invalid
                    return BadRequest(ModelState);
                }

                List<Guid> validBookGroups = new List<Guid>();
                foreach (Guid id in dto.BookGroupIds)
                {
                    if (!_bookService.IsBookAlreadyInBookGroup(dto.ProductId, id) && _bookService.IsBookGroupValid(agencyId, id))
                    {
                        validBookGroups.Add(id);
                    }
                }

                if (validBookGroups.Count == 0)
                {
                    // Handle case when all books are already in the list(s)
                    return Ok("This book is already in these/this list(s) or BookGroupId is not valid for you");
                }

                int changes = 0;
                foreach (Guid id in validBookGroups)
                {
                    ListBookGroup bg = new ListBookGroup
                    {
                        ListId = Guid.NewGuid(),
                        ProductId = dto.ProductId,
                        BookGroupId = id
                    };
                    changes += _bookService.AddBookToBookGroup(bg);
                }

                if (changes > 0)
                {
                    // Return success message
                    return Ok("Successfully added books to book group");
                }
                else
                {
                    // Handle case when no changes were made
                    return BadRequest("Failed to add books to book group");
                }
            }
            catch (Exception e)
            {
                // Handle exceptions
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        [HttpGet("BookGroup/GetAllBookGroupOfBook")]
        public IActionResult GetAllBookGroupOfBook(Guid productId, [FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookService.GetAllBookGroupOfBook(productId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);



            }
        }

        [HttpGet("BookGroup/GetAllBookOfBookGroup")]
        public IActionResult GetAllBookOfBookGroup(Guid bookGroupId, [FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookService.GetAllBookOfBookGroup(bookGroupId, @params);

                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("BookGroup/GetAllBookOfBookGroupByBookId")] //New
        public IActionResult GetAllBookOfBookGroupByBookId(Guid bookId, [FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookService.GetAllBookOfBookGroupByBookId(bookId, @params);

                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("BookGroup/GetBookOfBookGroupByName")]
        public IActionResult GetBookOfBookGroupByName(Guid bookGroupId, string? name, [FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookService.GetBookOfBookGroupByName(bookGroupId, name, @params);

                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet("BookGroup/FindAllBookGroupsByAgency")]
        public IActionResult GetAllBookGroupsByAgencyAdmin(Guid agencyId, [FromQuery] PagingParams @params)
        {
            try
            {
                var bookGroups = _bookService.GetAllBookGroupsByAgency(agencyId, @params);
                if (bookGroups != null)
                {
                    var metadata = new
                    {
                        bookGroups.TotalCount,
                        bookGroups.PageSize,
                        bookGroups.CurrentPage,
                        bookGroups.TotalPages,
                        bookGroups.HasNext,
                        bookGroups.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(bookGroups);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        [HttpGet, Authorize]
        [Route("BookGroup/GetAllBookGroupsForAgency")]
        public IActionResult GetBookGroupsByAgency([FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                var bookGroups = _bookService.GetAllBookGroupsByAgency(agencyId, @params);
                if (bookGroups != null)
                {
                    var metadata = new
                    {
                        bookGroups.TotalCount,
                        bookGroups.PageSize,
                        bookGroups.CurrentPage,
                        bookGroups.TotalPages,
                        bookGroups.HasNext,
                        bookGroups.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(bookGroups);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        [HttpGet, Authorize]
        [Route("BookGroup/GetBookGroupsForAgencyAndName")]
        public IActionResult GetBookGroupsByAgencyAndName(string? name, [FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                PagedList<BookGroup> bookGroups;
                if (name != null)
                {
                    bookGroups = _bookService.GetBookGroupsByAgencyAndName(agencyId, name, @params);
                }
                else
                {
                    bookGroups = _bookService.GetAllBookGroupsByAgency(agencyId, @params);
                }

                if (bookGroups != null)
                {
                    var metadata = new
                    {
                        bookGroups.TotalCount,
                        bookGroups.PageSize,
                        bookGroups.CurrentPage,
                        bookGroups.TotalPages,
                        bookGroups.HasNext,
                        bookGroups.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(bookGroups);
                }
                else
                {
                    return NotFound("No book groups found.");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }


        [HttpDelete, Authorize]
        [Route("BookGroup/RemoveBookFromBookGroup")]
        public IActionResult RemoveBookFromBookGroup(Guid productId, Guid bookGroupId)
        {
            try
            {
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id not found for the given owner");
                }
                int changes = _bookService.RemoveBookFromBookGroup(productId, bookGroupId);
                IActionResult result = (changes > 0) ? Ok("Successful!") : BadRequest("remove fail!");
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [HttpGet]
        [Route("SellerManager/GetAllBookInInventoryByUserId")] //Update
        public IActionResult GetAllBookInInventoryByUserId(Guid userId, [FromQuery] PagingParams @params)
        {
            try
            {
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                var chapters = _bookService.GetAllBookInInventory(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }


        [HttpGet]
        [Route("SellerManager/GetAllBookInInventoryByAgencyId")]
        public IActionResult GetAllBookInInventoryByAgencyId(Guid agencyId, [FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookService.GetAllBookInInventory(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }

        [HttpGet, Authorize]
        [Route("SellerManager/GetListBestSellerProductIdByNumberOfBookSold")]
        public IActionResult GetListBestSellerProductIdByNumberOfBookSold([FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                var chapters = _bookService.GetBestSellerProductIdByNumberOfBookSold(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }

        [HttpGet]
        [Route("SellerManager/GetListBestSellerProductIdByNumberOfBookSoldAndAgencyId")]
        public IActionResult GetListBestSellerProductIdByNumberOfBookSoldAndAgencyId(Guid agencyId, [FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookService.GetBestSellerProductIdByNumberOfBookSold(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }

        [HttpGet, Authorize]
        [Route("SellerManager/GetListBestSellerProductIdByRevenue")]
        public IActionResult GetListBestSellerProductIdByRevenue([FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                var chapters = _bookService.GetBestSellerProductIdByRevenue(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }


        [HttpGet]
        [Route("SellerManager/GetListBestSellerProductIdByRevenueAndAgencyId")]
        public IActionResult GetListBestSellerProductIdByRevenueAndAgencyId(Guid agencyId, [FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookService.GetBestSellerProductIdByRevenue(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }

        [HttpGet, Authorize]
        [Route("SellerManager/GetListBestSellerProductIdByUnitSold")]
        public IActionResult GetListBestSellerProductIdByUnitSold([FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                var chapters = _bookService.GetBestSellerProductIdByNumberOfUnitSold(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }

        [HttpGet]
        [Route("SellerManager/GetListBestSellerProductIdByUnitSoldAndAgencyId")]
        public IActionResult GetListBestSellerProductIdByUnitSoldAndAgencyId(Guid agencyId, [FromQuery] PagingParams @params)
        {
            try
            {
                var chapters = _bookService.GetBestSellerProductIdByNumberOfUnitSold(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }


        [HttpGet, Authorize]
        [Route("SellerManager/GetAllBookInInventory")]
        public IActionResult GetAllBookInInventory([FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                var chapters = _bookService.GetAllBookInInventory(agencyId, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }

        [HttpGet, Authorize] //Update
        [Route("SellerManager/SearchBookInInventory")]
        public IActionResult SearchBookInInventory(string? name, string? author, string? cate, string? type, DateTime startDate, DateTime endDate, [FromQuery] PagingParams @params)
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }
                var chapters = _bookService.SearchBookInInventory(agencyId, name, author, cate, type, startDate, endDate, @params);
                if (chapters != null)
                {
                    var metadata = new
                    {
                        chapters.TotalCount,
                        chapters.PageSize,
                        chapters.CurrentPage,
                        chapters.TotalPages,
                        chapters.HasNext,
                        chapters.HasPrevious
                    };
                    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
                    return Ok(chapters);
                }
                else return BadRequest("No chapter!!!");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }

        [HttpGet, Authorize]
        [Route("SellerManager/GetTotalQuantityInInventory")]
        public IActionResult GetTotalQuantityByAgencyId()
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                int c = _bookService.GetTotalQuantityByAgencyId(agencyId);
                if (c > 0)
                {
                    return Ok(c);
                }
                else
                {
                    return BadRequest("Get total quantity err");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);

            }
        }


        [HttpGet, Authorize]
        [Route("SellerManager/GetBestSellerBookOfAgencyByNumberOfUnitSold")]
        public IActionResult GetBestSellerBookOfAgencyByNumberOfUnitSold()
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                Guid bestSell = _bookService.GetBestSellerProductIdByNumberOfUnitSold(agencyId);
                if (bestSell != Guid.Empty)
                {
                    var book = _bookService.GetBookDetailsById(bestSell);
                    if (book != null)
                    {
                        return Ok(book);
                    }
                    else
                    {
                        return BadRequest("Can't get the best seller book detail !");
                    }
                }
                else
                {
                    return BadRequest("Error to get best seller book Id !");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


        [HttpGet, Authorize]
        [Route("SellerManager/GetBestSellerBookOfAgencyByNumberOfBookSold")]
        public IActionResult GetBestSellerBookOfAgencyByNumberOfBookSold()
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                Guid bestSell = _bookService.GetBestSellerProductIdByNumberOfBookSold(agencyId);
                if (bestSell != Guid.Empty)
                {
                    var book = _bookService.GetBookDetailsById(bestSell);
                    if (book != null)
                    {
                        return Ok(book);
                    }
                    else
                    {
                        return BadRequest("Can't get the best seller book detail !");
                    }
                }
                else
                {
                    return BadRequest("Error to get best seller book Id !");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet, Authorize]
        [Route("SellerManager/GetBestSellerBookOfAgencyByRevenue")]
        public IActionResult GetBestSellerBookOfAgencyByRevenue()
        {
            try
            {
                // Check if UserId is available in session
                var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "userId");
                if (userIdClaim == null)
                {
                    return BadRequest("Owner Id not found in session");
                }

                var userId = Guid.Parse(userIdClaim.Value);
                Guid agencyId = _accService.GetAgencyId(userId);

                if (agencyId == Guid.Empty)
                {
                    return BadRequest("Agency Id is not found for the given owner");
                }

                Guid bestSell = _bookService.GetBestSellerProductIdByRevenue(agencyId);
                if (bestSell != Guid.Empty)
                {
                    var book = _bookService.GetBookDetailsById(bestSell);
                    if (book != null)
                    {
                        return Ok(book);
                    }
                    else
                    {
                        return BadRequest("Can't get the best seller book detail !");
                    }
                }
                else
                {
                    return BadRequest("Error to get best seller book Id !");
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        //--------------------------------------------- END DATDQ -------------------------------------------------------//


    }
}
