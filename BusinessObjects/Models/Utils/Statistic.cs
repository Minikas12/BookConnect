using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Models.E_com.Trading;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Utils
{
	public class Statistic
	{
		[Key]
		public Guid StatId { get; set; }
		public Guid? PostId { get; set; }
		public Guid? BookId { get; set; }
		public int View { get; set; }
		public int? Interested { get; set; }
		public int? Purchase { get; set; }
		public int Search { get; set; }
        public int? Hearts { get; set; }
		public DateTime  LastUpdate { get; set; }
		//public int Share { get; set; }

		[ForeignKey("PostId"), JsonIgnore]
		public Post? Post { get; set; }

        [ForeignKey("BookId"), JsonIgnore]
        public Book? Book { get; set; }
    }
}


