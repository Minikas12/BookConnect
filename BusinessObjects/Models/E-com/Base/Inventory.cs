using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace BusinessObjects.Models
{
	public class Inventory
	{
		[Key]
        public Guid InventoryId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifed { get; set; }
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId"), JsonIgnore]
        public virtual Book Book { get; set; } = null!;
        public Guid AgencyId { get; set; }
        [ForeignKey("AgencyId"), JsonIgnore]
		public virtual Agency Agency { get; set; } = null!;

    }
}

