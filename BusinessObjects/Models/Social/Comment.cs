using BusinessObjects.Models.E_com.Trading;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BusinessObjects.Models.Trading
{
    public class Comment
    {
        [Key]
        public Guid CommentId { get; set; }
        public Guid CommenterId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreateDate { get; set; } = DateTime.Now;

        [ForeignKey("CommenterId"), JsonIgnore]
        public virtual AppUser? AppUser { get; set; }
    }
}
