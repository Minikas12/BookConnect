using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Models.Ecom;
using Newtonsoft.Json;

namespace BusinessObjects.Models
{
    public class OrderItemStatus
    {
        [Key]
        public Guid OrderItemStatusId { get; set; }

        public string? DeliveryStatus { get; set; }

        public Guid BasketId { get; set; }

        [ForeignKey("BasketId"), JsonIgnore]
        public virtual Basket Basket { get; set; } = null!;

        public Guid AgencyId { get; set; }

        [ForeignKey("AgencyId"), JsonIgnore]
        public virtual Agency Agency { get; set; } = null!;

        public Guid OrderId { get; set; }

        [ForeignKey("OrderId"), JsonIgnore]
        public virtual Order Order { get; set; } = null!;
        public Guid BookId { get; set; }

        [ForeignKey("BookId"), JsonIgnore]
        public virtual Book Book { get; set; } = null!;
    }
}
