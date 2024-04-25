using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace BusinessObjects.Models.Utils;
using Newtonsoft.Json;
public class ListBookGroup
{
    [Key]
    public Guid ListId { get; set; }
    public Guid BookGroupId { get; set; }
    [ForeignKey("BookGroupId"), JsonIgnore]
    public virtual BookGroup? BookGroup { get; set; }

    public Guid? ProductId { get; set; }
    [ForeignKey("ProductId"), JsonIgnore]
    public virtual Book? Book { get; set; }
}
