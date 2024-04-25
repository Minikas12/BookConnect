using BusinessObjects.Models.Ecom.Rating;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BusinessObjects.Models.E_com.Rating
{
    public class Reply
    {
        public Guid ReplyId { get; set; }
        public Guid RatingRecordId { get; set; }
        public Guid AgencyId { get; set; }

        public string? ReplyText { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool HasReplied { get; set; }

        [ForeignKey("RatingRecordId"), JsonIgnore]
        public virtual RatingRecord? RatingRecord { get; set; }

        [ForeignKey("AgencyId"), JsonIgnore]
        public virtual Agency? Agency { get; set; }
    }
}
