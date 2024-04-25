
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Utils
{
    public class BookGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid BookGroupId { get; set; }
        public string BookGroupName { get; set; } = null!;
        public string? ImageDir { get; set; } = null!;
        public string? Description { get; set; }
        public Guid AgencyId { get; set; }
        [ForeignKey("AgencyId"), JsonIgnore]
        public virtual Agency? Agency { get; set; }
    }
}
