using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Models.E_com.Trading;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Trading
{
	public class CommentOfPost
	{
		[Key]
		public Guid Id { get; set; }
		public Guid PostId { get; set; }
		public Guid CommentId { get; set; }

		[ForeignKey("PostId"), JsonIgnore]
		public Post Post { get; set; } = null!;
		[ForeignKey("CommentId"), JsonIgnore]
		public Comment Comment { get; set; } = null!;
	}
}

