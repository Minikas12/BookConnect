using BusinessObjects.Models.E_com.Trading;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Text.Json.Serialization;

namespace BusinessObjects.Models.Trading
{
    public class PostInterester
    {
        [Key]
        public Guid PostInterestId { get; set; }
        public Guid InteresterId { get; set; }
        public Guid PostId { get; set; }
        public bool IsChosen { get; set; }
        public DateTime CreateDate { get; set; }

        [ForeignKey("PostId"), JsonIgnore]
        public virtual Post Post { get; set; } = null!;

        [ForeignKey("InteresterId"), JsonIgnore]
        public virtual AppUser AppUser { get; set; } = null!;
    }
}
