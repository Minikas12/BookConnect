using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Models.E_com.Trading;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Trading
{
	public class HeartRecord
	{
		[Key]
		public Guid Id { get; set; }
		public Guid UserId { get; set; } //Person who heart post
		public Guid PostId { get; set; }

		[ForeignKey("UserId"), JsonIgnore]
		public AppUser User { get; set; } = null!;
        [ForeignKey("PostId"), JsonIgnore]
        public Post Post { get; set; } = null!;
    }
}

