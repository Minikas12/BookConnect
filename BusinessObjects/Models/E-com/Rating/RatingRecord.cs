using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Enums;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Ecom.Rating
{
	public class RatingRecord
	{
		[Key]
		public Guid RatingRecordId { get; set; }
		public Guid ReviewerId { get; set; }
		public Guid? RatingId { get; set; }
		public int RatingPoint { get; set; }
		public string? Comment { get; set; }
        public DateTime? CreatedDate { get; set; }

        [Column(TypeName = "varchar(20)")]
        public RatingType RatingType { get; set; }

		[ForeignKey("RatingId"), JsonIgnore]
		public Rating? Rating { get; set; } 

		[ForeignKey("ReviewerId"), JsonIgnore]
		public AppUser? Reviewer { get; set; } = null!;

        //[ForeignKey("RevieweeId"), JsonIgnore]
        //public AppUser? Reviewee { get; set; } = null!;
    }
}

