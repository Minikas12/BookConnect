using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
	public class Category
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid CateId { get; set; }
		public string CateName { get; set; } = null!;
		public string? ImageDir { get; set; } 
		public string? Description { get; set; }
		public bool IsSocialTag { get; set; } 
		public DateTime CreateDate { get; set; }
	}
}

