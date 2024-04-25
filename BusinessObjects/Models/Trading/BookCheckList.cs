
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Trading
{
	public class BookCheckList
	{
		[Key]
		public Guid Id { get; set; }
		public Guid TradeDetailsId { get; set; }
		public string Target { get; set; } = null!;
		public string? BookOwnerUploadDir { get; set; }
		public string? MiddleUploadDir { get; set; }

		[ForeignKey("TradeDetailsId"), JsonIgnore]
		public TradeDetails TradeDetails { get; set; } = null!;
	}
}

