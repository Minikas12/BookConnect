using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Utils
{
	public class UserTargetedCategory
	{
		[Key]
		public Guid Id { get; set; }

		public Guid CategoryId { get; set; }
		[ForeignKey("CategoryId"), JsonIgnore]
		public Category Category { get; set; } = null!;

		public Guid UserId { get; set; }
        [ForeignKey("UserId"), JsonIgnore]
        public AppUser User { get; set; } = null!;
	}
}

