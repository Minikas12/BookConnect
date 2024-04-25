using Microsoft.AspNetCore.Http;


namespace BusinessObjects.DTO
{
    public class SEODTO
    {
        public Guid BookId { get; set; }
        public string Title { get; set; }
    }
    public class BookSearchCriteria
    {
        public string? Name { get; set; }
        public string? CategoryIds { get; set; }
        public string? Type { get; set; }

        public string? MinPrice { get; set; } // Make nullable
        public string? MaxPrice { get; set; } // Make nullable
        public string? OverRating { get; set; }
    }

    // ----------------------------------DATDQ-----------------------------------------------//
    public class BookDetailsDTO
    {

        public Guid ProductId { get; set; }
        public string? Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Author { get; set; }
        public decimal? Price { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? PublishDate { get; set; }
        public string? Type { get; set; }
        public int? Stock { get; set; }
        public int? NumberOfBookSold { get; set; }
        public int? NumberOfUnitSold { get; set; }
        public Decimal? BookRevenue { get; set; }
        public string? BookImg { get; set; }
        public string? BackgroundImg { get; set; }
        public List<string>? Category { get; set; }
        public double? Rating { get; set; }
        public Guid RatingId { get; set; }  ///ADD NEW
        public Guid UserId { get; set; }
        public Guid AgencyId { get; set; }
        public string AgencyName { get; set; } = null!;

    }
    public class BookDTO
    {
        public Guid? ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? PublishDate { get; set; }

        public string? Type { get; set; }
        public string Author { get; set; } = null!;
        public List<IFormFile>? BookImg { get; set; }
        public IFormFile? BackgroundImg { get; set; }
        public int Quantity { get; set; }
        public List<string> Category { get; set; } = null!;
    }
    public class AddBookToCateDTO
    {
        public Guid ProductId { get; set; }
        public List<Guid> CateIds { get; set; } = null!;
    }
    public class UBookDTO //Update
    {
        public Guid ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public DateTime? PublishDate { get; set; }
        public string? Type { get; set; }
        public string? Author { get; set; }
        public List<IFormFile>? BookImg { get; set; } //Update
        public IFormFile? BackgroundImg { get; set; }
        public int? Quantity { get; set; }
        public List<string> Category { get; set; } = null!;
    }

    public class AddBookToBookGroupDTO
    {
        public Guid ProductId { get; set; }
        public List<Guid> BookGroupIds { get; set; } = null!;
    }
    // ----------------------------------END DATDQ-----------------------------------------------//
   
}


