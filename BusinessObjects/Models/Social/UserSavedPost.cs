using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BusinessObjects.Models.E_com.Trading;

namespace BusinessObjects.Models.Social
{
	public class UserSavedPost
	{
		[Key]
		public Guid Id { get; set; }

		public Guid PostId { get; set; }
		[ForeignKey("PostId"), JsonIgnore]
		public Post Post { get; set; } = null!;

		public Guid UserId { get; set; }
		[ForeignKey("UserId"), JsonIgnore]
		public AppUser AppUser { get; set; } = null!;
	}
}

