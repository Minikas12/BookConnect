using BusinessObjects.Models.Ecom.Rating;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
	public class Book
	{
        [Key]
        public Guid ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Type { get; set; }
        public string? Author { get; set; }
        public string? BookDir { get; set; }
        public string? BackgroundDir { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? PublishDate { get; set; }

        public Guid? RatingId { get; set; }
        [ForeignKey("RatingId"), JsonIgnore]
        public virtual Rating? Rating { get; set; }

    }
}

