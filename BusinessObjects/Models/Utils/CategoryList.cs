using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Models.E_com.Trading;
using Newtonsoft.Json;

namespace BusinessObjects.Models.Utils
{
    public class CategoryList
    {
        [Key]
        public Guid CategoryListId { get; set; }
        
        public Guid CategoryId { get; set; }
        [ForeignKey("CategoryId"), JsonIgnore]
		public virtual Category? Category { get; set; }

        public Guid? ProductId { get; set; }
        [ForeignKey("ProductId"), JsonIgnore]
        public virtual Book? Book { get; set; }

        public Guid? PostId { get; set; }
        [ForeignKey("PostId"), JsonIgnore]
        public virtual Post? Post { get; set; }
    }
}

