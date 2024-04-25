using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Ecom.Rating
{
	public class Rating
	{
		[Key]
		public Guid RatingId { get; set; }
		public double OverallRating { get; set; }
		public Guid? RevieweeId { get; set; }
		public int TotalReview { get; set; }

		[ForeignKey("RevieweeId"), JsonIgnore]
		public AppUser? Reviewee { get; set; } = null!;
	}
}

