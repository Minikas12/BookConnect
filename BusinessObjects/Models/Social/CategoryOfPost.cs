using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Models.E_com.Trading;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Trading
{
	public class CategoryOfPost
	{
		[Key]
		public Guid Id { get; set; }
		public Guid CateId { get; set; }
		public Guid PostId { get; set; }

		[ForeignKey("CateId"), JsonIgnore]
		public Category Cate { get; set; } = null!;
        [ForeignKey("PostId"), JsonIgnore]
        public Post Post { get; set; } = null!;
	}
}

