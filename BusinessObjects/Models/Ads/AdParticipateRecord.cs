using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Ads
{
	public class AdParticipateRecord
    {
		[Key]
		public Guid RecordId {get; set;}
		public DateTime CreateDate { get; set; }
		public string? Type { get; set; } //view or click

        public Guid AdId { get; set; }
		[ForeignKey("AdId"), JsonIgnore]
        public Advertisement Advertisement { get; set; } = null!;

		public Guid UserId { get; set; }
        [ForeignKey("UserId"), JsonIgnore]
        public AppUser User { get; set; } = null!;
	}
}

